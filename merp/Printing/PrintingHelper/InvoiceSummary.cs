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
			ModelSumm model = new ModelSumm();
			model.DailyTrx = new List<ModelSummDate>();
			model.GrpPrices = new List<ModelGrpPrice>();

			var invs = DataHelper.GetInvoices (printDate1, printDate2);
			var cns = DataHelper.GetCNNote(printDate1, printDate2);
			List<string> invnos = new List<string>();
			foreach(var itm in invs)
			{  
				invnos.Add(itm.invno);
			}

			using (var db = new SQLite.SQLiteConnection (pathToDatabase, true)) {
				model.PrintDate = DateTime.Now;
				model.UserID = "MOK";
				model.Company = db.Table<CompanyInfo> ().Take (1).FirstOrDefault ();
				model.TotalCash = GetSumTotal (invs, "CASH");
				model.TotalInv = GetSumTotal (invs, "INVOICE");

				var itemlist = from p in db.Table<InvoiceDtls> ()
						where invnos.Contains (p.invno)
					select p;

				var grpitm = from code in itemlist
					group code by new { code.icode, code.price }
					into g select new { key = g.Key, qty = g.Sum (x => x.qty), results = g };

				foreach (var gi in grpitm.OrderBy(x=>x.key.icode)) {
					ModelGrpPrice mprice = new ModelGrpPrice ();
					mprice.ICode = gi.key.icode;
					mprice.Price = gi.key.price;
					mprice.Qty = gi.qty;
					model.GrpPrices.Add (mprice);
				}

				var grp = from inv in invs
					group inv by inv.invdate into g
					select new { key = g.Key, results = g };

				foreach (var g in grp) {
					ModelSummDate summ = new ModelSummDate ();
					summ.Date = g.key;
					summ.CashList = new List<Invoice> ();
					summ.InvList = new List<Invoice> ();

					var typgrp = from ty in g.results
						group ty by ty.trxtype into tg
						select new { key = tg.Key, results = tg };
					foreach (var g1 in typgrp) {
						if (g1.key == "CASH")
							summ.CashList = g1.results.OrderBy (x => x.invno).ToList ();
						else
							summ.InvList = g1.results.OrderBy (x => x.invno).ToList ();

					}

					model.DailyTrx.Add (summ);
				}
			}
			return  model;
		}
		private static double GetSumTotal (Invoice[] invs,string trxtype)
		{
			double total = 0;
			var sum1 = from p in invs
					where p.trxtype == trxtype
			group p by 1 into g
			select new {
				amt = g.Sum (x => x.amount),
				tax = g.Sum (x => x.taxamt)
			};

			if (sum1.Count() >0)
				total = sum1.FirstOrDefault().amt + sum1.FirstOrDefault().tax;
			
			return total;
		}
	}
}

