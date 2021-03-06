﻿using System;
using System.Collections;

namespace wincom.mobile.erp
{
	public class PrintESCPDocumentBase
	{
		internal Hashtable extrapara;
		internal string text;
		internal string errMsg;
		internal PrintESCPHeader prtcompHeader ;
		internal PrintESCDetails prtDetail;
		internal PrintESCPFooter prtFooter;
		internal PrintESCPTaxSummary prtTaxSumm;

		internal  AccessRights rights;

		public PrintESCPDocumentBase()
		{
			prtcompHeader = new PrintESCPHeader();
			prtDetail = new PrintESCDetails();
			prtFooter = new PrintESCPFooter();
			prtTaxSumm = new PrintESCPTaxSummary ();
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
				test = test + line1.Trim() + "\n";
				test = test + line2.Trim() + "\n";
			} else {

				test = test + text + "\n";
			}
		}

	}
}

