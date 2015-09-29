using System;
using Android.App;

namespace wincom.mobile.erp
{
	
	public class PrintSalesOrder:PrintDocumentBase,IPrintDocument
	{
		SaleOrder so;
		SaleOrderDtls[] list;
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
			so = (SaleOrder)doc;
		}

		public void SetDocumentDtls (object docdtls)
		{
			list = (SaleOrderDtls[])docdtls;
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
			GetSaleOrderText (so, list);
			IPrintToDevice device = PrintDeviceManager.GetPrintingDevice<BlueToothDeviceHelper> ();
			device.SetCallingActivity (callingActivity);
			device.SetIsPrintCompLogo (iSPrintCompLogo ());
			isPrinted = device.StartPrint (text, noOfCopy, ref errMsg);

			return isPrinted;
		}

		private void GetSaleOrderText (SaleOrder so, SaleOrderDtls[] list)
		{
			//if (string.IsNullOrEmpty(so.billTo))
				 prtcompHeader.PrintCompHeader (ref text);
			//else prtcompHeader.PrintCustomerHeader(ref text,so.billTo);

			prtCustHeader.PrintCustomer (ref text, so.custcode);
			prtHeader.PrintSOHeader (ref text, so);
			string dline = "";
			double ttlAmt = 0;
			double ttltax = 0;
			int count = 0;
			foreach (SaleOrderDtls itm in list) {
				count += 1;
				dline = dline + prtDetail.PrintSODetail (itm, count);
				ttlAmt = ttlAmt + itm.netamount;
				ttltax = ttltax + itm.tax;
			}
			text += dline;
			prtTotal.PrintTotal (ref text, ttlAmt, ttltax);
			prtTaxSummary.PrintSOTaxSumm (ref text, list);
			prtFooter.PrintFooter (ref text);
			text += "\nTHANK YOU\n\n\n\n\n\n\n\n";
		}

	}
}

