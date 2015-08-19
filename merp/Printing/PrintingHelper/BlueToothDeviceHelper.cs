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

namespace wincom.mobile.erp
{
	public class BlueToothDeviceHelper:Activity,IPrintToDevice
	{
		BluetoothAdapter mBluetoothAdapter;
		BluetoothSocket mmSocket;
		BluetoothDevice mmDevice;
		internal AdPara apara;
		internal CompanyInfo compinfo;
		internal string msg;
		internal string pathToDatabase;
		Activity callingActivity;

		public BlueToothDeviceHelper ()
		{
			pathToDatabase = ((GlobalvarsApp)Application.Context).DATABASE_PATH;
			apara =  DataHelper.GetAdPara (pathToDatabase);
		}

		public void SetCallingActivity (Activity activity)
		{
			callingActivity = activity;
		}

		public bool StartPrint(string text,int noofcopy,ref string errmsg)
		{
			bool isPrinted = false;
			mBluetoothAdapter=null;
			mmSocket=null;
			mmDevice = null;
			FindBTPrinter ();
			if (mmDevice != null) {
				isPrinted =PrintToDevice (text, noofcopy);
			}
			errmsg = msg;

			return isPrinted;
		}

		private void FindBTPrinter(){
			string printername = apara.PrinterName.Trim ().ToUpper ();

			try{
				mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;

				if (mBluetoothAdapter ==null)
				{
					msg = callingActivity.Resources.GetString(Resource.String.msg_bluetoothnofound);
					return;
				}
				string txt ="";
				if (!mBluetoothAdapter.Enable()) {
					Intent enableBluetooth = new Intent(
						BluetoothAdapter.ActionRequestEnable);
					StartActivityForResult(enableBluetooth, 0);
				}

				var pair= mBluetoothAdapter.BondedDevices;
				if (pair.Count > 0) {
					foreach (BluetoothDevice dev in pair) {
						Console.WriteLine (dev.Name);
						txt = txt+","+dev.Name;
						if (dev.Name.ToUpper()==printername)
						{
							mmDevice = dev;
							//							File.WriteAllText(addrfile,dev.Address);
							break;
						}
					}
				}
				msg= callingActivity.Resources.GetString(Resource.String.msg_bluetoothfound) +mmDevice.Name;

			}catch {
				//txtv.Text = ex.Message;
				mmDevice = null;
				msg = callingActivity.Resources.GetString(Resource.String.msg_bluetoothnofound);
				//AlertShow(ex.Message);
			}finally {
				if (mBluetoothAdapter != null) {
					if (mBluetoothAdapter.IsDiscovering) {
						mBluetoothAdapter.CancelDiscovery ();
						Thread.Sleep (300);
					}

					mBluetoothAdapter.Dispose ();
					mBluetoothAdapter = null;
				}

			}
		}

		private bool PrintToDevice(string text,int noofcopy)
		{
			bool isPrinted = false;
			msg = "";
			Stream mmOutputStream=null;
			try {
				UUID uuid = UUID.FromString ("00001101-0000-1000-8000-00805F9B34FB");

				mmSocket = mmDevice.CreateInsecureRfcommSocketToServiceRecord (uuid);
				if (mmSocket == null) {
					msg = callingActivity.Resources.GetString(Resource.String.msg_bluetoothnofound);
					return isPrinted;
				}
			if (mmDevice.BondState == Bond.Bonded) {
					
					TrytoConnect(mmSocket);

					Thread.Sleep (300);
					mmOutputStream = mmSocket.OutputStream;
					byte[] charfont;
					charfont = new Byte[] { 27, 64 }; //Char font 9x17
					mmOutputStream.Write(charfont, 0, charfont.Length);
					if (apara.PaperSize=="58mm")
					{
						charfont = new Byte[] { 27, 33, 1 }; //Char font 9x17
						mmOutputStream.Write(charfont, 0, charfont.Length);
					}

					if (apara.PaperSize=="80mm")
					{
						charfont = new Byte[] { 27, 33, 0 }; //Char font 12x24
						mmOutputStream.Write(charfont, 0, charfont.Length);
					}
					charfont = new Byte[] { 28, 38 };
					mmOutputStream.Write(charfont, 0, charfont.Length);
				
					byte[] cc = Encoding.GetEncoding("GB18030").GetBytes(text);
					for (int i=0; i<noofcopy;i++)
					{
						int rem;
						int result =Math.DivRem(cc.Length, 2048, out rem);
						int pos =0;
						for(int line= 0;line<result;line++)
						{
							IsStreamCanWrite (mmOutputStream);
							mmOutputStream.Write (cc, pos, 2048);
							pos += 2048;
						}
						if (rem >0)
							mmOutputStream.Write (cc, pos, rem);
						Thread.Sleep (3000);

					}
					Thread.Sleep (300);
					charfont = new Byte[] { 28, 46 };
					mmOutputStream.Write(charfont, 0, charfont.Length);
					mmOutputStream.Close ();
					mmSocket.Close ();
					isPrinted =true;

				} else {
					//txtv.Text = "Device not connected";
					msg= callingActivity.Resources.GetString(Resource.String.msg_bluetoothnofound);
					//errmsg.Append(msg);
				}
			} catch (Exception ex) {
				msg = ex.Message;

			}
			finally {
				if (mmOutputStream != null) {
					mmOutputStream.Close ();
					mmOutputStream = null;
				}
				if (mmSocket != null) {
					mmSocket.Close ();
					mmSocket = null;
				}
				if (mmDevice != null) {
					mmDevice.Dispose ();
					mmDevice = null;
				}
			}


			return isPrinted;
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

		private bool TrytoConnect(BluetoothSocket mmSocket)
		{
			bool TrytoConnect = true;
			int count = 0;
			while (TrytoConnect) {
				try {
					Thread.Sleep(400);
					mmSocket.Connect ();
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

