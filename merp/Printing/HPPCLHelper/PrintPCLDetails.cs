using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace wincom.mobile.erp
{
	public class PrintPCLDetails:PrintPCLBase
	{
		double _ttltax;
		double _ttlAmt;
		int ttlline;

		public double TotalNetAmount
		{
			get { return _ttlAmt; }
		}

		public double TotaTaxAmount
		{
			get { return _ttltax; }
		}

		public int TotalLine
		{
			get { return ttlline; }
		}

		public void PrintDetalis(Stream mmOutputStream,Invoice inv,InvoiceDtls[] invdtls)
		{
			 ttlline = 0;
			_ttltax =0;
			_ttlAmt=0;
			string line = "";
			int itemno = 1;
			byte[] charfont;
			foreach (InvoiceDtls dtl in invdtls) {
				if (dtl.icode.Length < 15 && dtl.description.Length < 28) {
					ttlline += 1;
					line = (itemno.ToString () + ".").PadRight (4, ' ') + 
							dtl.icode.ToUpper ().PadRight (16, ' ') + 
							dtl.description.ToUpper ().PadRight (29, ' ') + 
							dtl.price.ToString ("n2").PadLeft (9, ' ') + 
							dtl.qty.ToString ("n0").PadLeft (5, ' ') + 
							"".PadLeft (6, ' ') + 
							dtl.tax.ToString ("n2").PadLeft (8, ' ') +
							dtl.taxgrp.PadLeft (7, ' ') + 
						    dtl.amount.ToString("n2") .PadLeft(10, ' ') + "\r";
					PrintLine (line,mmOutputStream);
				} else if (dtl.icode.Length > 15 && dtl.description.Length < 28) {
					ttlline += 2;
					line = (itemno.ToString () + ".").PadRight (4, ' ') + 
							dtl.icode.Substring(0,15).ToUpper ().PadRight (16, ' ') + 
							dtl.description.ToUpper ().PadRight (29, ' ') + 
							dtl.price.ToString ("n2").PadLeft (9, ' ') + 
							dtl.qty.ToString ("n0").PadLeft (5, ' ') + 
							"".PadLeft (6, ' ') + 
							dtl.tax.ToString ("n2").PadLeft (8, ' ') +
							dtl.taxgrp.PadLeft (7, ' ') + 
							dtl.amount.ToString("n2") .PadLeft(10, ' ') + "\r";
					PrintLine (line,mmOutputStream);
					PrintLine (dtl.icode.ToUpper().Substring(15)+"\r",mmOutputStream);
				}else if (dtl.description.Length > 28) {
					List<string> lines = GetLine (dtl.description,28);
					ttlline = ttlline + lines.Count;
					List<string> ICodes = new List<string> ();
					if (dtl.icode.Length > 15) {
						ICodes.Add (dtl.icode.ToUpper ().Substring (0, 15));
						ICodes.Add (dtl.icode.ToUpper ().Substring (15));
					}else ICodes.Add (dtl.icode.ToUpper ());
					for (int i = 0; i < lines.Count; i++) {
						ICodes.Add ("");
					}

					line = (itemno.ToString () + ".").PadRight (4, ' ') + 
						ICodes[0].PadRight (16, ' ') + 
						lines[0].PadRight (29, ' ') + 
						dtl.price.ToString ("n2").PadLeft (9, ' ') + 
						dtl.qty.ToString ("n0").PadLeft (5, ' ') + 
						"".PadLeft (6, ' ') + 
						dtl.tax.ToString ("n2").PadLeft (8, ' ') +
						dtl.taxgrp.PadLeft (7, ' ') + 
						dtl.amount.ToString("n2") .PadLeft(10, ' ') + "\r";
					PrintLine (line,mmOutputStream);
					for (int i = 1; i < lines.Count; i++) {
						line = (itemno.ToString () + ".").PadRight (4, ' ') + 
							ICodes[i].PadRight (16, ' ') + 
							lines[i].PadRight (29, ' ') +  "\r";
						PrintLine (line,mmOutputStream);
					}

				}  

				itemno += 1;
				_ttltax = _ttltax + dtl.tax;
				_ttlAmt = _ttlAmt + dtl.netamount;
			}


			//int remindline = 35 - ttlline;
			//SetLineFeed (mmOutputStream, remindline);

		}


		public List<string> GetPrintDetalis(Invoice inv,InvoiceDtls[] invdtls)
		{
			List<string> text = new List<string>();
			ttlline = 0;
			_ttltax =0;
			_ttlAmt=0;
			string line = "";
			int itemno = 1;
			byte[] charfont;
			Regex re = new Regex("\r\r$");
			string desc = "";
			foreach (InvoiceDtls dtl in invdtls) {
				
				desc = re.Replace(dtl.description, "").ToUpper();
				if (dtl.icode.Length < 15 && desc.Length < 28) {
					ttlline += 1;
					line = (itemno.ToString () + ".").PadRight (4, ' ') + 
						dtl.icode.ToUpper ().PadRight (16, ' ') + 
						desc.PadRight (29, ' ') + 
						dtl.price.ToString ("n2").PadLeft (9, ' ') + 
						dtl.qty.ToString ("n0").PadLeft (5, ' ') + 
						"".PadLeft (6, ' ') + 
						dtl.tax.ToString ("n2").PadLeft (8, ' ') +
						dtl.taxgrp.PadLeft (7, ' ') + 
						dtl.amount.ToString("n2") .PadLeft(10, ' ') + "\r";
					text.Add(line);
				} else if (dtl.icode.Length > 15 && desc.Length < 28) {
					ttlline += 2;
					line = (itemno.ToString () + ".").PadRight (4, ' ') + 
						dtl.icode.Substring(0,15).ToUpper ().PadRight (16, ' ') + 
						desc.PadRight (29, ' ') + 
						dtl.price.ToString ("n2").PadLeft (9, ' ') + 
						dtl.qty.ToString ("n0").PadLeft (5, ' ') + 
						"".PadLeft (6, ' ') + 
						dtl.tax.ToString ("n2").PadLeft (8, ' ') +
						dtl.taxgrp.PadLeft (7, ' ') + 
						dtl.amount.ToString("n2") .PadLeft(10, ' ') + "\r";
					text.Add(line);
					text.Add("".PadRight (4, ' ') +dtl.icode.ToUpper ().Substring (15) + "\r");

				}else if (desc.Length > 28) {
					List<string> lines = GetLine (desc,28);
					ttlline = ttlline + lines.Count;
					List<string> ICodes = new List<string> ();
					if (dtl.icode.Length > 15) {
						ICodes.Add (dtl.icode.ToUpper ().Substring (0, 15));
						ICodes.Add (dtl.icode.ToUpper ().Substring (15));
					}else ICodes.Add (dtl.icode.ToUpper ());
					for (int i = 0; i < lines.Count; i++) {
						ICodes.Add ("");
					}

					line = (itemno.ToString () + ".").PadRight (4, ' ') + 
						ICodes[0].PadRight (16, ' ') + 
						lines[0].PadRight (29, ' ') + 
						dtl.price.ToString ("n2").PadLeft (9, ' ') + 
						dtl.qty.ToString ("n0").PadLeft (5, ' ') + 
						"".PadLeft (6, ' ') + 
						dtl.tax.ToString ("n2").PadLeft (8, ' ') +
						dtl.taxgrp.PadLeft (7, ' ') + 
						dtl.amount.ToString("n2") .PadLeft(10, ' ') + "\r";
					text.Add(line);
					for (int i = 1; i < lines.Count; i++) {
						line = "".PadRight (4, ' ') + 
							ICodes[i].PadRight (16, ' ') + 
							lines[i].PadRight (29, ' ') +  "\r";
						text.Add(line);
					}
				}  

				itemno += 1;
				_ttltax = _ttltax + dtl.tax;
				_ttlAmt = _ttlAmt + dtl.netamount;
			}

			return text;
		}

		public List<string> GetPrintCNDetalis(CNNote inv,CNNoteDtls[] invdtls)
		{
			List<string> text = new List<string>();
			ttlline = 0;
			_ttltax =0;
			_ttlAmt=0;
			string line = "";
			int itemno = 1;
			byte[] charfont;
			Regex re = new Regex("\r\r$");
			string desc = "";
			foreach (CNNoteDtls dtl in invdtls) {

				desc = re.Replace(dtl.description, "").ToUpper();
				if (dtl.icode.Length < 15 && desc.Length < 28) {
					ttlline += 1;
					line = (itemno.ToString () + ".").PadRight (4, ' ') + 
						dtl.icode.ToUpper ().PadRight (16, ' ') + 
						desc.PadRight (29, ' ') + 
						dtl.price.ToString ("n2").PadLeft (9, ' ') + 
						dtl.qty.ToString ("n0").PadLeft (5, ' ') + 
						"".PadLeft (6, ' ') + 
						dtl.tax.ToString ("n2").PadLeft (8, ' ') +
						dtl.taxgrp.PadLeft (7, ' ') + 
						dtl.amount.ToString("n2") .PadLeft(10, ' ') + "\r";
					text.Add(line);
				} else if (dtl.icode.Length > 15 && desc.Length < 28) {
					ttlline += 2;
					line = (itemno.ToString () + ".").PadRight (4, ' ') + 
						dtl.icode.Substring(0,15).ToUpper ().PadRight (16, ' ') + 
						desc.PadRight (29, ' ') + 
						dtl.price.ToString ("n2").PadLeft (9, ' ') + 
						dtl.qty.ToString ("n0").PadLeft (5, ' ') + 
						"".PadLeft (6, ' ') + 
						dtl.tax.ToString ("n2").PadLeft (8, ' ') +
						dtl.taxgrp.PadLeft (7, ' ') + 
						dtl.amount.ToString("n2") .PadLeft(10, ' ') + "\r";
					text.Add(line);
					text.Add("".PadRight (4, ' ') +dtl.icode.ToUpper ().Substring (15) + "\r");

				}else if (desc.Length > 28) {
					List<string> lines = GetLine (desc,28);
					ttlline = ttlline + lines.Count;
					List<string> ICodes = new List<string> ();
					if (dtl.icode.Length > 15) {
						ICodes.Add (dtl.icode.ToUpper ().Substring (0, 15));
						ICodes.Add (dtl.icode.ToUpper ().Substring (15));
					}else ICodes.Add (dtl.icode.ToUpper ());
					for (int i = 0; i < lines.Count; i++) {
						ICodes.Add ("");
					}

					line = (itemno.ToString () + ".").PadRight (4, ' ') + 
						ICodes[0].PadRight (16, ' ') + 
						lines[0].PadRight (29, ' ') + 
						dtl.price.ToString ("n2").PadLeft (9, ' ') + 
						dtl.qty.ToString ("n0").PadLeft (5, ' ') + 
						"".PadLeft (6, ' ') + 
						dtl.tax.ToString ("n2").PadLeft (8, ' ') +
						dtl.taxgrp.PadLeft (7, ' ') + 
						dtl.amount.ToString("n2") .PadLeft(10, ' ') + "\r";
					text.Add(line);
					for (int i = 1; i < lines.Count; i++) {
						line = "".PadRight (4, ' ') + 
							ICodes[i].PadRight (16, ' ') + 
							lines[i].PadRight (29, ' ') +  "\r";
						text.Add(line);
					}
				}  

				itemno += 1;
				_ttltax = _ttltax + dtl.tax;
				_ttlAmt = _ttlAmt + dtl.netamount;
			}

			return text;
		}

		public void PrintLine (Stream mmOutputStream,string line)
		{
			byte[] charfont = Encoding.ASCII.GetBytes(line);
			mmOutputStream.Write(charfont, 0, charfont.Length);
		}

		public void FormFeed (Stream mmOutputStream)
		{
			SetFormFeed (mmOutputStream);
		}

		public int GetTotalDtlLineCount(Invoice inv,InvoiceDtls[] invdtls)
		{
			int ttlline = 0;
			foreach (InvoiceDtls dtl in invdtls) {
				if (dtl.icode.Length < 15 && dtl.description.Length < 28) {
					ttlline += 1;
				} else if (dtl.icode.Length > 15 && dtl.description.Length < 28) {
					ttlline += 2;
				}else if (dtl.description.Length > 28) {
					List<string> lines = GetLine (dtl.description,29);
					ttlline = ttlline + lines.Count;
				}
			}

			return ttlline;
		}

//		public List<string> GetLine(string line) 
//		{
//			string[] text = line.Split(new char[] { ' ' });
//			List<string> lines = new List<string>();
//			string str = "";
//			foreach (string txt in text)
//			{
//				if ((str.Length + txt.Length + 1) < 28)
//					str = str + txt + " ";
//				else
//				{
//					lines.Add(str);
//					str = txt+" ";
//				}
//			}
//			lines.Add(str);
//
//			return lines;
//		}
	}
}

