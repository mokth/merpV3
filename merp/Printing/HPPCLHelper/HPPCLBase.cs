using System;
using Android.App;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace wincom.mobile.erp
{
	public  abstract class PrintPCLBase
	{
		public  string USERID;
		public AdPara apara;
		public CompanyInfo compInfo;
		public string msg;
		public string pathToDatabase;

		public  PrintPCLBase()
		{
			pathToDatabase = ((GlobalvarsApp)Application.Context).DATABASE_PATH;
			USERID = ((GlobalvarsApp)Application.Context).USERID_CODE;
			apara =  DataHelper.GetAdPara (pathToDatabase);
			compInfo =DataHelper.GetCompany(pathToDatabase);
		}

	
		public void PrintLine(string text,Stream mmOutputStream)
		{
			byte[] cc = Encoding.ASCII.GetBytes (text);
			mmOutputStream.Write (cc, 0, cc.Length);

		}

		public void InitPrinter(Stream mmOutputStream)
		{ 
			byte[]  charfont;
			charfont = new Byte[] { 27,69 }; 
			mmOutputStream.Write(charfont, 0, charfont.Length);

		}

		public void SetFormFeed(Stream mmOutputStream)
		{ 
			byte[]  charfont;
			charfont = new Byte[] { 12 }; 
			mmOutputStream.Write(charfont, 0, charfont.Length);

		}

		public void Set12CPI(Stream mmOutputStream)
		{ 
			byte[]  charfont;
			charfont = new Byte[] { 27, 40, 115, 49, 51 ,72 }; //50
			mmOutputStream.Write(charfont, 0, charfont.Length);

		}

		public void Set16CPI(Stream mmOutputStream)
		{ 
			byte[]  charfont;
			charfont = new Byte[] { 27, 40, 115, 49, 54 ,72 }; 
			mmOutputStream.Write(charfont, 0, charfont.Length);

		}

		public void SetLineFeed(Stream mmOutputStream,int noOfline)
		{ 
			byte[]  charfont;
			charfont = new Byte[] { 13 }; 
			for (int i = 0; i < noOfline; i++) {
				mmOutputStream.Write (charfont, 0, charfont.Length);
			}

		}

		public void SetCenter(Stream mmOutputStream, bool isOn)
		{ 
			byte[]  charfont;
			if (isOn)
			{
				charfont = new Byte[] { 27, 97, 1 }; //Char font 9x17
				mmOutputStream.Write(charfont, 0, charfont.Length);
			}
			else
			{
				charfont = new Byte[] { 27, 97, 0 }; //Char font 9x17
				mmOutputStream.Write(charfont, 0, charfont.Length);
			}
		}

		public void SetLQMode(Stream mmOutputStream, bool isOn)
		{
			byte[] charfont;
			if (isOn)
			{
				charfont = new Byte[] { 27, 120, 1 }; //LQ
				mmOutputStream.Write(charfont, 0, charfont.Length);
			}
			else
			{
				charfont = new Byte[] { 27, 120, 0 }; //Draft
				mmOutputStream.Write(charfont, 0, charfont.Length);
			}
		}

		public void Set1Per6InchLineSpacing(Stream mmOutputStream)
		{ 
			byte[]  charfont;
			charfont = new Byte[] { 27,38, 108,54, 68 }; 
			mmOutputStream.Write(charfont, 0, charfont.Length);

		}

		public void Set1Per8InchLineSpacing(Stream mmOutputStream)
		{ 
			byte[]  charfont;
			charfont = new Byte[] { 27,38, 108,56, 68 }; 
			mmOutputStream.Write(charfont, 0, charfont.Length);

		}

//		public void SetDoubleStrike(Stream mmOutputStream, bool isOn)
//		{
//			byte[] charfont;
//			if (isOn)
//			{
//				charfont = new Byte[] { 27, 71 }; //Char font 9x17
//				mmOutputStream.Write(charfont, 0, charfont.Length);
//			}
//			else
//			{
//				charfont = new Byte[] { 27, 72 }; //Char font 9x17
//				mmOutputStream.Write(charfont, 0, charfont.Length);
//			}
//		}

		public void SetBold(Stream mmOutputStream, bool bold)
		{
			byte[]  charfont;
			if (bold)
			{
				charfont = new Byte[] { 27, 40 ,115, 51, 66 }; //bold On
				mmOutputStream.Write(charfont, 0, charfont.Length);
				//  charfont = new Byte[] { 27, 119, 1 }; //double height on
				// mmOutputStream.Write(charfont, 0, charfont.Length);
			}
			else
			{
				charfont = new Byte[] {27, 40 ,115 ,48 ,66 }; //Char font 9x17
				mmOutputStream.Write(charfont, 0, charfont.Length);
				// charfont = new Byte[] { 27, 119, 0 }; //Char font 9x17
				// mmOutputStream.Write(charfont, 0, charfont.Length);
			}
		}

		public List<string> GetLine(string line,int lineLen) 
		{
			string[] text = line.Split(new char[] { ' ','\n','\r' });
			List<string> lines = new List<string>();
			string str = "";
			foreach (string txt in text)
			{
				if ((str.Length + txt.Length + 1) < lineLen)
					str = str + txt + " ";
				else
				{
					lines.Add(str);
					str = txt+" ";
				}
			}
			lines.Add(str);

			return lines;
		}
		public List<string> GetNote(string line,int lineLen) 
		{
			string[] text = line.Split(new char[] { '\n','\r' });
			List<string> lines = new List<string>();
			string str = "";
			foreach (string txt in text)
			{
				if ((str.Length + txt.Length + 1) < lineLen)
					str = str + txt + " ";
				else
				{
					lines.Add(str);
					str = txt+" ";
				}
			}
			lines.Add(str);

			return lines;
		}
	}
}

