using System;

namespace wincom.mobile.erp
{
	public class PrintReportFooter:PrintHelperBase
	{
		public void PrintFooter (ref string test)
		{
			test += "\n\n\n\n";
			test += "------------------------------------------\n";
			test += "     RECEIVED BY (COMPANY CHOP AND SIGN)  \n";


		}
	}
}

