﻿using System;
using System.IO;
using System.Text;

namespace wincom.mobile.erp
{
	public class PrintPCLHeader:PrintPCLBase
	{
		internal int line;

		public void PrintHeader(Stream mmOutputStream,Invoice inv,string pageno)
		{
			//SetLQMode (mmOutputStream,true);
			SetBold(mmOutputStream, true);
			//SetCenter(mmOutputStream, true);
			string[] names = compInfo.CompanyName.Split (new char[] {
				'|'
			});
			string compname = (names.Length > 1) ? names [1] : names [0];
			byte[] charfont = Encoding.ASCII.GetBytes(compname);
			mmOutputStream.Write(charfont, 0, charfont.Length);
			SetBold(mmOutputStream, false);

			Set16CPI (mmOutputStream);
			charfont = Encoding.ASCII.GetBytes(" ("+compInfo.RegNo+")\r");
			line =1;
			mmOutputStream.Write(charfont, 0, charfont.Length);
			Set12CPI (mmOutputStream);
			//Set1Per6InchLineSpacing (mmOutputStream);

			string address = GetCompAddress (compInfo);
			charfont = Encoding.ASCII.GetBytes(address);
			mmOutputStream.Write(charfont, 0, charfont.Length);
			//SetCenter(mmOutputStream, false);
			SetLineFeed (mmOutputStream, 2);

			Trader cust = DataHelper.GetTrader (pathToDatabase, inv.custcode);

			//string strline =cust.CustName.ToUpper().PadRight(65, ' ') + "  TAX INVOICE\r"; line +=1;
			string strline = inv.description.ToUpper().PadRight(65, ' ') + "  TAX INVOICE\r"; line +=1;
			SetBold(mmOutputStream, true);
			charfont = Encoding.ASCII.GetBytes(strline);
			mmOutputStream.Write(charfont, 0, charfont.Length);

			SetBold(mmOutputStream, false);
			string line1 = cust.Addr1.ToUpper ().PadRight (65, ' ') +  "  INVOICE NO: " + inv.invno + "\r"; line +=1;
			line1 = line1 + cust.Addr2.ToUpper ().PadRight (65, ' ') + "  DATE      : " + inv.invdate.ToString ("dd-MM-yyyy") + "\r";
			line +=1;
			line1 = line1 +  cust.Addr3.ToUpper().PadRight(65, ' ') + "  TERMS     : "+cust.PayCode+ "\r";
			line +=1;
			address = "TEL: " + cust.Tel + "   FAX: " + cust.Fax;
			line1 = line1 +  address.ToUpper().PadRight(65, ' ') + "  PAGE NO   : "+pageno+"\r";
			line +=1;
			address = "GST NO: " + cust.gst; 
			line1 = line1 + address.ToUpper().PadRight(65, ' ') + " \r";
			line +=1;
			charfont = Encoding.ASCII.GetBytes(line1);
			mmOutputStream.Write(charfont, 0, charfont.Length);
			line1 = "";
			PrintCaption (ref line1);
			charfont = Encoding.ASCII.GetBytes(line1);
			mmOutputStream.Write(charfont, 0, charfont.Length);

		}

		public void PrintCNHeader(Stream mmOutputStream,CNNote inv,string pageno)
		{
			//SetLQMode (mmOutputStream,true);
			SetBold(mmOutputStream, true);
			//SetCenter(mmOutputStream, true);
			string[] names = compInfo.CompanyName.Split (new char[] {
				'|'
			});
			string compname = (names.Length > 1) ? names [1] : names [0];
			byte[] charfont = Encoding.ASCII.GetBytes(compname);
			mmOutputStream.Write(charfont, 0, charfont.Length);
			SetBold(mmOutputStream, false);

			Set16CPI (mmOutputStream);
			charfont = Encoding.ASCII.GetBytes(" ("+compInfo.RegNo+")\r");
			line =1;
			mmOutputStream.Write(charfont, 0, charfont.Length);
			Set12CPI (mmOutputStream);
			//Set1Per6InchLineSpacing (mmOutputStream);

			string address = GetCompAddress (compInfo);
			charfont = Encoding.ASCII.GetBytes(address);
			mmOutputStream.Write(charfont, 0, charfont.Length);
			//SetCenter(mmOutputStream, false);
			SetLineFeed (mmOutputStream, 2);

			Trader cust = DataHelper.GetTrader (pathToDatabase, inv.custcode);

			string strline =cust.CustName.ToUpper().PadRight(65, ' ') + "  CREDIT NOTE\r"; line +=1;
			SetBold(mmOutputStream, true);
			charfont = Encoding.ASCII.GetBytes(strline);
			mmOutputStream.Write(charfont, 0, charfont.Length);

			SetBold(mmOutputStream, false);
			string line1 = cust.Addr1.ToUpper ().PadRight (65, ' ') +  "  C/NOTE NO : " + inv.cnno + "\r"; line +=1;
			line1 = line1 + cust.Addr2.ToUpper ().PadRight (65, ' ') + "  INVOICE NO: " + inv.invno + "\r";
			line +=1;
			line1 = line1 +  cust.Addr3.ToUpper().PadRight(65, ' ') + "  DATE      : " + inv.invdate.ToString ("dd-MM-yyyy") + "\r";
			line +=1;
			address = "TEL: " + cust.Tel + "   FAX: " + cust.Fax;
			line1 = line1 +  address.ToUpper().PadRight(65, ' ') + "  TERMS     : "+cust.PayCode+ "\r";
			line +=1;
			address = "GST NO: " + cust.gst; 
			line1 = line1 + address.ToUpper().PadRight(65, ' ') + "  PAGE NO   : "+pageno+"\r";
			line +=1;
			charfont = Encoding.ASCII.GetBytes(line1);
			mmOutputStream.Write(charfont, 0, charfont.Length);
			line1 = "";
			PrintCaption (ref line1);
			charfont = Encoding.ASCII.GetBytes(line1);
			mmOutputStream.Write(charfont, 0, charfont.Length);

		}

		private string GetCompAddress(CompanyInfo comp)
		{
			string tel = string.IsNullOrEmpty (compInfo.Tel) ? " " : compInfo.Tel.Trim ();
			string fax = string.IsNullOrEmpty (compInfo.Fax) ? " " : compInfo.Fax.Trim ();
			string addr1 = string.IsNullOrEmpty (compInfo.Addr1) ? "" : compInfo.Addr1.Trim ();
			string addr2 = string.IsNullOrEmpty (compInfo.Addr2) ? "" : compInfo.Addr2.Trim ();
			string addr3 = string.IsNullOrEmpty (compInfo.Addr3) ? "" : compInfo.Addr3.Trim ();
			string addr4 = string.IsNullOrEmpty (compInfo.Addr4) ? "" : compInfo.Addr4.Trim ();
			string gst = string.IsNullOrEmpty (compInfo.GSTNo) ? "" : compInfo.GSTNo.Trim ();
			string compname = compInfo.CompanyName.Trim ();
			string address = "";
			if (!string.IsNullOrEmpty (addr1)) {
				address = address + compInfo.Addr1 + "\r"; line +=1;
			}
			if (!string.IsNullOrEmpty (addr2)) {
				address = address + compInfo.Addr2 + "\r"; line +=1;
			}
			if (!string.IsNullOrEmpty (addr3)) {
				address = address + compInfo.Addr3 + "\r";line +=1;
			}
			if (!string.IsNullOrEmpty (addr4)) {
				address = address + compInfo.Addr4 + "\r";line +=1;
			}
			address = address + "TEL: " + compInfo.Tel + "    FAX: " + compInfo.Fax+"\r"; line +=1;
			address = address + "GST NO :" + gst + "\r";line +=1;
			return address;
		}

		private void PrintCaption(ref string text)
		{
			//            "         1         2         3         4         5         6         7         8         9         0         1         2\r"
			//            "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890
			text = text + "----------------------------------------------------------------------------------------------\r";
			text = text + "No. ITEM CODE       ITEM DESCRIPTION                  UNIT  QTY  UNIT      GST    TAX   AMOUNT\r";
			text = text + "                                                     PRICE                 AMT   CODE         \r";
			text = text + "----------------------------------------------------------------------------------------------\r";
			            // 1234 123456789012345 1234567890123456789012345678 12345678  
		}
	}
}

