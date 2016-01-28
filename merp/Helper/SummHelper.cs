using System;
using System.Linq;
using Android.Runtime;
using WcfServiceItem;
using Android.App;
using System.Collections.Generic;

namespace wincom.mobile.erp
{
	public class SummHelper
	{
		/*  public double BF {get;set;}    //Stock from Yesterday
            public double NSTK { get; set; }  //New Stock Received by Actual Qty
            public double SSTK { get; set; }  //Latest Online receive qty
            public double DSTK { get; set; }  //BF - NSTK
            public double DSL { get; set; }   //SSL -ASL
            public double SAMT { get; set; }  //SSL * Unit price
            public double SSL { get; set; }  //Sales qty base on invoice
            public double SRT { get; set; }  //Return qty base on invoice (EXCH) and CN
            public double SBL { get; set; }  //SSTK - SSL - SRT
            public double AAMT { get; set; }  //ASL * UnitPrice
            public double ASL { get; set; }  //BF +NSTK -ART - ABL
            public double ART { get; set; }  //Return Qty base on actual count
            public double ABL { get; set; } //Balance qty base on actual count
            public double DAMT { get; set; } //SAMT - AAMT
		 */
		public static List<ItemStkSummary>  GetItemsSummary (string pathToDatabase,DateTime printdate1)
		{
			List<ItemStkSummary> itmsumms = new List<ItemStkSummary> ();
			using (var db = new SQLite.SQLiteConnection (pathToDatabase, true)) {
				var listinv = db.Table<Invoice> ().Where (x => x.invdate == printdate1).OrderBy (x => x.invdate).ToList<Invoice> ();
				List<string> invnos = new List<string> ();
				foreach (var itm in listinv) {
					invnos.Add (itm.invno);
				}

				var listcn = db.Table<CNNote> ().Where (x => x.invdate >= printdate1 ).OrderBy (x => x.invdate).ToList<CNNote> ();
				List<string> cnnos = new List<string> ();
				foreach (var itm in listcn) {
					cnnos.Add (itm.cnno);
				}

				var itemlist = from p in db.Table<InvoiceDtls> ()
				               where invnos.Contains (p.invno)
				               select p;

				var sumSales = from p in itemlist
				               where p.qty > 0
				               group p by p.icode into g
				               select new
				{
					key = g.Key,
					qty = g.Sum (x => x.qty),
					uprice = g.Max (x => x.price)
				};

				var sumExch = from p in itemlist
				              where p.qty < 0
				              group p by p.icode into g
				              select new
				{
					key = g.Key,
					qty = g.Sum (x => x.qty)
				};


				var itemcnlist = from p in db.Table<CNNoteDtls> ()
				                 where invnos.Contains (p.cnno)
				                 select p;

				var sumCN = from p in itemlist
				            where p.qty > 0
				            group p by p.icode into g
				            select new
				{
					key = g.Key,
					qty = g.Sum (x => x.qty)
				};

				List<string> icodes = new List<string>();
				foreach (var itm in sumSales)
				{
					icodes.Add(itm.key);
				}

				//var list = db.Table<ItemStock>().Where(x => x.DateTrx == printdate1).ToList();
				var list = from p in db.Table<ItemStock>()
						where icodes.Contains(p.ICode) && p.DateTrx == printdate1
					select p;

				//var list = db.Table<ItemStock> ().Where (x => x.DateTrx == printdate1).ToList ();
				double ttlsales = 0;
				double ttlCN = 0;
				double ttlexch = 0;
				double unitprice = 0;
				foreach (var itm in list) {
					double UnitPrice = 1;
					//  Console.WriteLine("{0} {1}",itm.ICode,itm.DateTrx);
					ItemStkSummary itmsumm = new ItemStkSummary ();

					ttlsales = 0;
					ttlCN = 0;
					ttlexch = 0;
					unitprice = 0;
					var list1 = sumExch.Where (x => x.key == itm.ICode).ToList ();
					var list2 = sumCN.Where (x => x.key == itm.ICode).ToList ();
					var list3 = sumSales.Where (x => x.key == itm.ICode).ToList ();
					if (list1.Count > 0) {
						ttlexch = Math.Abs (list1 [0].qty);
					}
					if (list2.Count > 0) {
						ttlCN = list2 [0].qty;
					}
					if (list3.Count > 0) {
						ttlsales = list3 [0].qty;
						unitprice = list3 [0].uprice;
					}

					itmsumm.ICODE = itm.ICode;
					itmsumm.DESC = itm.IDesc;
					itmsumm.BF = itm.QtyBrf;
					itmsumm.NSTK = itm.QtyAct;
					itmsumm.SSTK = itm.QtyGR;
					itmsumm.DSTK = itmsumm.SSTK - itmsumm.BF - itmsumm.NSTK;
					itmsumm.SSL = ttlsales;
					itmsumm.SRT = ttlCN + ttlexch;
					itmsumm.SBL = itmsumm.SSTK - itmsumm.SSL - itmsumm.SRT;
					itmsumm.ASL = itmsumm.BF + itmsumm.NSTK - itmsumm.ART - itmsumm.ABL;
					itmsumm.ART = itm.QtyRtr;
					itmsumm.ABL = itm.QtyBal;
					;
					itmsumm.DSL = itmsumm.SSL - itmsumm.ASL;
					;
					itmsumm.SAMT = itmsumm.SSL * UnitPrice;
					itmsumm.AAMT = itmsumm.ASL * UnitPrice;
					itmsumm.DAMT = itmsumm.SAMT - itmsumm.AAMT;

					Console.WriteLine ("{0} {1}", itm.ICode, itm.IDesc);
					Console.WriteLine ("  {0}\t{1}\t{2}\t{3}\t{4}\t{5}", itmsumm.BF, itmsumm.NSTK, itmsumm.SSTK, itmsumm.DSTK, itmsumm.DSL, itmsumm.SAMT);
					Console.WriteLine ("  {0}\t{1}\t{2}\t\t\t{3}", itmsumm.SSL, itmsumm.SRT, itmsumm.SBL, itmsumm.AAMT);
					Console.WriteLine ("  {0}\t{1}\t{2}\t\t\t{3}", itmsumm.ASL, itmsumm.ART, itmsumm.ABL, itmsumm.DAMT);
					itmsumms.Add (itmsumm);
				}

				return itmsumms;
			}
		}
	}
}

