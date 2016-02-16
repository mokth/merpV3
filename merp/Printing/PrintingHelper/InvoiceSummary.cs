using System;
using System.Data;
using System.Xml.Linq;
using System.Linq;
using System.Linq.Expressions;
using Android.App;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NVelocity.App;
using NVelocity;

namespace wincom.mobile.erp
{
	public class InvoiceSummary
	{
		
		public static string GetInvoiceSumm_Template(string template,string pathToDatabase,string userID, DateTime printDate1,DateTime printDate2)
		{
			string text = "";
			try {
				ModelSumm model = GetInvoiceSummModel (pathToDatabase, userID, printDate1, printDate2);
				model.ItemsSumm = SummHelper.GetItemsSummary(pathToDatabase, printDate1);
				VelocityEngine fileEngine = new VelocityEngine ();
				fileEngine.Init ();
				string content = GetTemplateFile (template, pathToDatabase);
				VelocityContext context = new VelocityContext ();
				StreamWriter ws = new StreamWriter (new MemoryStream ());
				context.Put ("util", new CustomTool ());
				context.Put ("model", model);
				fileEngine.Evaluate (context, ws, null, content);
				ws.Flush ();
				byte[] data = new byte[ws.BaseStream.Length - 2];
				ws.BaseStream.Position = 2;
				int nread = ws.BaseStream.Read (data, 0, data.Length);
				text = Encoding.UTF8.GetString (data, 0, nread);
				ws.Close ();
			} catch (Exception ex) {
			
			}
			return text;
		}

		internal static string GetTemplateFile(string templateFilename,string pathToDatabase){
			string content = "";

			try{
				String pathname = Path.GetDirectoryName (pathToDatabase);
				string filename = Path.Combine (pathname, templateFilename);

				if (File.Exists (filename)) {
					using (Stream sr = File.OpenRead (filename)) {
						using (MemoryStream ms = new MemoryStream ()) {
							sr.CopyTo (ms);
							byte[] data = ms.ToArray ();
							content = Encoding.UTF8.GetString (ms.ToArray (), 0, data.Length);
						}
					}
				}
			}
			catch{

			}

			return content;
		}

		public static ModelSumm GetInvoiceSummModel(string pathToDatabase,string userID, DateTime printDate1,DateTime printDate2)
		{
			ModelSumm model = new ModelSumm ();
			model.DailyTrx = new List<ModelSummDate> ();
			model.GrpPrices = new List<ModelGrpPrice> ();

			var itemCodes = DataHelper.GetItems ();
			var invs = DataHelper.GetInvoices (printDate1, printDate2);
			var cns = DataHelper.GetCNNote (printDate1, printDate2);
			List<string> invnos = new List<string> ();
			foreach (var itm in invs) {  
				invnos.Add (itm.invno);
			}

			List<string> cnnosTmp = new List<string> ();
			foreach (var cnitm in cns) {
				cnnosTmp.Add (cnitm.cnno);
			}

			using (var db = new SQLite.SQLiteConnection (pathToDatabase, true)) {
				var itemlist = from p in db.Table<InvoiceDtls> ()
				               where invnos.Contains (p.invno)
				               select p;
				
				var cnitemlist = from p in db.Table<CNNoteDtls> ()
				                 where cnnosTmp.Contains (p.cnno)
				                 select p;
				
				model.PrintDate = DateTime.Now;
				model.UserID = "MOK";
				model.Company = db.Table<CompanyInfo> ().Take (1).FirstOrDefault ();
				model.TotalCash = GetSumTotal (invs, "CASH", itemlist.ToList (), itemCodes, model);
				model.TotalInv = GetSumTotal (invs, "INVOICE", itemlist.ToList (), itemCodes, model);
				model.TotalCNInv = GetCNSumTotal (cns, "INVOICE", cnitemlist.ToList (), itemCodes, model);
				model.TotalCNCash = GetCNSumTotal (cns, "CASH", cnitemlist.ToList (), itemCodes, model);

				var grpitm = from code in itemlist
					group code by code.icode
					into g
					select new { key = g.Key, idesc=g.Max(x=>x.description),results = g };


				foreach (var grpicode in grpitm)
				{
					var grpprice =from  icode in grpicode.results 
						group icode by icode.price into g
						select new { key = g.Key,
						tax=g.Sum(x=>x.tax),
						amount=g.Sum(x=>x.netamount), 
						qty = g.Sum(x => x.qty), 
						results = g };

					ModelGrpPrice mprice = new ModelGrpPrice();
					mprice.ICode = grpicode.key;
					mprice.IDesc = grpicode.idesc;
					mprice.PriceList = new List<GrpPriceList>();

					foreach (var itm in grpprice) {
						GrpPriceList gprice = new GrpPriceList();
						gprice.Amount = itm.amount;
						gprice.Price = itm.key;
						gprice.Qty = itm.qty;
						gprice.TaxAmt = itm.tax;
						mprice.PriceList.Add(gprice);
					}
					model.GrpPrices.Add(mprice);
				}

//				var grp = from inv in invs
//					group inv by inv.invdate into g
//					select new { key = g.Key, results = g };


				ModelSummDate summ = new ModelSummDate ();
				summ.Date = printDate1;
				summ.CashList = new List<Invoice> ();
				summ.InvList = new List<Invoice> ();

				var typgrp = from ty in invs
				              group ty by ty.trxtype into tg
				              select new { key = tg.Key, results = tg };
				foreach (var g1 in typgrp) {
					if (g1.key == "CASH")
						summ.CashList = g1.results.OrderBy (x => x.invno).ToList ();
					else
						summ.InvList = g1.results.OrderBy (x => x.invno).ToList ();
				}

				var typgrp2 = from ty in cns
				              group ty by ty.trxtype into tg
				              select new { key = tg.Key, results = tg };
				foreach (var g1 in typgrp2) {
					if (g1.key == "CASH")
						summ.CashCNList = g1.results.OrderBy (x => x.cnno).ToList ();
					else
						summ.InvCNList = g1.results.OrderBy (x => x.cnno).ToList ();
				}

				model.DailyTrx.Add (summ);
				model.TtlTaxSumm = new List<TaxSumm> ();
				model.TtlTaxSumm = GeTotalTaxSumm(model.TtlTaxSumm, model.InvTaxSumm);
				model.TtlTaxSumm = GeTotalTaxSumm(model.TtlTaxSumm, model.CSTaxSumm);
				model.TtlTaxSumm = GeTotalTaxSumm(model.TtlTaxSumm, model.CNInvTaxSumm);
				model.TtlTaxSumm = GeTotalTaxSumm(model.TtlTaxSumm, model.CNCSTaxSumm);

			}
			return  model;
		}

		private static double GetSumTotal (Invoice[] invs,string trxtype,List<InvoiceDtls> itemlist,Item[] itemCodes,ModelSumm model)
		{
			double total = 0;
			var sum1 = from p in invs
					where p.trxtype == trxtype
			group p by 1 into g
			select new {
				amt = g.Sum (x => x.amount),
				tax = g.Sum (x => x.taxamt),
				grplist = g.ToList()
			};

			if (sum1.Count() >0)
				total = sum1.FirstOrDefault().amt + sum1.FirstOrDefault().tax;

			List<string> invnosTmp = new List<string>();
			foreach (var grptmp in sum1)
			{
				foreach (var lst in grptmp.grplist){
					invnosTmp.Add(lst.invno);
				}
			}

			var itemlistCS = from p in itemlist
					where invnosTmp.Contains(p.invno)
				select p;

			List<TaxSumm> taxsummCS = GetSubTaxSumm(itemCodes, itemlistCS.ToList());
			if (trxtype=="CASH")
				model.CSTaxSumm = taxsummCS;
			else model.InvTaxSumm = taxsummCS;

			return total;
		}

		private static double GetCNSumTotal (CNNote[] invs,string trxtype,List<CNNoteDtls> itemlist,Item[] itemCodes,ModelSumm model)
		{
			double total = 0;
			var sum1 = from p in invs
					where p.trxtype == trxtype
				group p by 1 into g
				select new {
				amt = g.Sum (x => x.amount),
				tax = g.Sum (x => x.taxamt),
				grplist = g.ToList()
			};


			if (sum1.Count() >0)
				total = sum1.FirstOrDefault().amt + sum1.FirstOrDefault().tax;

			List<string> invnosTmp = new List<string>();
			foreach (var grptmp in sum1)
			{
				foreach (var lst in grptmp.grplist){
					invnosTmp.Add(lst.invno);
				}
			}

			var itemlistCS = from p in itemlist
					where invnosTmp.Contains(p.cnno)
				select p;

			List<TaxSumm> taxsummCS = GetSubCNTaxSumm(itemCodes, itemlistCS.ToList());
			if (trxtype=="CASH")
				model.CNCSTaxSumm = taxsummCS;
			else model.CNInvTaxSumm = taxsummCS;

			return total;
		}

//		private static double GetCNSumTotal (CNNote[] invs,string trxtype)
//		{
//			double total = 0;
//			var sum1 = from p in invs
//					where p.trxtype == trxtype
//				group p by 1 into g
//				select new {
//				amt = g.Sum (x => x.amount),
//				tax = g.Sum (x => x.taxamt)
//			};
//
//			if (sum1.Count() >0)
//				total = sum1.FirstOrDefault().amt + sum1.FirstOrDefault().tax;
//
//			return total;
//		}

		static List<TaxSumm> GeTotalTaxSumm(List<TaxSumm> summ,List<TaxSumm> source)
		{
			foreach (var itm in source)
			{
				if (summ.Where(x => x.TaxGrp == itm.TaxGrp).Count() == 0)
				{
					TaxSumm s = new TaxSumm();
					s.TaxGrp = itm.TaxGrp;
					s.Amount = itm.Amount;
					s.TaxAmt =  itm.TaxAmt;
					s.TaxDesc = itm.TaxDesc;
					summ.Add(s);
				}
				else
				{
					var taxsum =summ.Where(x => x.TaxGrp == itm.TaxGrp).FirstOrDefault();
					taxsum.Amount = taxsum.Amount + itm.Amount;
					taxsum.TaxAmt = taxsum.TaxAmt + itm.TaxAmt;
				}
			}

			return summ;
		}

		static List<TaxSumm> GetSubTaxSumm(Item[] itemcodes, List<InvoiceDtls> invs) 
		{
			List<TaxSumm> summ = new List<TaxSumm>();
			var grp = from p in invs
				group p by p.taxgrp into g
				select new { taxgrp = g.Key, ttltax = g.Sum(x => x.tax), ttlAmt = g.Sum(v => v.netamount) };
			foreach (var g in grp)
			{
				TaxSumm tsumm = new TaxSumm();
				var list3 = itemcodes.Where(x => x.taxgrp == g.taxgrp).ToList();
				if (list3.Count > 0)
				{

					tsumm.TaxDesc = g.taxgrp.Trim() + " @ " + list3[0].tax.ToString() + "%";
				}
				else tsumm.TaxDesc= g.taxgrp.Trim();
				tsumm.TaxAmt = g.ttltax;
				tsumm.Amount = g.ttlAmt;
				tsumm.TaxGrp = g.taxgrp;
				summ.Add(tsumm);
			}

			return summ;
		}

		static List<TaxSumm> GetSubCNTaxSumm(Item[] itemcodes, List<CNNoteDtls> invs)
		{
			List<TaxSumm> summ = new List<TaxSumm>();
			var grp = from p in invs
				group p by p.taxgrp into g
				select new { taxgrp = g.Key, ttltax = g.Sum(x => x.tax), ttlAmt = g.Sum(v => v.netamount) };
			foreach (var g in grp)
			{
				TaxSumm tsumm = new TaxSumm();
				var list3 = itemcodes.Where(x => x.taxgrp == g.taxgrp).ToList();
				if (list3.Count > 0)
				{

					tsumm.TaxDesc = g.taxgrp.Trim() + " @ " + list3[0].tax.ToString() + "%";
				}
				else tsumm.TaxDesc = g.taxgrp.Trim();
				tsumm.TaxAmt = g.ttltax;
				tsumm.Amount = g.ttlAmt;
				tsumm.TaxGrp = g.taxgrp;
				summ.Add(tsumm);
			}

			return summ;
		}
	}
}

