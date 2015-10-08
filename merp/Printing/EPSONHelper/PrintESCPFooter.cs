using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace wincom.mobile.erp
{
	public class PrintESCPFooter:PrintESCPSBase
	{
		public void PrintFooter(Stream mmOutputStream,double ttltax,double ttlAmtExcl)
		{
			double ttlAmt = ttltax + ttlAmtExcl;

			decimal amt = Convert.ToDecimal (ttlAmt);
			string line = "RINGGIT MALAYSIA "+ NumberToWordHelper.NumberToWords (amt);
			List<string> amtlines = GetLine (line,94);
			Set12CPI (mmOutputStream);
			SetBold(mmOutputStream, true);
			foreach (string str in amtlines) {
				byte[] charfont1 = Encoding.ASCII.GetBytes(str);
				mmOutputStream.Write(charfont1, 0, charfont1.Length);
			}
			SetLineFeed(mmOutputStream,1);
			Set12CPI (mmOutputStream);
			SetBold(mmOutputStream, false);

			line =  "----------------------------------------------------------------------------------------------\n";
			byte[] charfont = Encoding.ASCII.GetBytes(line);
			mmOutputStream.Write(charfont, 0, charfont.Length);

			string note = apara.FooterNote;
			List<string> notes = GetNote (note,75);
			if (notes.Count < 3) {
				int left = 3 - notes.Count;
				for (int i = 0; i < left; i++)
					notes.Add ("");
 			}

			for (int i = 0; i < notes.Count; i++) {
				if (i == 0) {
					Set1Per8InchLineSpacing (mmOutputStream);
					Set15CPI (mmOutputStream);
					line = notes [i].PadRight (75, ' ');
					charfont = Encoding.ASCII.GetBytes (line);
					mmOutputStream.Write (charfont, 0, charfont.Length);
					Set12CPI (mmOutputStream);
					SetBold (mmOutputStream, true);
					line ="".PadRight(7,' ')+ "TOTAL EXCL GST " + ttlAmtExcl.ToString ("n2").PadLeft (12, ' ') + "\n";
					charfont = Encoding.ASCII.GetBytes (line);
					mmOutputStream.Write (charfont, 0, charfont.Length);
					SetBold (mmOutputStream, false);
				} else if (i == 1) {
					Set1Per8InchLineSpacing (mmOutputStream);
					Set15CPI (mmOutputStream);
					line = notes [i].PadRight (75, ' ');
					charfont = Encoding.ASCII.GetBytes (line);
					mmOutputStream.Write (charfont, 0, charfont.Length);
					Set12CPI (mmOutputStream);
					SetBold (mmOutputStream, true);
					line ="".PadRight(7,' ')+ "TOTAL GST      " + ttltax.ToString ("n2").PadLeft (12, ' ') + "\n";
					charfont = Encoding.ASCII.GetBytes (line);
					mmOutputStream.Write (charfont, 0, charfont.Length);
					SetBold (mmOutputStream, false);
				} else if (i == 2) {
					Set1Per8InchLineSpacing (mmOutputStream);
					Set15CPI (mmOutputStream);
					line = notes [i].PadRight (75, ' ');
					charfont = Encoding.ASCII.GetBytes (line);
					mmOutputStream.Write (charfont, 0, charfont.Length);
					Set12CPI (mmOutputStream);
					SetBold (mmOutputStream, true);
					line = "".PadRight(7,' ')+"TOTAL INCL GST " + ttlAmt.ToString ("n2").PadLeft (12, ' ') + "\n";
					charfont = Encoding.ASCII.GetBytes (line);
					mmOutputStream.Write (charfont, 0, charfont.Length);
					SetBold (mmOutputStream, false);
				} else {
					Set1Per8InchLineSpacing (mmOutputStream);
					Set15CPI (mmOutputStream);
					line = notes [i].PadRight (75, ' ');
					charfont = Encoding.ASCII.GetBytes (line);
					mmOutputStream.Write (charfont, 0, charfont.Length);
				}
			}
			SetBold(mmOutputStream, false);
			Set1Per6InchLineSpacing (mmOutputStream);
			Set12CPI (mmOutputStream);
			SetLineFeed (mmOutputStream, 3);
			line = "  ____________________________     ____________________________\n";
			charfont = Encoding.ASCII.GetBytes(line);
			mmOutputStream.Write(charfont, 0, charfont.Length);
			line = "        DELIVERED BY                 CUSTOMER SIGNATURE & CHOP\n";
			charfont = Encoding.ASCII.GetBytes(line);
			mmOutputStream.Write(charfont, 0, charfont.Length);
			Set1Per8InchLineSpacing (mmOutputStream);
			Set15CPI (mmOutputStream);
			string note1 ="".PadLeft(45,' ')+"We hereby acknowledge receipt of above mentioned\n";
			string note2 ="".PadLeft(45,' ')+"goods in good order and conditions.\n";
			charfont = Encoding.ASCII.GetBytes(note1);
			mmOutputStream.Write(charfont, 0, charfont.Length);	
			charfont = Encoding.ASCII.GetBytes(note2);
			mmOutputStream.Write(charfont, 0, charfont.Length);	
			Set1Per6InchLineSpacing (mmOutputStream);
			Set12CPI (mmOutputStream);
			SetFormFeed (mmOutputStream);	
		}

		public void PrintFooter_ori(Stream mmOutputStream,double ttltax,double ttlAmtExcl)
		{
			double ttlAmt = ttltax + ttlAmtExcl;
			string line =  "----------------------------------------------------------------------------------------------\n";
			byte[] charfont = Encoding.ASCII.GetBytes(line);
			mmOutputStream.Write(charfont, 0, charfont.Length);


			decimal amt = Convert.ToDecimal (ttlAmt);
			line = "RINGGIT MALAYSIA "+ NumberToWordHelper.NumberToWords (amt);
			List<string> amtlines = GetLine (line,60);

			Set12CPI (mmOutputStream);
			SetBold(mmOutputStream, true);

			if (amtlines.Count < 3) {
				int left = 3 - amtlines.Count;
				for (int i = 0; i < left; i++)
					amtlines.Add ("");
			}

			for (int i = 0; i < amtlines.Count; i++) {
				if (i==0)
					line = amtlines[i].PadRight(67,' ')   + "TOTAL EXCL GST "+ ttlAmtExcl.ToString("n2").PadLeft(12, ' ') + "\n";
				if (i==1)
					line = amtlines[i].PadRight(67,' ')   + "TOTAL GST      "+ ttltax.ToString("n2").PadLeft(12, ' ') + "\n";
				if (i==2)
					line = amtlines[i].PadRight(67,' ')   + "TOTAL INCL GST "+ ttlAmt.ToString("n2").PadLeft(12, ' ') + "\n";
				charfont = Encoding.ASCII.GetBytes(line);
				mmOutputStream.Write(charfont, 0, charfont.Length);
			}
			SetBold(mmOutputStream, false);
			Set1Per8InchLineSpacing (mmOutputStream);
			Set15CPI (mmOutputStream);

			string note = apara.FooterNote+"\n";
			charfont = Encoding.ASCII.GetBytes(note);
			mmOutputStream.Write(charfont, 0, charfont.Length);
			Set1Per6InchLineSpacing (mmOutputStream);
			Set12CPI (mmOutputStream);
			SetLineFeed (mmOutputStream, 3);
			line = "  ________________________    ____________________    ________________________\n";
			charfont = Encoding.ASCII.GetBytes(line);
			mmOutputStream.Write(charfont, 0, charfont.Length);
			line = "        DELIVERED BY                 DATE             CUSTOMER SIGNATURE & CHOP\n";
			charfont = Encoding.ASCII.GetBytes(line);
			mmOutputStream.Write(charfont, 0, charfont.Length);
			Set1Per8InchLineSpacing (mmOutputStream);
			Set15CPI (mmOutputStream);
			string note1 ="".PadLeft(67,' ')+"We hereby acknowledge receipt of above mentioned\n";
			string note2 ="".PadLeft(67,' ')+"goods in good order and conditions.\n";
			charfont = Encoding.ASCII.GetBytes(note1);
			mmOutputStream.Write(charfont, 0, charfont.Length);	
			charfont = Encoding.ASCII.GetBytes(note2);
			mmOutputStream.Write(charfont, 0, charfont.Length);	
			Set1Per6InchLineSpacing (mmOutputStream);
			Set12CPI (mmOutputStream);
			SetFormFeed (mmOutputStream);	
		}



	}
}

