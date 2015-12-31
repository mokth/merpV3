using System;
using Android.App;
using System.IO;
using System.Text;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using NVelocity;
using NVelocity.App;
using System.Collections.Generic;

namespace wincom.mobile.erp
{
	
	public class PrintInvoice:PrintDocumentBase,IPrintDocument
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
			string pathToDatabase = ((GlobalvarsApp)Application.Context).DATABASE_PATH;
			string userID = ((GlobalvarsApp)Application.Context).USERID_CODE;
			text = "";
			errMsg = "";
			bool isPrinted = false;
			text =GetInvoiceText_Template("invoice.vm",pathToDatabase,userID, inv, list); //Get from template
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
			prtFooter.PrintFooter (ref text);
			text += "\nTHANK YOU\n\n\n\n\n\n\n\n";
		}

	}
}

