using System;
using System.Collections;

namespace wincom.mobile.erp
{
	public class PrintPCLDocumentBase
	{
		internal Hashtable extrapara;
		internal string text;
		internal string errMsg;
		internal PrintPCLHeader prtcompHeader ;
		internal PrintPCLDetails prtDetail;
		internal PrintPCLFooter prtFooter;
		internal PrintPCLTaxSummary prtTaxSumm;

		internal  AccessRights rights;

		public PrintPCLDocumentBase()
		{
			prtcompHeader = new PrintPCLHeader();
			prtDetail = new PrintPCLDetails();
			prtFooter = new PrintPCLFooter();
			prtTaxSumm = new PrintPCLTaxSummary ();
			rights =Utility.GetAccessRights ();

		}

		internal bool iSPrintCompLogo()
		{
			return rights.IsPrintCompLogo;		
		}

		internal void PrintLongText(ref string test,string text)
		{
			if (text.Length > 42) {

				string temp = text.Substring(0,42);
				int pos = temp.LastIndexOf(" ");
				string line1 = text.Substring(0, pos);
				string line2 = text.Substring(pos);
				test = test + line1.Trim() + "\r";
				test = test + line2.Trim() + "\r";
			} else {

				test = test + text + "\r";
			}
		}

	}
}

