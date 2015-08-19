using System;

namespace wincom.mobile.erp
{
	public class PrintItemDetail:PrintHelperBase
	{
		public string PrintDetail(InvoiceDtls itm,int count)
		{
			string test = "";
			string desc = itm.description;
			string pline2 = desc.ToUpper().Trim()+ "\n";
			pline2 = pline2 + count.ToString ().PadRight (3, ' ');
			if (itm.qty < 0) {
				string sqty = "(EX)"+itm.qty.ToString ().Trim ()  ;
				pline2 = pline2 +sqty.PadLeft (9, ' ')+" ";
			}else  pline2 = pline2 + itm.qty.ToString ().PadLeft (9, ' ')+" ";
			pline2 = pline2 + Math.Round (itm.price, 2).ToString ("n2").PadLeft (8, ' ')+" ";
			string stax=Math.Round (itm.tax, 2).ToString ("n2") +" "+ itm.taxgrp;
			pline2 = pline2 + stax.PadLeft (10, ' ') + " ";
			pline2 = pline2 + Math.Round (itm.netamount, 2).ToString ("n2").PadLeft (9, ' ');
			test += pline2 + "\n";

			return test;
		}

		public string PrintCNDetail(CNNoteDtls itm,int count)
		{
			string test = "";
			string desc = itm.description;
			string pline2 = desc.ToUpper().Trim()+ "\n";
			pline2 = pline2 + count.ToString ().PadRight (3, ' ');
			if (itm.qty < 0) {
				string sqty = "(EX)"+itm.qty.ToString ().Trim ()  ;
				pline2 = pline2 +sqty.PadLeft (9, ' ')+" ";
			}else  pline2 = pline2 + itm.qty.ToString ().PadLeft (9, ' ')+" ";
			pline2 = pline2 + Math.Round (itm.price, 2).ToString ("n2").PadLeft (8, ' ')+" ";
			string stax=Math.Round (itm.tax, 2).ToString ("n2") +" "+ itm.taxgrp;
			pline2 = pline2 + stax.PadLeft (10, ' ') + " ";
			pline2 = pline2 + Math.Round (itm.netamount, 2).ToString ("n2").PadLeft (9, ' ');
			test += pline2 + "\n";

			return test;
		}

		public string PrintSODetail(SaleOrderDtls itm,int count)
		{
			string test = "";
			string desc = itm.description;
			string pline2 = desc.ToUpper().Trim()+ "\n";
			pline2 = pline2 + count.ToString ().PadRight (3, ' ');
			if (itm.qty < 0) {
				string sqty = "(EX)"+itm.qty.ToString ().Trim ()  ;
				pline2 = pline2 +sqty.PadLeft (9, ' ')+" ";
			}else  pline2 = pline2 + itm.qty.ToString ().PadLeft (9, ' ')+" ";
			pline2 = pline2 + Math.Round (itm.price, 2).ToString ("n2").PadLeft (8, ' ')+" ";
			string stax=Math.Round (itm.tax, 2).ToString ("n2") +" "+ itm.taxgrp;
			pline2 = pline2 + stax.PadLeft (10, ' ') + " ";
			pline2 = pline2 + Math.Round (itm.netamount, 2).ToString ("n2").PadLeft (9, ' ');
			test += pline2 + "\n";

			return test;
		}
	}
}

