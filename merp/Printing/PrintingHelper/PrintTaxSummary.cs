using System;
using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;
using System.Linq;
using System.Linq.Expressions;

namespace wincom.mobile.erp
{
	public class PrintTaxSummary:PrintHelperBase
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

			test += "SUMMARY\n";
			test += "-------------------------------\n";
			test += "TAX            AMOUNT   TAX AMT\n";
			test += "-------------------------------\n";
			//       123456789 12345678901 123456789 
			string pline="";
			foreach (var g in grp) {
				var list3 =list2.Where (x => x.taxgrp == g.taxgrp).ToList ();
				if (list3.Count > 0) {
					string stax = g.taxgrp.Trim () + " @ " + list3 [0].tax.ToString () + "%";
					pline = pline + stax.PadRight (10,' ');
				} else pline = pline + g.taxgrp.Trim().PadRight (10, ' ');
				pline = pline + g.ttlAmt.ToString("n2").PadLeft(11, ' ')+" ";
				pline = pline + g.ttltax.ToString("n2").PadLeft(9, ' ');
				test += pline + "\n";
				pline = "";
			}
			test += "-------------------------------\n";
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

			test += "SUMMARY\n";
			test += "-------------------------------\n";
			test += "TAX            AMOUNT   TAX AMT\n";
			test += "-------------------------------\n";
			//       123456789 12345678901 123456789 
			string pline="";
			foreach (var g in grp) {
				var list3 =list2.Where (x => x.taxgrp == g.taxgrp).ToList ();
				if (list3.Count > 0) {
					string stax = g.taxgrp.Trim () + " @ " + list3 [0].tax.ToString () + "%";
					pline = pline + stax.PadRight (10,' ');
				}else pline = pline + g.taxgrp.Trim().PadRight (10, ' ');
				pline = pline + g.ttlAmt.ToString("n2").PadLeft(11, ' ')+" ";
				pline = pline + g.ttltax.ToString("n2").PadLeft(9, ' ');
				test += pline + "\n";
				pline = "";
			}
			test += "-------------------------------\n";
		}

		public void PrintSOTaxSumm(ref string test,SaleOrderDtls[] list )
		{
			List<Item> list2 = new List<Item> ();
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				list2 = db.Table<Item> ().ToList<Item> ();
			}
			var grp = from p in list
				group p by p.taxgrp into g
				select new {taxgrp = g.Key, ttltax = g.Sum (x => x.tax),ttlAmt = g.Sum (v => v.netamount)};

			test += "SUMMARY\n";
			test += "-------------------------------\n";
			test += "TAX            AMOUNT   TAX AMT\n";
			test += "-------------------------------\n";
			//       123456789 12345678901 123456789 
			string pline="";
			foreach (var g in grp) {
				var list3 =list2.Where (x => x.taxgrp == g.taxgrp).ToList ();
				if (list3.Count > 0) {
					string stax = g.taxgrp.Trim () + " @ " + list3 [0].tax.ToString () + "%";
					pline = pline + stax.PadRight (10,' ');
				} else pline = pline + g.taxgrp.Trim().PadRight (10, ' ');
				pline = pline + g.ttlAmt.ToString("n2").PadLeft(11, ' ')+" ";
				pline = pline + g.ttltax.ToString("n2").PadLeft(9, ' ');
				test += pline + "\n";
				pline = "";
			}
			test += "-------------------------------\n";
		}
	}
}

