using System;
using System.Text;

namespace wincom.mobile.erp
{
	public class PrintPCLSumm:PrintSummary
	{
		internal override bool Print()
		{
			text = "";
			errMsg = "";
			bool isPrinted = false;
			text = GetInvoiceSumm (printDate1, printDate2);
			text = text.Replace ('\n', '\r');
			TCPHPPCLHelper device = new TCPHPPCLHelper();
			//IPrintToDevice device = PrintDeviceManager.GetPrintingDevice<BlueToothDeviceHelper> ();
			device.SetCallingActivity (callingActivity);
			bool isReady = device.StartPrint (text, noOfCopy, ref errMsg);
			if (isReady) {
				byte[] charfont = Encoding.ASCII.GetBytes(text);
				device.mmOutputStream.Write(charfont, 0, charfont.Length);
				device.Close ();
			}


			return isPrinted;
		}
	}
}

