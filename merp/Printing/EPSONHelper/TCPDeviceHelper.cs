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
	public class TCPDeviceHelper:Activity,IPrintToDevice
	{
		//BluetoothAdapter mBluetoothAdapter;
		TcpClient mmSocket;
		//BluetoothDevice mmDevice;
		internal AdPara apara;
		internal CompanyInfo compinfo;
		internal string msg;
		internal string pathToDatabase;
		Activity callingActivity;
		bool iSPrintCompLogo=false;
		private  static byte ESC_CHAR = 0x1B;
		private  static byte GS = 0x1D;
		private  static byte[] LINE_FEED = new byte[]{0x0A};
		private  static byte[] CUT_PAPER = new byte[]{GS, 0x56, 0x00};
		private  static byte[] INIT_PRINTER = new byte[]{ESC_CHAR, 0x40};
		private static byte[] SELECT_BIT_IMAGE_MODE = {0x1B, 0x2A, 33};
		private  static byte[] SET_LINE_SPACE_24 = new byte[]{ESC_CHAR, 0x33, 24};
		private  static byte[] SET_LINE_SPACE_1 = new byte[]{ESC_CHAR, 0x33, 1};

		public TCPDeviceHelper ()
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
			iSPrintCompLogo = IsPrint;
		}

		public bool StartPrint(string text,int noofcopy,ref string errmsg)
		{
			bool isPrinted = false;
			mmSocket=null;
			//FindBTPrinter ();
			//if (mmDevice != null) {
				isPrinted =PrintToDevice (text, noofcopy);
			//}
			errmsg = msg;

			return isPrinted;
		}


		private bool PrintToDevice(string text,int noofcopy)
		{
			bool isPrinted = false;
			msg = "";
			Stream mmOutputStream=null;
			try {


				mmSocket =  new TcpClient(apara.PrinterIP,9100);

			
					
					//TrytoConnect(mmSocket);

					Thread.Sleep (300);
				    mmOutputStream = mmSocket.GetStream();
					byte[] charfont;

					if (iSPrintCompLogo)
						PrintLogo(mmOutputStream);

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

		private void PrintLogo(Stream mmOutputStream)
		{
			string document = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
			string logopath = "";
			if (apara.PaperSize=="58mm")
			     logopath = System.IO.Path.Combine (document, "logo58.png");
			else  logopath = System.IO.Path.Combine (document, "logo80.png");
			if (!File.Exists (logopath))
				return;
			Stream fg =null;
			try {
				fg = File.Open (logopath, FileMode.Open);

				var bitmap = BitmapFactory.DecodeStream (fg);

				int[][] pixel = getPixelsSlow (bitmap);
				printImage (mmOutputStream, pixel);

			} catch (Exception ex) {

			}finally{
				if (fg != null) {
					fg.Close ();
					fg = null;
				}
			}
		}

		private int[][] getPixelsSlow(Bitmap image) {
			int width = image.Width;
			int height = image.Height;
			int[][] result = new int[height][];
			for (int row = 0; row < height; row++) {
				result [row] = new int[width];
				for (int col = 0; col < width; col++) {
					int clr= image.GetPixel (col,row);

					Color c = new Color (clr);
					//Console.WriteLine ("{0} {1}",clr,c.ToString());
					result [row] [col] = c.ToArgb ();
				}
			}

			return result;
		}

		private void printImage(Stream mmOutputStream,int[][] pixels) {
			// Set the line spacing at 24 (we'll print 24 dots high)
			byte[] charfont = new Byte[] { 27, 64 }; //Char font 9x17
			mmOutputStream.Write(charfont, 0, charfont.Length);
			if (apara.PaperSize == "80mm") {
				charfont = new Byte[] { 27, 33, 0 }; //Char font 12x24
				mmOutputStream.Write (charfont, 0, charfont.Length);
				mmOutputStream.Write (SET_LINE_SPACE_1, 0, SET_LINE_SPACE_1.Length);
			}else mmOutputStream.Write (SET_LINE_SPACE_24, 0, SET_LINE_SPACE_24.Length);

			for (int y = 0; y < pixels.Length; y += 24) {
				// Like I said before, when done sending data, 
				// the printer will resume to normal text printing
				mmOutputStream.Write(SELECT_BIT_IMAGE_MODE,0,SELECT_BIT_IMAGE_MODE.Length);
				// Set nL and nH based on the width of the image
				byte[] dd =new byte[]{(byte)(0x00ff & pixels[y].Length), (byte)((0xff00 & pixels[y].Length) >> 8)};
				mmOutputStream.Write(dd,0,dd.Length);
				for (int x = 0; x < pixels[y].Length;x++) {
					// for each stripe, recollect 3 bytes (3 bytes = 24 bits)
					byte[] data=recollectSlice(y, x, pixels);
					mmOutputStream.Write(data,0,data.Length);
				}

				// Do a line feed, if not the printing will resume on the same line
				mmOutputStream.WriteByte(13);
			}
			//mmOutputStream.WriteByte(SET_LINE_SPACE_30);
		}

		private byte[] recollectSlice(int y, int x, int[][] img) {
			byte[] slices = new byte[] {0, 0, 0};
			for (int yy = y, i = 0; yy < y + 24 && i < 3; yy += 8, i++) {
				byte slice = 0;
				for (int b = 0; b < 8; b++) {
					int yyy = yy + b;
					if (yyy >= img.Length) {
						continue;
					}
					int col = img[yyy][x]; 
					bool v = shouldPrintColor(col);
					slice |= (byte) ((v ? 1 : 0) << (7 - b));
				}
				slices[i] = slice;
			}

			return slices;
		}

		private bool shouldPrintColor(int col) {
			int threshold = 127;
			int a, r, g, b, luminance;
			a = (col >> 24) & 0xff;
			if (a != 0xff) {// Ignore transparencies
				return false;
			}
			Color clr = new Color (col);
			//r = (col >> 16) & 0xff;
			//	g = (col >> 8) & 0xff;
			//	b = col & 0xff;

			r = (col & 0x00ff0000) >> 16;
			g = (col & 0x0000ff00) >> 8;
			b = col & 0x000000ff;

			luminance = (int) (0.299 * r + 0.587 * g + 0.114 * b);

			return luminance < threshold;
			//return true;
		}

	}
}

