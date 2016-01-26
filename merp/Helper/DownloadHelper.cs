using System;
using System.Linq;
using WcfServiceItem;
using System.Collections.Generic;
using Android.App;
using SQLite;
using Android.Widget;
using System.Collections;

namespace wincom.mobile.erp
{
	public class DownloadHelper:Activity,IEventListener
	{
		Service1Client _client;
		WCFHelper _wfc = new WCFHelper();

		public Activity CallingActivity=null;
		public OnUploadDoneDlg Downloadhandle;
		public OnUploadDoneDlg DownloadAllhandle;
		public volatile static bool _downloadAll = false;
		public volatile static bool _downloadPro = false;
		public volatile static bool _downloadItem = false;
		public volatile static bool _downloadCust = false;

		public DownloadHelper ()
		{
			EventManagerFacade.Instance.GetEventManager().AddListener(this);
		}

		public void StartDownloadAll()
		{
			_downloadAll = true;
			startDownloadCompInfoex() ;
		}

		public void NotDownloadAll()
		{
			_downloadAll = false;

		}

		public void startDownloadItem()
		{
			string comp =((GlobalvarsApp)CallingActivity.Application).COMPANY_CODE;
			string brn =((GlobalvarsApp)CallingActivity.Application).BRANCH_CODE;
			string userid = ((GlobalvarsApp)CallingActivity.Application).USERID_CODE;

			_client = _wfc.GetServiceClient ();	
			if (_client != null) {
				_client.GetItemCodesExCompleted += ClientOnGetItemCompleted;
				_client.GetItemCodesExAsync (comp, brn,userid );
			}
		}

		public void startDownloadItemGR()
		{
			string comp =((GlobalvarsApp)CallingActivity.Application).COMPANY_CODE;
			string brn =((GlobalvarsApp)CallingActivity.Application).BRANCH_CODE;
			string userid = ((GlobalvarsApp)CallingActivity.Application).USERID_CODE;

			_client = _wfc.GetServiceClient ();	
			if (_client != null) {
				_client.GetItemReceiveCompleted += _client_GetItemReceiveCompleted;
				_client.GetItemReceiveAsync (comp, brn,userid );
			}
		}


		public void startDownloadCustomer()
		{
			string comp =((GlobalvarsApp)CallingActivity.Application).COMPANY_CODE;
			string brn =((GlobalvarsApp)CallingActivity.Application).BRANCH_CODE;
			string userid = ((GlobalvarsApp)CallingActivity.Application).USERID_CODE;
		
			_client = _wfc.GetServiceClient ();	
			if (_client != null) {
				_client.GetCustomersExCompleted += ClientOnGetCustomerCompleted;
				_client.GetCustomersExAsync (comp, brn, userid);
			}
		}

		void _client_GetItemReceiveCompleted (object sender, GetItemReceiveCompletedEventArgs e)
		{
			List<ItemGR> list = new List<ItemGR> ();
			string msg = null;

			if ( e.Error != null)
			{
				msg =  e.Error.Message;
			}
			else if ( e.Cancelled)
			{
				msg = CallingActivity.Resources.GetString(Resource.String.msg_reqcancel);
			}
			else
			{
				list =  e.Result.ToList<ItemGR>();
				RunOnUiThread (() => InsertItemGRIntoDb(list));
			}

			if (msg != null) {
				RunOnUiThread (() => Downloadhandle.Invoke (CallingActivity, 0, msg));
			}

		}

		public  void startDownloadCompInfoex()
		{
			string comp =((GlobalvarsApp)CallingActivity.Application).COMPANY_CODE;
			string brn =((GlobalvarsApp)CallingActivity.Application).BRANCH_CODE;
			string userid = ((GlobalvarsApp)CallingActivity.Application).USERID_CODE;
			_client = _wfc.GetServiceClient ();	
		
			if (_client != null) {
			_client.GetCompProfileCompleted += ClientOnCompProfCompleted;
			_client.GetCompProfileAsync (comp,brn,userid);
			}
		}

		public  void startDownloadCompInfo()
		{
			_downloadAll =false;
			string comp =((GlobalvarsApp)CallingActivity.Application).COMPANY_CODE;
			string brn =((GlobalvarsApp)CallingActivity.Application).BRANCH_CODE;
			string userid = ((GlobalvarsApp)CallingActivity.Application).USERID_CODE;
		
			_client = _wfc.GetServiceClient ();	
			if (_client != null) {
				_client.GetCompProfileCompleted += ClientOnCompProfCompleted;
				_client.GetCompProfileAsync (comp, brn, userid);
			}
		}


		public void startDownloadRunNoInfo()
		{
			string comp =((GlobalvarsApp)CallingActivity.Application).COMPANY_CODE;
			string brn =((GlobalvarsApp)CallingActivity.Application).BRANCH_CODE;
			string userid = ((GlobalvarsApp)CallingActivity.Application).USERID_CODE;
			_client = _wfc.GetServiceClient ();	

			if (_client != null) {
				_client.GetRunnoCompleted += _client_GetRunnoCompleted;
				_client.GetRunnoAsync (comp, brn, userid);
			}
		}


		public void startLogin(string userid, string passw, string code)
		{
			_client = _wfc.GetServiceClient ();	
			if (_client != null) {
				PhoneTool ptool = new PhoneTool ();
				string serial = ptool.DeviceIdIMEI ();
				_client.LoginExCompleted += ClientOnLoginCompleted;
				_client.LoginExAsync (userid, passw, code, serial);
			}
		}

		private void ClientOnGetItemCompleted(object sender, GetItemCodesExCompletedEventArgs e)
		{
			List<ItemCode> list = new List<ItemCode> ();
			string msg = null;

			if ( e.Error != null)
			{
				msg =  e.Error.Message;
			}
			else if ( e.Cancelled)
			{
				msg = CallingActivity.Resources.GetString(Resource.String.msg_reqcancel);
			}
			else
			{
				list =  e.Result.ToList<ItemCode>();
				RunOnUiThread (() => InsertItemIntoDb (list));
			}

			if (msg != null) {
				RunOnUiThread (() => Downloadhandle.Invoke (CallingActivity, 0, msg));
				if (_downloadAll) {
					_downloadAll = false;	
					FireEvent (EventID.LOGIN_DOWNCOMPLETE);
				}
			}

		}

		private void ClientOnGetCustomerCompleted(object sender, GetCustomersExCompletedEventArgs e)
		{
			List<Customer> list = new List<Customer> ();
			string msg = null;
			bool success = true;
			if ( e.Error != null)
			{
				msg =  e.Error.Message;
				success = false;
			}
			else if ( e.Cancelled)
			{
				msg = CallingActivity.Resources.GetString(Resource.String.msg_reqcancel);
				success = false;
			}
			else
			{
				list =  e.Result.ToList<Customer>();
				RunOnUiThread (() => InsertCustomerIntoDb (list));
			}
			if (!success) {
				RunOnUiThread (() => Downloadhandle.Invoke(CallingActivity,0,msg));
				if (_downloadAll) {
					_downloadAll = false;	
					FireEvent (EventID.LOGIN_DOWNCOMPLETE);
				}
			}
		}

		private void ClientOnCompProfCompleted(object sender,GetCompProfileCompletedEventArgs e)
		{
			string msg = null;
			bool success = true;
			if ( e.Error != null)
			{
				msg =  e.Error.Message;
				success = false;
			}
			else if ( e.Cancelled)
			{
				msg = CallingActivity.Resources.GetString(Resource.String.msg_reqcancel);
				success = false;
			}
			else
			{
				CompanyProfile pro = (CompanyProfile)e.Result;
				RunOnUiThread (() => InsertCompProfIntoDb( pro));
			}
			if (!success) {
				RunOnUiThread (() => Downloadhandle.Invoke(CallingActivity,0,msg));
				if (_downloadAll) {
					_downloadAll = false;	
					FireEvent (EventID.LOGIN_DOWNCOMPLETE);
				}
			}
		}

		private void ClientOnLoginCompleted(object sender, LoginExCompletedEventArgs e)
		{
			string msg = null;
			bool success = true;
			if ( e.Error != null)
			{
				msg =  e.Error.Message;
				success = false;
			}
			else if ( e.Cancelled)
			{
				msg = CallingActivity.Resources.GetString(Resource.String.msg_reqcancel);
				success = false;
			}
			else
			{

				RunOnUiThread (() => InsertItemIntoDb (e.Result.ToString()));
			}
			if (!success) {
				RunOnUiThread (() => Downloadhandle.Invoke(CallingActivity,0,msg));
			}
		}

		private void ClientOnCompProfCompletedEx(object sender,GetCompProfileCompletedEventArgs e)
		{
			string msg = null;
			bool success = true;
			if ( e.Error != null)
			{
				msg =  e.Error.Message;
				success = false;
			}
			else if ( e.Cancelled)
			{
				msg = CallingActivity.Resources.GetString(Resource.String.msg_reqcancel);
				success = false;
			}
			else
			{
				CompanyProfile pro = (CompanyProfile)e.Result;
				RunOnUiThread (() => InsertCompProfIntoDbEx( pro));
			}
			if (!success) {
				RunOnUiThread (() => Downloadhandle.Invoke(CallingActivity,0,msg));
			}
		}

		void _client_GetRunnoCompleted (object sender, GetRunnoCompletedEventArgs e)
		{
			List<RunnoInfo> list = new List<RunnoInfo> ();
			string msg = null;
			bool success = true;
			if ( e.Error != null)
			{
				msg =  e.Error.Message;
				success = false;
			}
			else if ( e.Cancelled)
			{
				msg = CallingActivity.Resources.GetString(Resource.String.msg_reqcancel);
				success = false;
			}
			else
			{
				list =  e.Result.ToList<RunnoInfo>();
				RunOnUiThread (() => InsertRunoIntoDb(list));
			}
			if (!success) {
				RunOnUiThread (() => Downloadhandle.Invoke(CallingActivity,0,msg));
				if (_downloadAll) {
					_downloadAll = false;	
					FireEvent (EventID.LOGIN_DOWNCOMPLETE);
				}

			}
		}

		private void InsertRunoIntoDb(List<RunnoInfo> list)
		{
			string pathToDatabase = ((GlobalvarsApp)CallingActivity.Application).DATABASE_PATH;

			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list2 = db.Table<AdNumDate> ().ToList<AdNumDate> ();
				foreach (var runinfo in list) {

					var found = list2.Where (x => x.Month == runinfo.Month && x.Year == runinfo.Year && x.TrxType == runinfo.Trxtype).ToList ();
					if (found.Count > 0) {
						found [0].RunNo = runinfo.RunNo;
						db.Update (found [0]);
					} else {
						AdNumDate num = new AdNumDate ();
						num.ID = -1;
						num.Month = runinfo.Month;
						num.Year = runinfo.Year;
						num.RunNo = runinfo.RunNo;
						num.TrxType = runinfo.Trxtype;
						db.Insert (num);
					}
				}
			}
			string dmsg = CallingActivity.Resources.GetString (Resource.String.msg_successdownrunno);
			if (_downloadAll) {
				_downloadPro = true;
				DownloadAllhandle.Invoke (CallingActivity, 0,dmsg);// "Successfully downloaded runing no.");
				FireEvent (EventID.DOWNLOADED_RUNNO);

			} else
				if (CallingActivity!=null)
					Downloadhandle.Invoke (CallingActivity, 0, dmsg);

		}
		void AlertShow(string text)
		{
			AlertDialog.Builder alert = new AlertDialog.Builder (this);

			alert.SetMessage (text);
			RunOnUiThread (() => {
				alert.Show();
			} );

		}
		private void InsertCompProfIntoDb(CompanyProfile pro)
		{
			string pathToDatabase = ((GlobalvarsApp)CallingActivity.Application).DATABASE_PATH;
			if (pro.CompanyName == "SUSPENDED")
				return;
			
			try{
			  AccessRights rights= DataHelper.InsertCompProfIntoDb (pro, pathToDatabase);
				((GlobalvarsApp)CallingActivity.Application).EnableGPSTracking(rights);
			}
			catch(Exception ex) {
				AlertShow (ex.Message + ex.StackTrace);
			}
			//DownloadAllhandle.Invoke(CallingActivity,0,"Successfully downloaded Profile.");
			startDownloadRunNoInfo ();
		}

		private void InsertItemIntoDb(List<ItemCode> list)
		{
			string pathToDatabase = ((GlobalvarsApp)CallingActivity.Application).DATABASE_PATH;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				//var list2 = db.Table<Item>().ToList<Item>();
				db.DeleteAll<Item> ();
				foreach (ItemCode item in list) {
					Item itm = new Item ();
					itm.ICode = item.ICode;
					itm.IDesc = item.IDesc;
					itm.Price = item.Price;
					itm.tax = item.tax;
					itm.taxgrp = item.taxgrp;
					itm.isincludesive = item.isincludesive;
					itm.RetailPrice = item.RetailPrice;
					itm.VIPPrice = item.VIPPrice;
					itm.WholeSalePrice = item.WholesalePrice;
					itm.Barcode = item.Barcode;
					itm.StdUom = item.UOM;
					db.Insert (itm);
				}
			}

			string dmsg = CallingActivity.Resources.GetString (Resource.String.msg_successdownitems);
			dmsg = dmsg.Replace ("nn", list.Count.ToString ());

			if (_downloadAll) {
				_downloadItem = true;
				DownloadAllhandle.Invoke(CallingActivity,list.Count,dmsg);
				FireEvent (EventID.DOWNLOADED_ITEM);
			}
			else Downloadhandle.Invoke(CallingActivity,list.Count,dmsg);
		}

		private void InsertItemGRIntoDb(List<ItemGR> list)
		{
			string pathToDatabase = ((GlobalvarsApp)CallingActivity.Application).DATABASE_PATH;
			string userid = ((GlobalvarsApp)CallingActivity.Application).USERID_CODE;
			DateTime today = new DateTime(DateTime.Today.Year,DateTime.Today.Month,DateTime.Today.Day,00,00,00);
			DateTime tempdate = today.AddDays (-4);
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				//var list2 = db.Table<Item>().ToList<Item>();
				var itemcode= db.Table<Item>().ToList();
				var stock= db.Table<ItemStock>().Where(x=>x.DateTrx>tempdate).OrderByDescending(x=>x.DateTrx).ToList();

				string desc = "";
				foreach (ItemGR item in list) {
					desc = "";
					var founditm = itemcode.Where (x => x.ICode == item.ICode).ToList ();
					if (founditm.Count > 0)
						desc = founditm [0].IDesc.Trim();
					
					var found = stock.Where (x => x.ICode == item.ICode && x.DateTrx == item.TrxDate).ToList ();
					if (found.Count > 0) {
						found [0].QtyGR = item.Qty;
						found [0].IDesc = desc;
						db.Update (found[0]);
					} else {
						ItemStock itm = new ItemStock ();
						itm.ICode = item.ICode;
						itm.IDesc = desc;
						itm.DateTrx = item.TrxDate;
						itm.QtyGR = item.Qty;
						itm.QtyAct = 0;
						itm.QtyBrf = 0;
						itm.QtyCrf = 0;
						itm.QtyRtr = 0;
						itm.QtySales = 0;
						itm.StdUom = item.UOM;
						itm.Wh = item.WH;
						db.Insert (itm);
					}
				}

				for ( int i=0;i<3;i++)
				{
					int nday = i * -1;
					tempdate =today.AddDays (nday);
					var stock2= db.Table<ItemStock>().Where(x=>x.DateTrx==tempdate).ToList();
					foreach (var itmc in itemcode) {
						var found =stock2.Where(x=>x.DateTrx==tempdate&&x.ICode==itmc.ICode).ToList();
						if (found.Count == 0) {
							ItemStock itm = new ItemStock ();
							itm.ICode = itmc.ICode;
							itm.IDesc = itmc.IDesc;
							itm.DateTrx = tempdate;
							itm.QtyGR = 0;
							itm.QtyAct = 0;
							itm.QtyBrf = 0;
							itm.QtyCrf = 0;
							itm.QtyRtr = 0;
							itm.QtySales = 0;
							itm.StdUom = itmc.StdUom;
							itm.Wh = userid;
							db.Insert (itm);
						}
					}

				}

			}

			string dmsg = CallingActivity.Resources.GetString (Resource.String.msg_successdownitems);
			dmsg = dmsg.Replace ("nn", list.Count.ToString ());
			Downloadhandle.Invoke(CallingActivity,list.Count,dmsg);

		}

		private void InsertCustomerIntoDb(List<Customer> list)
		{
			string pathToDatabase = ((GlobalvarsApp)CallingActivity.Application).DATABASE_PATH;
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				//var list2 = db.Table<Trader>().ToList<Trader>();
				db.DeleteAll<Trader> ();// (list2);
				foreach (Customer item in list) {
					Trader itm = new Trader ();
					itm.CustCode = item.CustomerCode;
					itm.CustName = item.CustomerName;
					itm.Addr1 = item.Addr1;
					itm.Addr2 = item.Addr2;
					itm.Addr3 = item.Addr3;
					itm.Addr4 = item.Addr4;
					itm.Tel = item.Tel;
					itm.Fax = item.Fax;
					itm.gst = item.Gst;
					itm.PayCode = item.PayCode;
					itm.CustType = item.CustType;
					itm.AgentCode = item.ExtraInfo;

					db.Insert (itm);
//					if (list2.Where (x => x.CustCode == itm.CustCode).ToList ().Count () == 0) {
//						db.Insert (itm);
//					} else {
//						db.Update (itm);
//					}
				}
			}

			string dmsg = CallingActivity.Resources.GetString (Resource.String.msg_successdowncusts);
			dmsg = dmsg.Replace ("nn", list.Count.ToString ());

			if (_downloadAll) {
				FireEvent (EventID.LOGIN_DOWNCOMPLETE);
				DownloadAllhandle.Invoke(CallingActivity,list.Count,dmsg);
			}else Downloadhandle.Invoke(CallingActivity,list.Count,dmsg);

		}

		private void InsertItemIntoDb(string result)
		{
			string pathToDatabase = ((GlobalvarsApp)CallingActivity.Application).DATABASE_PATH;
			string[] para = result.Split (new char[]{ '|' });
			if (para [0] != "OK") {
				Downloadhandle.Invoke(CallingActivity,0, CallingActivity.Resources.GetString (Resource.String.msg_faillogin));
				return;
			}
			EditText passw =  CallingActivity.FindViewById<EditText> (Resource.Id.login_password);
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				var list2 = db.Table<AdUser>().ToList<AdUser>();
				list2.RemoveAll (x => x.UserID == para[1]);
				AdUser user = new AdUser ();
				user.BranchCode = para [3];
				user.CompCode = para [2];
				user.Islogon = true;
				user.Password = passw.Text;
				user.UserID = para [1];
				user.LastConnect = DateTime.Now;
				db.Insert (user);
			}
			((GlobalvarsApp)CallingActivity.Application).USERID_CODE = para [1];
			((GlobalvarsApp)CallingActivity.Application).COMPANY_CODE = para [2];
			((GlobalvarsApp)CallingActivity.Application).BRANCH_CODE = para [3];
			//DownloadCOmpleted ("Successfully Logon.");
			//Downloadhandle.Invoke(CallingActivity,0,"Successfully Logon.");
			FireEvent (EventID.LOGIN_SUCCESS);
		}

		private void InsertCompProfIntoDbEx(CompanyProfile pro)
		{
			string pathToDatabase = ((GlobalvarsApp)CallingActivity.Application).DATABASE_PATH;
			DataHelper.InsertCompProfIntoDb (pro, pathToDatabase);

			string dmsg = CallingActivity.Resources.GetString (Resource.String.msg_successdownprofile);
			//FireEvent (EventID.LOGIN_DOWNCOMPRO);
			if (_downloadAll) {
				DownloadAllhandle.Invoke(CallingActivity,0, dmsg);
				FireEvent (EventID.DOWNLOADED_PROFILE);
			}
			else Downloadhandle.Invoke(CallingActivity,0,dmsg);
		}

		void FireEvent (int eventID)
		{
			Hashtable param = new Hashtable ();
			EventParam p = new EventParam (eventID, param);
			EventManagerFacade.Instance.GetEventManager ().PerformEvent (CallingActivity, p);
		}

		public event nsEventHandler eventHandler;

		public void FireEvent(object sender,EventParam eventArgs)
		{
			if (eventHandler != null)
				eventHandler (sender, eventArgs);
		}


		public void PerformEvent(object sender, EventParam e)
		{

			switch (e.EventID) {
			case EventID.DOWNLOADED_PROFILE:
				startDownloadRunNoInfo ();
				break;
			case EventID.DOWNLOADED_RUNNO:
				if (_downloadAll) {
					if (_downloadPro) {
						_downloadPro = false;
						startDownloadItem ();
					}
				}
				break;
			case EventID.DOWNLOADED_ITEM:
				if (_downloadAll) {
					if (_downloadItem) {
						_downloadItem = false;
						startDownloadCustomer ();
					}
				}
				break;

			}
		}
	}
}

