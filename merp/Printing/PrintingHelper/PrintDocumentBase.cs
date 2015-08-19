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

	}
}

