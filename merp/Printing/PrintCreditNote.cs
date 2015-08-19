using System;
using System.Linq;
using Android.App;

namespace wincom.mobile.erp
{
	public class PrintCreditNote:PrintDocumentBase,IPrintDocument
	{
		CNNote cn;
		CNNoteDtls[] list;
		int noOfCopy=1;
		Activity callingActivity;

		public void SetCallingActivity (Activity activity)
		{
			callingActivity = activity;
		}

		public bool StartPrint ()
		{
			return Print ();
		}

		public void SetDocument (object doc)
		{
			cn = (CNNote)doc;
		}

		public void SetDocumentDtls (object docdtls)
		{
			list = (CNNoteDtls[])docdtls;
		}

		public void SetNoOfCopy (int noofcopy)
		{
			noOfCopy = noofcopy;
		}


		public void SetExtraPara (System.Collections.Hashtable para)
		{
			extrapara = para;
		}

		public string GetErrMsg()
		{
			return errMsg;
		}

		private bool Print()
		{
			text = "";
			errMsg = "";
			bool isPrinted = false;
			GetCreditNoteText (cn, list);
			IPrintToDevice device = PrintDeviceManager.GetPrintingDevice<BlueToothDeviceHelper> ();
			device.SetCallingActivity (callingActivity);
			isPrinted = device.StartPrint (text, noOfCopy, ref errMsg);

			return isPrinted;
		}

		private void GetCreditNoteText (CNNote cn, CNNoteDtls[] list)
		{
			string dline="";
			int count =0;
			double ttlAmt = 0;
			double ttltax = 0;
			double invTtlAmt =0;
			double invTtlTax =0;
			bool IsfoundInvoice = false;
			IsfoundInvoice = PrintCNInvoice (cn,ref invTtlAmt, ref invTtlTax);
			if (!IsfoundInvoice) {
				prtcompHeader.PrintCompHeader (ref text);
				prtCustHeader.PrintCustomer (ref text, cn.custcode);
			}

			prtHeader.PrintCNHeader (ref text, cn);

			foreach(CNNoteDtls itm in list)
			{
				count+=1;
				dline =dline+prtDetail.PrintCNDetail (itm,count);
				ttlAmt = ttlAmt+ itm.netamount;
				ttltax = ttltax+itm.tax;
			}
			text += dline;
		 	prtTotal.PrintTotal (ref text,ttlAmt,ttltax);

			prtTaxSummary.PrintCNTaxSumm(ref text,list );
			prtFooter.PrintFooter (ref text);
			if (IsfoundInvoice)
			{
				text += "\nTHANK YOU\n\n";
				prtTotal.PrintTotal (ref text,ttlAmt, ttltax, invTtlAmt,invTtlTax);	
				text += "\n\n\n\n\n\n\n\n";
			}else text += "\nTHANK YOUn\n\n\n\n\n\n\n";
		}

		private bool PrintCNInvoice(CNNote cn,ref double ttlAmt,ref double ttltax)
		{
			bool IsfoundInvoice =false;
			InvoiceDtls[] list =null;
			Invoice inv=null;
			string pathToDatabase = ((GlobalvarsApp)Application.Context).DATABASE_PATH;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)){
				var lsinv= db.Table<Invoice> ().Where (x => x.invno==cn.invno).ToList<Invoice>();
				if (lsinv.Count > 0) {
					IsfoundInvoice =true;
					inv = lsinv [0];
					var ls = db.Table<InvoiceDtls> ().Where (x => x.invno == cn.invno).ToList<InvoiceDtls> ();
					list = new InvoiceDtls[ls.Count];
					ls.CopyTo (list);
				}
			}


			if (inv != null) {
				GetInvoiceText (inv, list);
				foreach(InvoiceDtls itm in list)
				{
					ttlAmt = ttlAmt+ itm.netamount;
					ttltax = ttltax+itm.tax;
				}
			}

			return IsfoundInvoice;
		}
	
		private void GetInvoiceText (Invoice inv, InvoiceDtls[] list)
		{
			prtcompHeader.PrintCompHeader (ref text);
			prtCustHeader.PrintCustomer (ref text, inv.custcode);
			prtHeader.PrintHeader (ref text, inv);
			string dline = "";
			double ttlAmt = 0;
			double ttltax = 0;
			int count = 0;
			foreach (InvoiceDtls itm in list) {
				count += 1;
				dline = dline + prtDetail.PrintDetail (itm, count);
				ttlAmt = ttlAmt + itm.netamount;
				ttltax = ttltax + itm.tax;
			}
			text += dline;
			prtTotal.PrintTotal (ref text, ttlAmt, ttltax);
			prtTaxSummary.PrintTaxSumm (ref text, list);
			text += "\n\n\n";
		}

	}
}

