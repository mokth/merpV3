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
			string pathToDatabase = ((GlobalvarsApp)Application.Context).DATABASE_PATH;
			string userID = ((GlobalvarsApp)Application.Context).USERID_CODE;
			text = "";
			errMsg = "";
			bool isPrinted = false;
			text =GetCreditNoteText_Template("creditnote.vm",pathToDatabase,userID, cn, list); //Get from template
			if (string.IsNullOrEmpty (text)) {
				GetCreditNoteText (cn, list);
			}

			IPrintToDevice device = PrintDeviceManager.GetPrintingDevice<BlueToothDeviceHelper> ();
			device.SetCallingActivity (callingActivity);
			device.SetIsPrintCompLogo (iSPrintCompLogo ());
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
			string pathToDatabase = ((GlobalvarsApp)Application.Context).DATABASE_PATH;
			Invoice inv = null;
			IsfoundInvoice = PrintCNInvoice (cn,ref invTtlAmt, ref invTtlTax);
			if (!IsfoundInvoice) {
				prtcompHeader.PrintCompHeader (ref text);
				prtCustHeader.PrintCustomer (ref text, cn.custcode);
			}else 	inv = DataHelper.GetInvoice (pathToDatabase, cn.invno);

			prtHeader.PrintCNHeader (ref text, cn);

			foreach(CNNoteDtls itm in list)
			{
				count+=1;
				dline =dline+prtDetail.PrintCNDetail (itm,count);
				ttlAmt = ttlAmt+ itm.netamount;
				ttltax = ttltax+itm.tax;
			}
			text += dline;
			if (!IsfoundInvoice) {
				if (cn.trxtype=="CASH")
					prtTotal.PrintTotalAjust (ref text, ttlAmt, ttltax);
				else prtTotal.PrintTotal (ref text, ttlAmt, ttltax);
			} else {
				if (inv.trxtype=="CASH")
					prtTotal.PrintTotalAjust (ref text, ttlAmt, ttltax);
				else prtTotal.PrintTotal (ref text, ttlAmt, ttltax);
			}

			prtTaxSummary.PrintCNTaxSumm(ref text,list );

			prtFooter.PrintFooter (ref text);
			if (IsfoundInvoice)
			{
				text += "\nTHANK YOU\n\n";
				if (inv.trxtype=="CASH")
				 	  prtTotal.PrintTotalAjust (ref text,ttlAmt, ttltax, invTtlAmt,invTtlTax);	
				else prtTotal.PrintTotal (ref text,ttlAmt, ttltax, invTtlAmt,invTtlTax);	
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
			prtCustHeader.PrintCustomerInv (ref text, inv);
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
			if (inv.trxtype=="CASH")
				prtTotal.PrintTotalAjust (ref text, ttlAmt, ttltax);
			else prtTotal.PrintTotal (ref text, ttlAmt, ttltax);
			prtTaxSummary.PrintTaxSumm (ref text, list);
			text += "\n\n\n";
		}

	}
}

