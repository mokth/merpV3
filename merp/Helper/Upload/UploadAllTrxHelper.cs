using System;
using System.Linq;
using WcfServiceItem;
using System.Collections.Generic;
using Android.App;
using SQLite;
using Android.Widget;

namespace wincom.mobile.erp
{

	public class UploadAllTrxHelper:Activity,IUploadHelper
	{
		Service1Client _client;
		WCFHelper _wfc = new WCFHelper();
		string pathToDatabase;
		string comp;
		string brn ;
		string userid ;		
		string ver;
		volatile List<OutLetBillTemp> allbills = new List<OutLetBillTemp> ();
		volatile List<OutLetBill> bills = new List<OutLetBill> ();
		volatile string _errmsg;
		volatile int invcount =0;
		public OnUploadDoneDlg Uploadhandle;
		public Activity CallingActivity=null;

		private void InitVar()
		{
			pathToDatabase = ((GlobalvarsApp)CallingActivity.Application).DATABASE_PATH;
			comp =((GlobalvarsApp)CallingActivity.Application).COMPANY_CODE;
			brn =((GlobalvarsApp)CallingActivity.Application).BRANCH_CODE;
			userid =((GlobalvarsApp)CallingActivity.Application).USERID_CODE;		
			ver =((GlobalvarsApp)CallingActivity.Application).VERSION;
		}

		public void startUpload()
		{
			invcount =0;
			_errmsg = "";
			InitVar ();
			_client = _wfc.GetServiceClient ();	
			if (_client != null) {
				GetAllBills ();
				_client.UploadOutletBillsCompleted += ClientOnUploadOutletBillsCompleted;
				UploadBillsToServer ();
			}
		}

		public void SetUploadHandel(OnUploadDoneDlg uploadhandle)
		{
			Uploadhandle = uploadhandle;
		}

		public void SetCallingActivity(Activity callingActivity)
		{
			CallingActivity = callingActivity;
		}

		private void UploadBillsToServer()
		{
			PhoneTool ptool = new PhoneTool ();
			string phone = ptool.PhoneNumber ();
			string serial =ptool.DeviceIdIMEI();

		
			bills = GetBills();
			invcount += bills.Count;
			if (bills.Count > 0) {
				_client.UploadOutletBillsAsync (bills.ToArray (), comp, brn, serial, ver );
			} else {
				RunOnUiThread (() => Uploadhandle.Invoke(CallingActivity,invcount,_errmsg));
			}
		}

		public void UpdateUploadStat()
		{
			string pathToDatabase = ((GlobalvarsApp)CallingActivity.Application).DATABASE_PATH;
			DateTime now = DateTime.Now;
			try {
				using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
					var listInv = db.Table<Invoice> ().Where (x => x.isUploaded == false).ToList<Invoice> ();
					var listCN = db.Table<CNNote> ().Where (x => x.isUploaded == false).ToList<CNNote> ();
					var listSO = db.Table<SaleOrder> ().Where (x => x.isUploaded == false).ToList<SaleOrder> ();
					var listDO = db.Table<DelOrder> ().Where (x => x.isUploaded == false).ToList<DelOrder> ();

					List<Invoice> invlist = new List<Invoice> ();
					List<CNNote> cnlist = new List<CNNote> ();
					List<SaleOrder> solist = new List<SaleOrder> ();
					List<DelOrder> dolist = new List<DelOrder> ();

					string module = "";
					foreach (OutLetBill bill in bills) {
						if (bill.TrxType == "CASH" || bill.TrxType == "INVOICE")
							module = "INV";
						else if (bill.TrxType == "CN")
							module = "CN";
						else if (bill.TrxType == "DO")
							module = "DO";
						else if (bill.TrxType == "SO")
							module = "SO";
						else
							module = "XXX";

						var biList = allbills.Where (x => x.InvNo == bill.InvNo && x.Module == module).ToList ();
						if (biList.Count == 0)
							continue;
						foreach(var bitem in biList)
						{
							bitem.IsUploaded = true;
						}

						if (module =="INV") {
							var found = listInv.Where (x => x.invno == bill.InvNo && bill.TrxType == x.trxtype).ToList<Invoice> ();
							if (found.Count > 0) {  
								found [0].isUploaded = true;
								found [0].uploaded = now;
								invlist.Add (found [0]);
							}
						}
						else if (module == "CN") {
							var found = listCN.Where (x => x.cnno == bill.InvNo).ToList<CNNote> ();
							if (found.Count > 0) {  
								found [0].isUploaded = true;
								found [0].uploaded = now;
								cnlist.Add (found [0]);
							}
						}else if (module == "DO") {
							var found = listDO.Where (x => x.dono == bill.InvNo).ToList<DelOrder> ();
							if (found.Count > 0) {  
								found [0].isUploaded = true;
								found [0].uploaded = now;
								dolist.Add (found [0]);
							}
						}else if (module == "SO") {
							var found = listSO.Where (x => x.sono == bill.InvNo).ToList<SaleOrder> ();
							if (found.Count > 0) {  
								found [0].isUploaded = true;
								found [0].uploaded = now;
								solist.Add (found [0]);
							}
						}


					}	

					if (invlist.Count > 0)
						db.UpdateAll (invlist);

					if (cnlist.Count > 0)
						db.UpdateAll (cnlist);

					if (dolist.Count > 0)
						db.UpdateAll (dolist);

					if (solist.Count > 0)
						db.UpdateAll (solist);

					DataHelper.UpdateLastConnect (pathToDatabase);
//					if (invlist2.Count>0)
//						db.UpdateAll (invlist2);
					UploadBillsToServer ();
				}
			} catch (Exception ex) {
				_errmsg = "Update status Error." + ex.Message;
			}
		}

		private void GetAllBills()
		{
			allbills = new List<OutLetBillTemp> ();
			GetAllInvBills ();
			GetAllCNs ();
			GetAllSOs();
			GetAllDOs ();

		}
		private void GetAllInvBills()
		{
			
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list1 = db.Table<Invoice> ().Where(x=>x.isUploaded==false)
					.OrderBy(x=>x.invno)
					.ToList<Invoice> ();

				var list2 = db.Table<InvoiceDtls> ().ToList<InvoiceDtls> ();

				foreach (Invoice inv in list1) {
					var list3 = list2.Where (x => x.invno == inv.invno).ToList<InvoiceDtls> ();
					foreach (InvoiceDtls invdtl in list3) {
						OutLetBillTemp bill = new OutLetBillTemp ();
						bill.UserID = userid;
						bill.BranchCode = brn;
						bill.CompanyCode = comp;
						bill.Created = inv.created;
						bill.CustCode = inv.custcode;
						bill.ICode = invdtl.icode;
						bill.InvDate = inv.invdate;
						bill.InvNo = inv.invno;
						bill.IsInclusive = invdtl.isincludesive;
						bill.Amount = invdtl.amount;
						bill.NetAmount = invdtl.netamount;
						bill.TaxAmt = invdtl.tax;
						bill.TaxGrp = invdtl.taxgrp;
						bill.UPrice = invdtl.price;
						bill.Qty = invdtl.qty;
						bill.TrxType = inv.trxtype;
						bill.Module = "INV";
						allbills.Add (bill);
					}
				}
			}


		}

		private  void GetAllCNs()
		{

			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list1 = db.Table<CNNote> ().Where(x=>x.isUploaded==false)
					.OrderBy(x=>x.cnno)
					.ToList<CNNote> ();

				var list2 = db.Table<CNNoteDtls> ().ToList<CNNoteDtls> ();

				foreach (CNNote inv in list1) {
					var list3 = list2.Where (x => x.cnno == inv.cnno).ToList<CNNoteDtls> ();
					foreach (CNNoteDtls invdtl in list3) {
						OutLetBillTemp bill = new OutLetBillTemp ();
						bill.UserID = userid;
						bill.BranchCode = brn;
						bill.CompanyCode = comp;
						bill.Created = inv.created;
						bill.CustCode = inv.custcode;
						bill.ICode = invdtl.icode;
						bill.InvDate = inv.invdate;
						bill.InvNo = inv.cnno;
						bill.CNInvNo = inv.invno;
						bill.IsInclusive = invdtl.isincludesive;
						bill.Amount = invdtl.amount;
						bill.NetAmount = invdtl.netamount;
						bill.TaxAmt = invdtl.tax;
						bill.TaxGrp = invdtl.taxgrp;
						bill.UPrice = invdtl.price;
						bill.Qty = invdtl.qty;
						bill.TrxType = "CN";
						bill.Module = "CN";
						allbills.Add (bill);
					}
				}
			}

		}

		private  void GetAllSOs()
		{
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list1 = db.Table<SaleOrder> ().Where(x=>x.isUploaded==false)
					.OrderBy(x=>x.sono)
					.ToList<SaleOrder> ();

				var list2 = db.Table<SaleOrderDtls> ().ToList<SaleOrderDtls> ();

				foreach (SaleOrder so in list1) {
					var list3 = list2.Where (x => x.sono == so.sono).ToList<SaleOrderDtls> ();
					foreach (SaleOrderDtls invdtl in list3) {
						OutLetBillTemp bill = new OutLetBillTemp ();
						bill.UserID = userid;
						bill.BranchCode = brn;
						bill.CompanyCode = comp;
						bill.Created = so.created;
						bill.CustCode = so.custcode;
						bill.ICode = invdtl.icode;
						bill.InvDate = so.sodate;
						bill.InvNo = so.sono;
						bill.IsInclusive = invdtl.isincludesive;
						bill.Amount = invdtl.amount;
						bill.NetAmount = invdtl.netamount;
						bill.TaxAmt = invdtl.tax;
						bill.TaxGrp = invdtl.taxgrp;
						bill.UPrice = invdtl.price;
						bill.Qty = invdtl.qty;
						bill.TrxType = "SO";
						bill.CNInvNo = so.custpono;
						bill.Remark = so.remark;
						bill.OtheDesc = so.billTo;
						bill.Module = "SO";
						allbills.Add (bill);
					}
				}
			}

		}

		private void GetAllDOs()
		{
			
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list1 = db.Table<DelOrder> ().Where(x=>x.isUploaded==false)
					.OrderBy(x=>x.dono)
					.ToList<DelOrder> ();

				var list2 = db.Table<DelOrderDtls> ().ToList<DelOrderDtls> ();

				foreach (DelOrder delOrder in list1) {
					var list3 = list2.Where (x => x.dono == delOrder.dono).ToList<DelOrderDtls> ();
					foreach (DelOrderDtls invdtl in list3) {
						OutLetBillTemp bill = new OutLetBillTemp ();
						bill.UserID = userid;
						bill.BranchCode = brn;
						bill.CompanyCode = comp;
						bill.Created = delOrder.created;
						bill.CustCode = delOrder.custcode;
						bill.ICode = invdtl.icode;
						bill.InvDate = delOrder.dodate;
						bill.InvNo = delOrder.dono;
						bill.IsInclusive = invdtl.isincludesive;
						bill.Amount = invdtl.amount;
						bill.NetAmount = invdtl.netamount;
						bill.TaxAmt = invdtl.tax;
						bill.TaxGrp = invdtl.taxgrp;
						bill.UPrice = invdtl.price;
						bill.Qty = invdtl.qty;
						bill.TrxType = "DO";
						bill.Remark = delOrder.remark;
						bill.OtheDesc = delOrder.term;
						//bill.CNInvNo = so.custpono;
						bill.Module="DO";
						allbills.Add (bill);
					}
				}
			}

		}

		public List<OutLetBill> GetBills()
		{
			
			bills = new List<OutLetBill> ();
			var list11 = from p in allbills
			             where p.IsUploaded == false
			             group p by p.InvNo into g
				select new {key = g.Key,result = g.ToList (),count=g.Count()};

			int counter = 0;
			List<OutLetBillTemp> list1 = new List<OutLetBillTemp> ();
			foreach (var grp in list11) {
				counter = counter + grp.count;
				list1.AddRange(grp.result);
				if (counter > 30) {
					break;
				}
			}

//			var list1 = allbills.Where (x => x.IsUploaded == false)
//				.GroupBy(x=>x.InvNo)
//					.Take (10)
//					.ToList ();
		
			foreach (OutLetBillTemp trx in list1) {
					
				OutLetBill bill = new OutLetBill ();
				bill.UserID = userid;
				bill.BranchCode = brn;
				bill.CompanyCode = comp;
				bill.Created = trx.Created;
				bill.CustCode = trx.CustCode;
				bill.ICode = trx.ICode;
				bill.InvDate = trx.InvDate;
				bill.InvNo = trx.InvNo;
				bill.IsInclusive = trx.IsUploaded;
				bill.Amount = trx.Amount;
				bill.NetAmount = trx.NetAmount;
				bill.TaxAmt = trx.TaxAmt;
				bill.TaxGrp = trx.TaxGrp;
				bill.UPrice = trx.UPrice;
				bill.Qty = trx.Qty;
				bill.TrxType = trx.TrxType;
				bills.Add (bill);
			}

			return bills;
		}



		public void ClientOnUploadOutletBillsCompleted(object sender, UploadOutletBillsCompletedEventArgs e)
		{
			bool success = false;
			if ( e.Error != null)
			{
				_errmsg =  e.Error.Message;
			
			}
			else if ( e.Cancelled)
			{
				_errmsg = "Request was cancelled.";
			}
			else
			{
				_errmsg = e.Result.ToString ();
				if (_errmsg== "OK") {
					success = true;
					UpdateUploadStat();
				}
			}

			if (!success)
				RunOnUiThread (() => Uploadhandle.Invoke(CallingActivity,0,_errmsg));

			//

		}

//		void DownloadCOmpleted (string msg)
//		{
//			Toast.MakeText (this, msg, ToastLength.Long).Show ();	
//
//		}
	}
}

