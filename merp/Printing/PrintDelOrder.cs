using System;
using Android.App;

namespace wincom.mobile.erp
{
	public class PrintDelOrder:PrintDocumentBase,IPrintDocument
	{
		DelOrder delOrder;
		DelOrderDtls[] list;
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
			delOrder = (DelOrder)doc;
		}

		public void SetDocumentDtls (object docdtls)
		{
			list = (DelOrderDtls[])docdtls;
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
			GetDelOrderText (delOrder, list);
			IPrintToDevice device = PrintDeviceManager.GetPrintingDevice<BlueToothDeviceHelper> ();
			device.SetCallingActivity (callingActivity);
			device.SetIsPrintCompLogo (iSPrintCompLogo ());
			isPrinted = device.StartPrint (text, noOfCopy, ref errMsg);

			return isPrinted;
		}

		private void GetDelOrderText (DelOrder delOrder, DelOrderDtls[] list)
		{
			prtcompHeader.PrintCompHeader (ref text);
			prtCustHeader.PrintCustomer (ref text, delOrder.custcode,"DELIVER TO");
			prtHeader.PrintDOHeader (ref text, delOrder);
			string dline = "";
			double ttlAmt = 0;
			double ttltax = 0;
			int count = 0;
			foreach (DelOrderDtls itm in list) {
				count += 1;
				dline = dline + prtDetail.PrintDODetail (itm, count);
				ttlAmt = ttlAmt + itm.qty;
			}
			text += dline;
			prtTotal.PrintDOTotal(ref text, ttlAmt);
			prtFooter.PrintFooter (ref text);
			text += "\nTHANK YOU\n\n\n\n\n\n\n\n";
		}

	}
}

