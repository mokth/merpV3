using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NVelocity.App;
using NVelocity;

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
		internal  AccessRights rights;

		public PrintDocumentBase()
		{
			prtcompHeader = new PrintCompanyHeader();
			prtCustHeader = new PrintCustomerHeader();
			prtHeader = new PrintReportHeader();
			prtDetail = new PrintItemDetail ();
			prtFooter = new PrintReportFooter();
			prtTaxSummary= new PrintTaxSummary();
			prtTotal =new PrintTotalAmount ();
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

		internal string GetInvoiceText_Template (string templatefilename,string pathToDatabase,string userID,Invoice inv, InvoiceDtls[] list)
		{
			string template = GetTemplateFile(templatefilename, pathToDatabase);
			if (string.IsNullOrEmpty (template))
				return "";

			MyModel model = new MyModel ();
			model.Customer = GetCustomer(inv.custcode,model,pathToDatabase);
			model.PrintDate = DateTime.Now;
			model.UserID = userID;
			model.Invoicehdr = inv;
			model.InvDtls = new List<InvoiceDtls> ();
			model.TaxSumm = new List<TaxInfo> ();
			foreach (var item in list) {
				model.InvDtls.Add (item);
			}
			GetInvTaxInfo (list, pathToDatabase, model);

			VelocityEngine fileEngine = new VelocityEngine();
			fileEngine.Init();

			VelocityContext context = new VelocityContext();
			StreamWriter ws = new StreamWriter(new MemoryStream());
			context.Put("util", new CustomTool());
			context.Put("model", model);
			fileEngine.Evaluate(context, ws, null, template);
			string text = "";
			ws.Flush();
			byte[] data = new byte[ws.BaseStream.Length-2];
			ws.BaseStream.Position = 2;
			int nread = ws.BaseStream.Read(data, 0, data.Length);
			text = Encoding.UTF8.GetString(data, 0, nread);
			ws.Close();
			return text;
		}

		internal string GetTemplateFile(string templateFilename,string pathToDatabase){
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

		internal void GetInvTaxInfo (InvoiceDtls[] list, string pathToDatabase, MyModel model)
		{
			List<Item> list2 = new List<Item> ();
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				list2 = db.Table<Item> ().ToList<Item> ();
			}
			var grp = from p in list
				group p by p.taxgrp into g
				select new {
				taxgrp = g.Key,
				ttltax = g.Sum (x => x.tax),
				ttlAmt = g.Sum (v => v.netamount)
			};
			foreach (var g in grp)
			{
				TaxInfo taxinfo = new TaxInfo();
				taxinfo.Tax = g.taxgrp.Trim();
				taxinfo.TaxPer =0;
				var list3 = list2.Where(x => x.taxgrp == g.taxgrp).ToList();
				if (list3.Count > 0)
				{
					taxinfo.TaxPer = list3[0].tax;
				}
				taxinfo.TaxAmt = g.ttltax;
				taxinfo.Amount = g.ttlAmt;
				model.TaxSumm.Add(taxinfo);

			}
		}

		internal Trader GetCustomer (string custcode,MyModel model,string pathToDatabase)
		{
			Trader cust = new Trader ();
			Trader comp = DataHelper.GetTrader (pathToDatabase, custcode);

			string tel = string.IsNullOrEmpty (comp.Tel) ? " " : comp.Tel.Trim ();
			string fax = string.IsNullOrEmpty (comp.Fax) ? " " : comp.Fax.Trim ();
			string addr1 =string.IsNullOrEmpty (comp.Addr1) ? "" : comp.Addr1.Trim ();
			string addr2 =string.IsNullOrEmpty (comp.Addr2) ? "" : comp.Addr2.Trim ();
			string addr3 =string.IsNullOrEmpty (comp.Addr3) ? "" : comp.Addr3.Trim ();
			string addr4 =string.IsNullOrEmpty (comp.Addr4) ? "" : comp.Addr4.Trim ();
			string gst =string.IsNullOrEmpty (comp.gst) ? "" : comp.gst.Trim ();

			cust = comp;

			string test = "";
			if (addr1!="")
				test += comp.Addr1.Trim () + "\n";	
			if (addr2!="")
				test += comp.Addr2.Trim () + "\n";	
			if (addr3!="")
				test += comp.Addr3.Trim () + "\n";	
			if (addr4!="")
				test += comp.Addr4.Trim () + "\n";	

			model.CustomerAddress = test;

			return cust;
		}
	}
}

