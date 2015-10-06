using System;
using Android.Bluetooth;
using Android.App;
using Android.Widget;
using Android.Content;
using System.IO;
using Java.Util;
using System.Threading;
using System.Text;
using Android.OS;
using Android.Graphics;
using System.Net.Sockets;

namespace wincom.mobile.erp
{
	public class TCPDotMatrixHelper:Activity,IPrintToDevice
	{
		//BluetoothAdapter mBluetoothAdapter;
		TcpClient mmSocket;
		public Stream mmOutputStream=null;
		//BluetoothDevice mmDevice;
		internal AdPara apara;
		internal CompanyInfo compinfo;
		internal string msg;
		internal string pathToDatabase;
		Activity callingActivity;


		public TCPDotMatrixHelper ()
		{
			pathToDatabase = ((GlobalvarsApp)Application.Context).DATABASE_PATH;
			apara =  DataHelper.GetAdPara (pathToDatabase);
		}

		public void SetCallingActivity (Activity activity)
		{
			callingActivity = activity;
		}

		public void SetIsPrintCompLogo (bool  IsPrint)
		{
			
		}

		public bool StartPrint(string text,int noofcopy,ref string errmsg)
		{
			bool isPrinted = false;
			mmSocket=null;
			//FindBTPrinter ();
			//if (mmDevice != null) {
			isPrinted =IsDeviceReady ();
			//}
			errmsg = msg;

			return isPrinted;
		}

		void InitPrinter ()
		{
			byte[] charfont;
			charfont = new Byte[] {
				27,
				64
			};
			//
			mmOutputStream.Write (charfont, 0, charfont.Length);
		}

		private bool IsDeviceReady()
		{
			bool isPrinted = false;
			msg = "";

			try {
				mmSocket = new TcpClient ();
				    
				if (!TrytoConnect (mmSocket))
					return isPrinted;

				Thread.Sleep (300);
				mmOutputStream = mmSocket.GetStream ();
				InitPrinter ();
			
				isPrinted = true;

			} catch (Exception ex) {
				msg = ex.Message;

			}

			return isPrinted;
		}

		public void Close()
		{
			if (mmOutputStream != null) {
				mmOutputStream.Close ();
				mmOutputStream = null;
			}
			if (mmSocket != null) {
				mmSocket.Close ();
				mmSocket = null;
			}
		}

		static bool IsStreamCanWrite (Stream mmOutputStream)
		{
			int nwait = 0;
			bool isReady = false;
			while (true) {
				if (mmOutputStream.CanWrite) {
					isReady = true;
					break;
				}
				else {
					nwait += 1;
					if (nwait > 10)
						break;
				}
				Thread.Sleep (300);
			}
			return isReady;
		}

		private bool TrytoConnect(TcpClient mmSocket)
		{
			bool TrytoConnect = true;
			int count = 0;
			while (TrytoConnect) {
				try {
					Thread.Sleep(400);
					mmSocket.Connect(apara.PrinterIP,9100);	
					TrytoConnect = false;
				} catch {
					count += 1;
					if (count==5)
						TrytoConnect = false;
				}
			}
			return !TrytoConnect;
		}

	}
}

