using System;
using System.Linq;
using Android.Runtime;
using WcfServiceItem;
using Android.App;
using System.Collections.Generic;

namespace wincom.mobile.erp
{
	public class DataHelper
	{
		public static AdUser GetUser(string pathToDatabase)
		{
	  	AdUser user=null;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list2 = db.Table<AdUser> ().ToList<AdUser> ();
				if (list2.Count > 0) {
					user = list2 [0];
				}
			}
		
			return user;
		}

		public static void UpdateLastConnect(string pathToDatabase)
		{
			AdUser user=null;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list2 = db.Table<AdUser> ().ToList<AdUser> ();
				if (list2.Count > 0) {
					user = list2 [0];
					user.LastConnect = DateTime.Now;
				   db.Update(user);	
				}
			}


		}


		public static int GetLastInvRunNo(string pathToDatabase, DateTime invdate,string trxtype )
		{
			DateTime Sdate = invdate.AddDays (1 - invdate.Day);
			DateTime Edate = new DateTime (invdate.Year, invdate.Month, DateTime.DaysInMonth (invdate.Year, invdate.Month));
			int runno = -1;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list = db.Table<Invoice> ().Where(x=>x.invdate>=Sdate && x.invdate<=Edate && x.trxtype==trxtype)
						    //.OrderByDescending(x=>x.invdate)
						    //.ThenByDescending (x => x.created)
					        .ToList<Invoice> ();
				List<string> list2 = new List<string> ();
				foreach (var inv in list) {
					if (inv.invno.Length > 5) {
						list2.Add (inv.invno);
					}
				}

				if (list2.Count > 0) {
					//string invno =list2[0].invno;
					string invno = list2.Max (x => x.Substring(x.Length - 4));
					runno = Convert.ToInt32(invno);
					//if (invno.Length > 5)
				//	{
				//		string srunno = invno.Substring(invno.Length - 4);
				
				//	}
				}
			}

			return runno;
		}

		public static int GetLastSORunNo(string pathToDatabase, DateTime sodate )
		{
			DateTime Sdate = sodate.AddDays (1 - sodate.Day);
			DateTime Edate = new DateTime (sodate.Year, sodate.Month, DateTime.DaysInMonth (sodate.Year, sodate.Month));
			int runno = -1;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list = db.Table<SaleOrder> ().Where(x=>x.sodate>=Sdate && x.sodate<=Edate)
					.OrderByDescending(x=>x.sono)
					.ToList<SaleOrder> ();

				List<string> list2 = new List<string> ();
				foreach (var so in list) {
					if (so.sono.Length > 5) {
						list2.Add (so.sono);
					}
				}

				if (list2.Count > 0) {
					string sono = list2.Max (x => x.Substring(x.Length - 4));
					runno = Convert.ToInt32(sono);
					//string sono =list2[0].sono;
					//if (sono.Length > 5)
					//{
					//	string srunno = sono.Substring(sono.Length - 4);
					//	runno = Convert.ToInt32(srunno);
					//}
				}
			}

			return runno;
		}

		public static int GetLastDORunNo(string pathToDatabase, DateTime dodate )
		{
			DateTime Sdate = dodate.AddDays (1 - dodate.Day);
			DateTime Edate = new DateTime (dodate.Year, dodate.Month, DateTime.DaysInMonth (dodate.Year, dodate.Month));
			int runno = -1;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list = db.Table<DelOrder> ().Where(x=>x.dodate>=Sdate && x.dodate<=Edate)
					.OrderByDescending(x=>x.dono)
					.ToList<DelOrder> ();

				List<string> list2 = new List<string> ();
				foreach (var doord in list) {
					if (doord.dono.Length > 5) {
						list2.Add (doord.dono);
					}
				}

				if (list2.Count > 0) {
					string dono = list2.Max (x => x.Substring(x.Length - 4));
					runno = Convert.ToInt32(dono);
//					string dono =list2[0].dono;
//					if (dono.Length > 5)
//					{
//						string srunno = dono.Substring(dono.Length - 4);
//						runno = Convert.ToInt32(srunno);
//					}
				}
			}

			return runno;
		}

		public static int GetLastCNRunNo(string pathToDatabase, DateTime invdate )
		{
			DateTime Sdate = invdate.AddDays (1 - invdate.Day);
			DateTime Edate = new DateTime (invdate.Year, invdate.Month, DateTime.DaysInMonth (invdate.Year, invdate.Month));
			int runno = -1;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list= db.Table<CNNote> ().Where(x=>x.invdate>=Sdate && x.invdate<=Edate)
					.OrderByDescending(x=>x.cnno)
					.ToList<CNNote> ();

				List<string> list2 = new List<string> ();
				string cnno = "";
				foreach (var cn in list) {
					cnno = cn.cnno.Replace ("(INV)", "").Replace ("(CS)", "");
					if (cnno.Length > 5) {
						list2.Add (cnno);
					}
				}

				if (list2.Count > 0) {
					string sno = list2.Max (x => x.Substring(x.Length - 4));
					runno = Convert.ToInt32(sno);
					//string invno =list2[0].cnno;
					//invno = invno.Replace ("(INV)", "");
					//invno = invno.Replace ("(CS)", "");
//					if (invno.Length > 5)
//					{
//						string srunno = invno.Substring(invno.Length - 4);
//						runno = Convert.ToInt32(srunno);
//					}
				}
			}

			return runno;
		}


		public static bool GetInvoicePrintStatus(string pathToDatabase,string invno,AccessRights rights)
		{
			bool iSPrinted = false;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				
				if (rights.InvNotEditAftPrint) {
					var list = db.Table<Invoice> ().Where (x => x.invno == invno).ToList ();
					if (list.Count > 0) {
						iSPrinted = list [0].isPrinted; 				
					}
				}
			}
			return iSPrinted;
		}

		public static bool GetDelOderPrintStatus(string pathToDatabase,string dono,AccessRights rights)
		{
			bool iSPrinted = false;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {

				if (rights.DONotEditAftPrint) {
					var list = db.Table<DelOrder> ().Where (x => x.dono == dono).ToList ();
					if (list.Count > 0) {
						iSPrinted = list [0].isPrinted; 				
					}
				}
			}
			return iSPrinted;
		}

		public static bool GetCNNotePrintStatus(string pathToDatabase,string cnno,AccessRights rights)
		{
			bool iSPrinted = false;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var info = db.Table<CompanyInfo> ().FirstOrDefault ();
				if (rights.CNNotEditAftPrint) {
					var list = db.Table<CNNote> ().Where (x => x.cnno == cnno).ToList ();
					if (list.Count > 0) {
						iSPrinted = list [0].isPrinted; 				
					}
				}
			}
			return iSPrinted;
		}

		public static bool GetSaleOrderPrintStatus(string pathToDatabase,string sono,AccessRights rights)
		{
			bool iSPrinted = false;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var info = db.Table<CompanyInfo> ().FirstOrDefault ();
				if (rights.CNNotEditAftPrint) {
					var list = db.Table<SaleOrder> ().Where (x => x.sono == sono).ToList ();
					if (list.Count > 0) {
						iSPrinted = list [0].isPrinted; 				
					}
				}
			}
			return iSPrinted;
		}

		public static CompanyInfo GetCompany(string pathToDatabase)
		{
			CompanyInfo info = null;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				info = db.Table<CompanyInfo> ().FirstOrDefault ();
			}
			return info;
		}

		public static Trader GetTrader(string pathToDatabase,string custcode)
		{
			Trader info = null;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				info = db.Table<Trader> ().Where(x=>x.CustCode==custcode).FirstOrDefault ();
			}
			return info;
		}


		public static AdPara GetAdPara(string pathToDatabase)
		{
			AdPara info = null;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				info = db.Table<AdPara> ().FirstOrDefault ();
			}
			if (info == null) {
				info = new AdPara ();
			}
			if (string.IsNullOrEmpty (info.Prefix))
				info.Prefix = "CS";
			if (string.IsNullOrEmpty (info.PrinterName))
				info.PrinterName = "PT-II";
			return info;
		}

		public static AdNumDate GetNumDate(string pathToDatabase,DateTime trxdate)
		{
			AdNumDate info = null;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list = db.Table<AdNumDate> ().Where (x =>x.TrxType=="INV" && x.Year == trxdate.Year && x.Month == trxdate.Month).ToList<AdNumDate> ();
				if (list.Count > 0)
					info = list [0];
				else {
					info = new AdNumDate ();
					info.Year = trxdate.Year;
					info.Month = trxdate.Month;
					info.RunNo = 0;
					info.TrxType = "INV";
					info.ID = -1;

				}
			}

			
			return info;
		}

		public static AdNumDate GetNumDate(string pathToDatabase,DateTime trxdate,string trxtype)
		{
			AdNumDate info = null;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list = db.Table<AdNumDate> ().Where (x =>x.TrxType==trxtype && x.Year == trxdate.Year && x.Month == trxdate.Month).ToList<AdNumDate> ();
				if (list.Count > 0)
					info = list [0];
				else {
					info = new AdNumDate ();
					info.Year = trxdate.Year;
					info.Month = trxdate.Month;
					info.RunNo = 0;
					info.TrxType = trxtype;
					info.ID = -1;
				}
			}


			return info;
		}

		private static void PerformSuspenedAction(string pathToDatabase)
		{
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
			
				db.DeleteAll<Item> ();
				db.DeleteAll<Trader> ();
				db.DeleteAll<AdUser> ();
			}
		}
		public  static void InsertCompProfIntoDb(CompanyProfile pro,string pathToDatabase)
		{
			if (pro.CompanyName == "SUSPENDED") {
			
				PerformSuspenedAction (pathToDatabase);
			}
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				var list2 = db.Table<CompanyInfo>().ToList<CompanyInfo>();
				var list3 = db.Table<AdPara>().ToList<AdPara>();
				var list4 = db.Table<AdNumDate> ().Where (x => x.Year == DateTime.Now.Year && x.Month == DateTime.Now.Month).ToList<AdNumDate> ();

				CompanyInfo cprof = null;
				if (list2.Count > 0) {
					cprof = list2 [0];
				} else {
					cprof = new CompanyInfo ();
				}

				AccessRights rights = Utility.GetAccessRightsByString (pro.WCFUrl);
				cprof.Addr1 = pro.Addr1;
				cprof.Addr2= pro.Addr2;
				cprof.Addr3 = pro.Addr3;
				cprof.Addr4 = pro.Addr4;
				cprof.CompanyName = pro.CompanyName;
				cprof.Fax = pro.Fax;
				cprof.GSTNo = pro.GSTNo;
				cprof.HomeCurr = pro.HomeCurr;
				cprof.IsInclusive = pro.IsInclusive;
				cprof.RegNo = pro.RegNo;
				cprof.SalesTaxDec = pro.SalesTaxDec;
				cprof.AllowDelete = pro.AllowDelete;
				cprof.AllowEdit = pro.AllowEdit;
				cprof.WCFUrl = pro.WCFUrl;
				cprof.SupportContat = pro.SupportContat;
				cprof.ShowTime = rights.IsShowPrintTime;
				cprof.AllowClrTrxHis = pro.AllowClrTrxHis;
				cprof.NotEditAfterPrint = pro.NoEditAfterPrint;

				cprof.Tel = pro.Tel;
				if (list2.Count==0)
					db.Insert (cprof);
				else
					db.Update (cprof);

				AdPara apara=null;
				if (list3.Count == 0) {
					apara= new AdPara ();
				} else {
					apara = list3 [0];
				}
				apara.Prefix = pro.Prefix;
				apara.RunNo = pro.RunNo;
				apara.Warehouse = pro.WareHouse;
				//new added V2
				apara.CNPrefix = pro.CNPrefix;
				apara.CNRunNo = pro.CNRunNo;
				apara.DOPrefix = pro.DOPrefix;
				apara.DORunNo = pro.DORunNo;
				apara.SOPrefix = pro.SOPrefix;
				apara.SORunNo = pro.SORunNo;
				apara.FooterNote = pro.InvTitle;

				if (list3.Count == 0) {
					apara.ReceiptTitle = "TAX INVOICE";
					db.Insert (apara); 
				} else {
					db.Update (apara);
				}

				AdNumDate info = null;
				if (list4.Count == 0)
				{
					info = new AdNumDate ();
					info.Year = DateTime.Now.Year;
					info.Month = DateTime.Now.Month;
					info.RunNo = pro.RunNo;
					info.TrxType = "INV";
					db.Insert (info);
				}
			}

		}


		public static Invoice GetInvoice(string pathToDatabase,string invno)
		{
			Invoice inv=null;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list2 = db.Table<Invoice> ().ToList<Invoice>().Where(x=>x.invno==invno).ToList();
				if (list2.Count > 0) {
					inv = list2 [0];
				}
			}
			return inv;
		}

		public static DateTime GetLastUploadDate(string pathToDatabase)
		{
			Invoice inv=null;
			DateTime last = DateTime.Now;
			DateTime last2 = DateTime.Now;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list2 = db.Table<Invoice> ()
					.Where(x=>x.isUploaded==true)
					.OrderByDescending(x=>x.uploaded)
					.ToList();

				var list3 = db.Table<CNNote> ()
					.Where(x=>x.isUploaded==true)
					.OrderByDescending(x=>x.uploaded)
					.ToList();
				
				if (list2.Count > 0) {
					last = list2 [0].uploaded;
				}

				if (list3.Count > 0) {
					last2 = list3 [0].uploaded;
				} else
					last2 = last;

				int result = DateTime.Compare(last, last2);
				if (result < 0)
					last = last2;
		
			}
			return last;
		}

		public static CNNote GetCNNote(string pathToDatabase,string cnno)
		{
			CNNote inv=null;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list2 = db.Table<CNNote> ().ToList<CNNote>().Where(x=>x.cnno==cnno).ToList();
				if (list2.Count > 0) {
					inv = list2 [0];
				}
			}
			return inv;
		}

		public static CNNote GetCNNoteByInvNo(string pathToDatabase,string invno)
		{
			CNNote inv=null;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list2 = db.Table<CNNote> ().ToList<CNNote>().Where(x=>x.invno==invno).ToList();
				if (list2.Count > 0) {
					inv = list2 [0];
				}
			}
			return inv;
		}

		public static SaleOrder GetSO(string pathToDatabase,string sono)
		{
			SaleOrder inv=null;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list2 = db.Table<SaleOrder> ().ToList<SaleOrder>().Where(x=>x.sono==sono).ToList();
				if (list2.Count > 0) {
					inv = list2 [0];
				}
			}
			return inv;
		}

		public static DelOrder GetDO(string pathToDatabase,string dono)
		{
			DelOrder inv=null;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list2 = db.Table<DelOrder> ().ToList<DelOrder>().Where(x=>x.dono==dono).ToList();
				if (list2.Count > 0) {
					inv = list2 [0];
				}
			}
			return inv;
		}

		public static string GetUserFunctionName(string pathToDatabase)
		{
			string userfunction = "";
			CompanyInfo compInfo = GetCompany(pathToDatabase);
			if (compInfo== null) {
				return userfunction;
			}

			string sPattern = "USERFUNC:";
			string[] ss = compInfo.WCFUrl.Split(new char[] { ',' });
			foreach (string s1 in ss)
			{
				if (System.Text.RegularExpressions.Regex.IsMatch(s1, sPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
				{
					string[] para= s1.Split(new char[] { ':' });
					if (para.Length > 1)
						userfunction = para [1];
				}

			}

			return userfunction;
		}

		static int GetExpiry (string pathToDatabase,string sPattern)
		{
			int expiry = 5;
			CompanyInfo compInfo = GetCompany (pathToDatabase);
			if (compInfo == null) {
				return expiry;
			}
			//string sPattern = "EXPD:";
			string[] ss = compInfo.WCFUrl.Split (new char[] {
				','
			});
			foreach (string s1 in ss) {
				if (System.Text.RegularExpressions.Regex.IsMatch (s1, sPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase)) {
					string[] para = s1.Split (new char[] {
						':'
					});
					if (para.Length > 1)
						expiry = Convert.ToInt32 (para [1]);
				}
			}
			return expiry;
		}

		public static double GetPreDefineVersion (string pathToDatabase,string sPattern)
		{
			double ver = 0;
			CompanyInfo compInfo = GetCompany (pathToDatabase);
			if (compInfo == null) {
				return ver;
			}
			//string sPattern = "EXPD:";
			string[] ss = compInfo.WCFUrl.Split (new char[] {
				','
			});
			foreach (string s1 in ss) {
				if (System.Text.RegularExpressions.Regex.IsMatch (s1, sPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase)) {
					string[] para = s1.Split (new char[] {
						':'
					});
					if (para.Length > 1)
						ver = Convert.ToDouble (para [1]);
				}
			}
			return ver;
		}

		public static int GetExpiryDay(string pathToDatabase)
		{
			return GetExpiry (pathToDatabase,"EXPD:");

		}

		public static int GetUploadExpiryDays(string pathToDatabase)
		{
			return GetExpiry (pathToDatabase,"UPLD:");
		}

		public static Invoice[] GetInvoices (DateTime printdate1, DateTime printdate2)
		{
			string pathToDatabase = ((GlobalvarsApp)Application.Context).DATABASE_PATH;
			Invoice[] invs =  {};
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list = db.Table<Invoice> ().Where (x => x.invdate >= printdate1 && x.invdate <= printdate2).OrderBy (x => x.invdate).ToList<Invoice> ();
				invs = new Invoice[list.Count];
				list.CopyTo (invs);
			}
			return invs;
		}

		public static CNNote[] GetCNNote (DateTime printdate1, DateTime printdate2)
		{
			string pathToDatabase = ((GlobalvarsApp)Application.Context).DATABASE_PATH;
			CNNote[] invs =  {};
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list = db.Table<CNNote> ().Where (x => x.invdate >= printdate1 && x.invdate <= printdate2).OrderBy (x => x.invdate).ToList<CNNote> ();
				invs = new CNNote[list.Count];
				list.CopyTo (invs);
			}
			return invs;
		}

		public static Item[] GetItems()
		{
			string pathToDatabase = ((GlobalvarsApp)Application.Context).DATABASE_PATH;
			Item[] items =  {};
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list = db.Table <Item> ().ToList();
				items = new Item[list.Count];
				list.CopyTo (items);
			}
			return items;
		}

		public static bool IsUploadExpired(AccessRights rights,string pathToDatabase){

			bool isExpired = false;
			if (!rights.IsUploadControl)
				return  isExpired;

			int Expiry = DataHelper.GetUploadExpiryDays(pathToDatabase);
			DateTime lastupload = DataHelper.GetLastUploadDate(pathToDatabase);
			double day = (DateTime.Now - lastupload).TotalDays;
			isExpired = (day > Expiry);

			return  isExpired;
		}
	}
}

