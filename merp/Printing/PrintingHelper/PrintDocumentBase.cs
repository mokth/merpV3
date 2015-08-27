using System;
using System.Collections;

namespace wincom.mobile.erp
{
	public class PrintDocumentBase
	{
		internal Hashtable extrapara;
		internal string text;
		internal string errMsg;
		internal PrintCompanyHeader prtcompHeader ;
		internal PrintCustomerHeader prtCustHeader;
		internal PrintReportHeader prtHeader;
		internal PrintItemDetail prtDetail;
		internal PrintReportFooter prtFooter;
		internal PrintTaxSummary prtTaxSummary;
		internal PrintTotalAmount prtTotal;

		public PrintDocumentBase()
		{
			prtcompHeader = new PrintCompanyHeader();
			prtCustHeader = new PrintCustomerHeader();
			prtHeader = new PrintReportHeader();
			prtDetail = new PrintItemDetail ();
			prtFooter = new PrintReportFooter();
			prtTaxSummary= new PrintTaxSummary();
			prtTotal =new PrintTotalAmount ();

		}

		internal void PrintLongText(ref string test,string text)
		{
			if (text.Length > 42) {

				string temp = text.Substring(0,42);
				int pos = temp.LastIndexOf(" ");
				string line1 = text.Substring(0, pos);
				string line2 = text.Substring(pos);
				test = test + line1.Trim() + "\n";
				test = test + line2.Trim() + "\n";
			} else {

				test = test + text + "\n";
			}
		}

	}
}

