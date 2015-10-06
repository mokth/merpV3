using System;
using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;
using System.Linq;
using System.Linq.Expressions;

namespace wincom.mobile.erp
{
	public class PrintPCLTaxSummary:PrintPCLBase
	{
		public void PrintTaxSumm(ref string test,InvoiceDtls[] list )
		{
			List<Item> list2 = new List<Item> ();
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				list2 = db.Table<Item> ().ToList<Item> ();
			}
			var grp = from p in list
				group p by p.taxgrp into g
				select new {taxgrp = g.Key, ttltax = g.Sum (x => x.tax),ttlAmt = g.Sum (v => v.netamount)};


			test += "------------------------------------------------\r";
			test += "SUMMARY  TAX GROUP             AMOUNT   TAX AMT \r";
			test += "------------------------------------------------\r";
			//       12345678 123456789012345 123456789012 1234567890 
			string pline="";
			foreach (var g in grp) {
				var list3 =list2.Where (x => x.taxgrp == g.taxgrp).ToList ();
				if (list3.Count > 0) {
					string stax = g.taxgrp.Trim () + " @ " + list3 [0].tax.ToString () + "%";
					pline = pline + stax.PadRight (15,' ');
				} else pline = pline + g.taxgrp.Trim().PadRight (15, ' ');
				pline = pline + g.ttlAmt.ToString("n2").PadLeft(12, ' ')+" ";
				pline = pline + g.ttltax.ToString("n2").PadLeft(10, ' ');
				test += "".PadRight(9,' ')+pline + "\r";
				pline = "";
			}

		}

		public void PrintCNTaxSumm(ref string test,CNNoteDtls[] list )
		{
			List<Item> list2 = new List<Item> ();
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				list2 = db.Table<Item> ().ToList<Item> ();
			}
			var grp = from p in list
				group p by p.taxgrp into g
				select new {taxgrp = g.Key, ttltax = g.Sum (x => x.tax),ttlAmt = g.Sum (v => v.netamount)};


			test += "------------------------------------------------\r";
			test += "SUMMARY  TAX GROUP             AMOUNT   TAX AMT \r";
			test += "------------------------------------------------\r";
			//       12345678 123456789012345 123456789012 1234567890 
			string pline="";
			foreach (var g in grp) {
				var list3 =list2.Where (x => x.taxgrp == g.taxgrp).ToList ();
				if (list3.Count > 0) {
					string stax = g.taxgrp.Trim () + " @ " + list3 [0].tax.ToString () + "%";
					pline = pline + stax.PadRight (15,' ');
				} else pline = pline + g.taxgrp.Trim().PadRight (15, ' ');
				pline = pline + g.ttlAmt.ToString("n2").PadLeft(12, ' ')+" ";
				pline = pline + g.ttltax.ToString("n2").PadLeft(10, ' ');
				test += "".PadRight(9,' ')+pline + "\r";
				pline = "";
			}

		}


	}
}

