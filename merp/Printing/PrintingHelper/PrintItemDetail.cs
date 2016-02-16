using System;

namespace wincom.mobile.erp
{
	public class PrintItemDetail:PrintHelperBase
	{
		public string PrintDetail(InvoiceDtls itm,int count)
		{
			string test = "";
			string desc = itm.description;
			string pline2 =itm.icode.ToUpper()+"\n"+desc.ToUpper().Trim()+ "\n";
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

		public string PrintDetail_NTax(InvoiceDtls itm,int count)
		{
			string test = "";
			string desc = itm.description;
			string pline2 = desc.ToUpper().Trim()+ "\n";
			pline2 = pline2 + count.ToString ().PadRight (4, ' ');
			if (itm.qty < 0) {
				string sqty = "(EX)"+itm.qty.ToString ().Trim ()  ;
				pline2 = pline2 +sqty.PadLeft (9, ' ')+" ";
			}else  pline2 = pline2 + itm.qty.ToString ().PadLeft (9, ' ')+" ";

			pline2 = pline2 + Math.Round (itm.price, 2).ToString ("n2").PadLeft (12, ' ')+" ";
			pline2 = pline2 + Math.Round (itm.netamount, 2).ToString ("n2").PadLeft (14, ' ');
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

		public string PrintCNDetail_NTax(CNNoteDtls itm,int count)
		{
			string test = "";
			string desc = itm.description;
			string pline2 = desc.ToUpper().Trim()+ "\n";
			pline2 = pline2 + count.ToString ().PadRight (4, ' ');
			if (itm.qty < 0) {
				string sqty = "(EX)"+itm.qty.ToString ().Trim ()  ;
				pline2 = pline2 +sqty.PadLeft (9, ' ')+" ";
			}else  pline2 = pline2 + itm.qty.ToString ().PadLeft (9, ' ')+" ";
			pline2 = pline2 + Math.Round (itm.price, 2).ToString ("n2").PadLeft (12, ' ')+" ";
			pline2 = pline2 + Math.Round (itm.netamount, 2).ToString ("n2").PadLeft (14, ' ');
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

		public string PrintSODetail_NTax(SaleOrderDtls itm,int count)
		{
			string test = "";
			string desc = itm.description;
			string pline2 = desc.ToUpper().Trim()+ "\n";
			pline2 = pline2 + count.ToString ().PadRight (4, ' ');
			if (itm.qty < 0) {
				string sqty = "(EX)"+itm.qty.ToString ().Trim ()  ;
				pline2 = pline2 +sqty.PadLeft (9, ' ')+" ";
			}else  pline2 = pline2 + itm.qty.ToString ().PadLeft (9, ' ')+" ";
			pline2 = pline2 + Math.Round (itm.price, 2).ToString ("n2").PadLeft (12, ' ')+" ";
			pline2 = pline2 + Math.Round (itm.netamount, 2).ToString ("n2").PadLeft (14, ' ');
			test += pline2 + "\n";

			return test;
		}


		public string PrintDODetail(DelOrderDtls itm,int count)
		{
			string test = "";
			string desc = itm.description;
			string pline2 = desc.ToUpper ().Trim ();
			string scount = count.ToString ().PadRight (4, ' ');
			if (pline2.Length > 33) {

				string[] strs = pline2.Split (new char[]{ ' ' });
				string tmp = "";

				string sqty = itm.qty.ToString ("n").PadLeft (5, ' ');
				foreach (string s in strs) {
					if ((tmp + s + " ").Length > 33) {
						test  = test + scount + tmp.PadRight (33, ' ') + sqty+"\n"; 
						scount = "".PadRight (4, ' ');
						sqty = "".PadRight (5, ' ');
						tmp = s+" ";
					} else {
						tmp = tmp + s+" ";
					}
				}
				test = test + "".PadRight (4, ' ') + tmp + "\n";

			} else {
				test = count.ToString ().PadRight (4, ' ') + pline2.PadRight (33, ' ')+scount+ "\n";

			}

			return test;
		}
	}
}

