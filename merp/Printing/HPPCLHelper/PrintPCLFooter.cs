using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace wincom.mobile.erp
{
	public class PrintPCLFooter:PrintPCLBase
	{
		public void PrintFooter(Stream mmOutputStream,double ttltax,double ttlAmtExcl)
		{
			double ttlAmt = ttltax + ttlAmtExcl;
			string line =  "----------------------------------------------------------------------------------------------\r";
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
					line = amtlines[i].PadRight(67,' ')   + "TOTAL EXCL GST "+ ttlAmtExcl.ToString("n2").PadLeft(12, ' ') + "\r";
				if (i==1)
					line = amtlines[i].PadRight(67,' ')   + "TOTAL GST      "+ ttltax.ToString("n2").PadLeft(12, ' ') + "\r";
				if (i==2)
					line = amtlines[i].PadRight(67,' ')   + "TOTAL INCL GST "+ ttlAmt.ToString("n2").PadLeft(12, ' ') + "\r";
				charfont = Encoding.ASCII.GetBytes(line);
				mmOutputStream.Write(charfont, 0, charfont.Length);
			}
			SetBold(mmOutputStream, false);
			SetLineFeed (mmOutputStream, 3);
			line = "  ________________________    ____________________    ________________________\r";
			charfont = Encoding.ASCII.GetBytes(line);
			mmOutputStream.Write(charfont, 0, charfont.Length);
			line = "         RECEIVED BY                 DATE                   COMPANY CHOP  \r";
			charfont = Encoding.ASCII.GetBytes(line);
			mmOutputStream.Write(charfont, 0, charfont.Length);
			SetFormFeed (mmOutputStream);	
		}


	}
}

