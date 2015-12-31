using System;
using Android.App;

namespace wincom.mobile.erp
{
	
	public class PrintInvoice_NTAX:PrintDocumentBase,IPrintDocument
	{
		Invoice inv;
		InvoiceDtls[] list;
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
			inv = (Invoice)doc;
		}

		public void SetDocumentDtls (object docdtls)
		{
			list = (InvoiceDtls[])docdtls;
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
			string pathToDatabase = ((GlobalvarsApp)Application.Context).DATABASE_PATH;
			string userID = ((GlobalvarsApp)Application.Context).USERID_CODE;
			text =GetInvoiceText_Template("invoiceNTax.vm",pathToDatabase,userID, inv, list); //Get from template
			if (string.IsNullOrEmpty (text)) {
				GetInvoiceText (inv, list); //take defaul is template is null
			}
			IPrintToDevice device = PrintDeviceManager.GetPrintingDevice<BlueToothDeviceHelper> ();
			device.SetCallingActivity (callingActivity);
			device.SetIsPrintCompLogo (iSPrintCompLogo ());
			isPrinted = device.StartPrint (text, noOfCopy, ref errMsg);

			return isPrinted;
		}

		private void GetInvoiceText (Invoice inv, InvoiceDtls[] list)
		{
			prtcompHeader.PrintCompHeader (ref text);
			prtCustHeader.PrintCustomerInv (ref text, inv);
			prtHeader.PrintHeader_NTax (ref text, inv);
			string dline = "";
			double ttlAmt = 0;
			double ttltax = 0;
			int count = 0;
			foreach (InvoiceDtls itm in list) {
				count += 1;
				dline = dline + prtDetail.PrintDetail_NTax (itm, count);
				ttlAmt = ttlAmt + itm.netamount;
				ttltax = ttltax + itm.tax;
			}
			text += dline;
			prtTotal.PrintTotal_NTax (ref text, ttlAmt, ttltax);
			prtFooter.PrintFooter (ref text);
			text += "\nTHANK YOU\n\n\n\n\n\n\n\n";
		}

	}
}

