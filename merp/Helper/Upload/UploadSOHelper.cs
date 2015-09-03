﻿using System;
using System.Linq;
using WcfServiceItem;
using System.Collections.Generic;
using Android.App;
using SQLite;
using Android.Widget;

namespace wincom.mobile.erp
{
	
	public class UploadSOHelper:Activity,IUploadHelper
	{
		Service1Client _client;
		WCFHelper _wfc = new WCFHelper();
		volatile List<OutLetBill> bills = new List<OutLetBill> ();
		volatile string _errmsg;
		volatile int invcount =0;
		public OnUploadDoneDlg Uploadhandle;
		public Activity CallingActivity=null;

		public void startUpload()
		{
			invcount =0;
			_errmsg = "";
			_client = _wfc.GetServiceClient ();	
			_client.UploadOutletBillsCompleted+= ClientOnUploadOutletBillsCompleted;
			UploadBillsToServer ();
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
			string comp =((GlobalvarsApp)CallingActivity.Application).COMPANY_CODE;
			string brn =((GlobalvarsApp)CallingActivity.Application).BRANCH_CODE;
			bills = GetBills();
			invcount += bills.Count;
			if (bills.Count > 0) {
				_client.UploadOutletBillsAsync (bills.ToArray (), comp, brn, serial, phone);
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
					var list1 = db.Table<SaleOrder> ().ToList<SaleOrder> ().Where (x => x.isUploaded == false).ToList<SaleOrder> ();
					List<SaleOrder> invlist = new List<SaleOrder>();
					foreach (OutLetBill bill in bills) {
						var found = list1.Where(x=>x.sono==bill.InvNo && bill.TrxType==x.trxtype).ToList<SaleOrder>();
						if (found.Count>0)
						{  
							found[0].isUploaded = true;
							found[0].uploaded = now;
							invlist.Add(found[0]);
						}

					}	

					if (invlist.Count>0)
						db.UpdateAll (invlist);
					DataHelper.UpdateLastConnect(pathToDatabase);
					UploadBillsToServer ();
				}
			} catch (Exception ex) {
				_errmsg = "Update status Error." + ex.Message;
			}
		}

		public List<OutLetBill> GetBills()
		{
			string pathToDatabase = ((GlobalvarsApp)CallingActivity.Application).DATABASE_PATH;
			string comp =((GlobalvarsApp)CallingActivity.Application).COMPANY_CODE;
			string brn =((GlobalvarsApp)CallingActivity.Application).BRANCH_CODE;
			string userid =((GlobalvarsApp)CallingActivity.Application).USERID_CODE;

			bills = new List<OutLetBill> ();
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list1 = db.Table<SaleOrder> ().Where(x=>x.isUploaded==false)
					.OrderBy(x=>x.sono)
					.Take(10)
					.ToList<SaleOrder> ();
				
				var list2 = db.Table<SaleOrderDtls> ().ToList<SaleOrderDtls> ();

				foreach (SaleOrder so in list1) {
					var list3 = list2.Where (x => x.sono == so.sono).ToList<SaleOrderDtls> ();
					foreach (SaleOrderDtls invdtl in list3) {
						OutLetBill bill = new OutLetBill ();
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
						bills.Add (bill);
					}
				}
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

		}

	}
}

