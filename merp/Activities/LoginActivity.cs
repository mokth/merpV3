using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.ServiceModel;
using System.IO;
using WcfServiceItem;
using Android.Util;
using Android.Content.PM;
using SQLite;
using System.Net;
using Plugin.Geolocator.Abstractions;
using Plugin.Geolocator;


namespace wincom.mobile.erp
{
	[Activity (Label = "M-ERP V5", MainLauncher = true,NoHistory=true, Theme="@style/android:Theme.Holo.Light.NoActionBar" )]			
	public class LoginActivity : Activity,IEventListener
	{
		private Service1Client _client;
		string pathToDatabase;
		static volatile bool _donwloadPro = false;
		AccessRights rights;
		IGeolocator locator;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.SignIn);
			// Create your application here
			EventManagerFacade.Instance.GetEventManager().AddListener(this);
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			PackageInfo pInfo = PackageManager.GetPackageInfo (PackageName, 0);
//			Button import = FindViewById<Button>(Resource.Id.logimport);
//
//			import.Click+= (object sender, EventArgs e) => {
//				ImportDatabase();
//			};	
			Button login = FindViewById<Button>(Resource.Id.login);
			Button bexit = FindViewById<Button>(Resource.Id.exit);
			TextView txtver = FindViewById<TextView> (Resource.Id.textVer);
			EditText txtcode = FindViewById<EditText> (Resource.Id.login_code);
			bexit.Click += (object sender, EventArgs e) => {
				Finish();
				Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
			};
			//InitializeServiceClient();
			txtver.Text = "VERSION "+pInfo.VersionName;
			((GlobalvarsApp)this.Application).VERSION = pInfo.VersionName;

			AdUser user=null;
			//SQLiteConnection...CreateFile(pathToDatabase);
			if (!File.Exists (pathToDatabase)) {
				createTable (pathToDatabase);
			} else
			{
//				if (pInfo.VersionCode >= 15) {
//					if (!CheckIfColumnExists ()) {
//						UpdateDatbase ();
//						UpdateItem ();
//						UpdateTrader ();
//					}
//				}
			}
			//else {
//				user = DataHelper.GetUser (pathToDatabase);
//				UpdateDatbase ();
//				if (user !=null)
//					BeforeReLoginToCloud (user);
//			}

			user = DataHelper.GetUser (pathToDatabase);
//			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
//				var list2 = db.Table<AdUser> ().ToList<AdUser> ();
//				if (list2.Count > 0) {
//					user = list2 [0];
//				}
//			}


			if (user != null) {
				((GlobalvarsApp)this.Application).USERID_CODE = user.UserID;
				if (user.Islogon) {
					ShowMainActivity (user.CompCode, user.BranchCode);		
					return;
				} else {
					//txtcode.Enabled = false;
					//txtcode.SetBackgroundColor (Android.Graphics.Color.Gray); 
					txtcode.Visibility = ViewStates.Gone;
					login.Click += (object sender, EventArgs e) => {
						LoginLocal(user);
					};
				}
			} else {
				
				login.Click += (object sender, EventArgs e) => {
					LoginIntoCloud();
				};
			}

			CheckSystemUpdate ();

		}

		private void CheckSystemUpdate()
		{
			WCFHelper wcf = new WCFHelper ();
			try{
				Service1Client client = wcf.GetServiceClient ();
				client.GetVersionCompleted+= Client_GetVersionCompleted;
				client.GetVersionAsync ();
			}catch {
			}
		}

		void Client_GetVersionCompleted (object sender, GetVersionCompletedEventArgs e)
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
				msg = Resources.GetString(Resource.String.msg_reqcancel);
				success = false;
			}
			else
			{
				RunOnUiThread (() => SystemUpdate(e.Result ));
			}
		}

		void SystemUpdate (string version)
		{
			PackageInfo pInfo = PackageManager.GetPackageInfo (PackageName, 0);
			double webver = Convert.ToDouble (version);
			double locver = Convert.ToDouble (pInfo.VersionName);
			double predefineVer = 0;
			rights = Utility.GetAccessRights (pathToDatabase);
			if (rights != null) {
				if (rights.IsVersionControl) {
					predefineVer = DataHelper.GetPreDefineVersion (pathToDatabase, "VER:");
					if (predefineVer == webver) {
						//download form google play
					} else {
						if (predefineVer > locver) {
							DownlooadAPK (predefineVer.ToString ().Trim ());
						}
						return;// is version contral. no need to check latest version control on google play
					}
				}
			}

			if (webver > locver) {
				var builderd = new AlertDialog.Builder(this);
				builderd.SetMessage("New Version "+version+ " is available, ready to update?" );
				//builderd.SetMessage("Confirm to download database from server ? All local data will be overwritten by the downloaded data.");
				builderd.SetPositiveButton("OK", (s, e) => { UpdateSysem ();;});
				builderd.SetNegativeButton("Cancel", (s, e) => { /* do something on Cancel click */ });
				builderd.Create().Show();
			}
		}
			
		void UpdateSysem ()
		{
			PackageInfo pInfo = PackageManager.GetPackageInfo (PackageName, 0);
			String appPackageName = pInfo.PackageName;
			Android.Net.Uri url = Android.Net.Uri.Parse ("market://details?id=" + appPackageName);
			// 28 3.13
			Intent marketIntent = new Intent (Intent.ActionView, url);
			marketIntent.AddFlags (ActivityFlags.NoHistory | ActivityFlags.ClearWhenTaskReset | ActivityFlags.MultipleTask | ActivityFlags.NewTask);
			StartActivity (marketIntent);
		}

		void DownlooadAPK(string ver)
		{
			try {

				WebClient myWebClient = new WebClient ();
				var sdcard = Path.Combine (Android.OS.Environment.ExternalStorageDirectory.Path, "erpdata");
				string filename ="com.wincom.merpv5_@@.zip".Replace("@@",ver);
				string url = WCFHelper.GeUploadApkUrl () + filename;
				string localfilename = Path.Combine (sdcard, "com.wincom.merpv5.zip");
				if (File.Exists(localfilename))
					File.Delete(localfilename);

				DownloadFileHelper downfile = new DownloadFileHelper(this);
				downfile.OnFinishDownloadHandle += Downfile_OnFinishDownloadDBHandle;
				downfile.StartDownload(url,localfilename);

			} catch (Exception ex)
			{
				Toast.MakeText (this, Resources.GetString(Resource.String.msg_faildowndb), ToastLength.Long).Show ();	
			}
		}

		void Downfile_OnFinishDownloadDBHandle (string filename)
		{
			string apkfile = filename.Replace ("zip", "apk");
			var sdcard = Path.Combine (Android.OS.Environment.ExternalStorageDirectory.Path, "erpdata");
			if (File.Exists(apkfile))
				File.Delete(apkfile);
			try {
				ZipHelper.DecompressFiles (filename, sdcard);
				Java.IO.File file = new Java.IO.File (apkfile);
				file.SetReadable (true, false);
				Intent intent = new Intent(Intent.ActionInstallPackage);
				intent.SetDataAndType (Android.Net.Uri.FromFile (file), "application/vnd.android.package-archive");
				StartActivity (intent);
			} catch (Exception ex)
			{
			   Console.WriteLine(ex);
			}
		}


		void AlertShow(string text)
		{
			AlertDialog.Builder alert = new AlertDialog.Builder (this);

			alert.SetMessage (text);
			RunOnUiThread (() => {
				alert.Show();
			} );

		}

		private bool CheckIfColumnExists()
		{
			bool isfound = true;
			try{
				using (SQLite.SQLiteConnection Conn = new SQLiteConnection(pathToDatabase))
				{
					var col =Conn.GetTableInfo("AdPara");
					isfound=(col.Count >13);
				
				}
			}catch {
				isfound = false;
			}
			return isfound;
		}

//		private void UpdateDatbase()
//		{
//			try {
//				using (var conn = new SQLite.SQLiteConnection (pathToDatabase)) {
//
//					string sql = @"ALTER TABLE AdPara RENAME TO sqlitestudio_temp_table;
//								   CREATE TABLE AdPara (ID integer PRIMARY KEY AUTOINCREMENT NOT NULL, PrinterName varchar, Prefix varchar, PaperSize varchar, Warehouse varchar, ReceiptTitle varchar, RunNo integer, CNRunNo integer, SORunNo integer, DORunNo integer, CNPrefix varchar, DOPrefix varchar, SOPrefix varchar, PrinterIP varchar, PrinterType varchar,FooterNote varchar,FooterCNNote varchar,FooterDONote,FooterSONote varchar);
//								   INSERT INTO AdPara (ID, PrinterName, Prefix, PaperSize, Warehouse, ReceiptTitle, RunNo, CNRunNo, SORunNo, DORunNo, CNPrefix, DOPrefix, SOPrefix) SELECT ID, PrinterName, Prefix, PaperSize, Warehouse, ReceiptTitle, RunNo, CNRunNo, SORunNo, DORunNo, CNPrefix, DOPrefix, SOPrefix FROM sqlitestudio_temp_table;
//								   DROP TABLE sqlitestudio_temp_table";
//					string[] sqls = sql.Split (new char[]{ ';' });
//					foreach (string ssql in sqls) {
//						conn.Execute (ssql, new object[]{ });
//					}
//				}
//			} catch (Exception ex) {
//				Toast.MakeText (this, ex.Message, ToastLength.Long).Show ();	
//			}
//		}
//
//		private void UpdateItem()
//		{
//			try {
//				using (var conn = new SQLite.SQLiteConnection (pathToDatabase)) {
//
//					string sql = @"ALTER TABLE Item RENAME TO sqlitestudio_temp_table2;
//									CREATE TABLE Item (ID integer PRIMARY KEY AUTOINCREMENT NOT NULL, ICode varchar, IDesc varchar, Price float, tax float, taxgrp varchar, isincludesive integer, VIPPrice float, RetailPrice float, WholeSalePrice float, Barcode varchar, StdUom VARCHAR);
//									INSERT INTO Item (ID, ICode, IDesc, Price, tax, taxgrp, isincludesive, VIPPrice, RetailPrice, WholeSalePrice, Barcode) SELECT ID, ICode, IDesc, Price, tax, taxgrp, isincludesive, VIPPrice, RetailPrice, WholeSalePrice, Barcode FROM sqlitestudio_temp_table2;
//									DROP TABLE sqlitestudio_temp_table2";
//					string[] sqls = sql.Split (new char[]{ ';' });
//					foreach (string ssql in sqls) {
//						conn.Execute (ssql, new object[]{ });
//					}
//				}
//			} catch (Exception ex) {
//				//Toast.MakeText (this, ex.Message, ToastLength.Long).Show ();	
//				AlertShow("UpdateItem() "+ex.Message);
//			}
//		}
//
//		private void UpdateTrader()
//		{
//			try {
//				using (var conn = new SQLite.SQLiteConnection (pathToDatabase)) {
//
//					string sql = @"ALTER TABLE Trader RENAME TO sqlitestudio_temp_table3;
//								   CREATE TABLE Trader (CustCode varchar PRIMARY KEY NOT NULL, CustName varchar, Addr1 varchar, Addr2 varchar, Addr3 varchar, Addr4 varchar, Tel varchar, Fax varchar, gst varchar, PayCode varchar, CustType varchar, AgentCode VARCHAR);
//							       INSERT INTO Trader (CustCode, CustName, Addr1, Addr2, Addr3, Addr4, Tel, Fax, gst, PayCode, CustType) SELECT CustCode, CustName, Addr1, Addr2, Addr3, Addr4, Tel, Fax, gst, PayCode, CustType FROM sqlitestudio_temp_table3;
//								   DROP TABLE sqlitestudio_temp_table3";
//					string[] sqls = sql.Split (new char[]{ ';' });
//					foreach (string ssql in sqls) {
//						conn.Execute (ssql, new object[]{ });
//					}
//				}
//			} catch (Exception ex) {
//				//Toast.MakeText (this, ex.Message, ToastLength.Long).Show ();	
//				AlertShow("UpdateTrader() "+ex.Message);
//			}
//		}


		void ImportDatabase ()
		{
			var sdcard = Path.Combine (Android.OS.Environment.ExternalStorageDirectory.Path, "erpdata");
			string filename = Path.Combine (sdcard,"erplite.db");
			File.Copy(filename, pathToDatabase, true);
		}

		void ExitApp ()
		{
			((GlobalvarsApp)this.Application).ISLOGON = false;
			Finish ();
			Android.OS.Process.KillProcess (Android.OS.Process.MyPid ());

		}

		void createTable(string pathToDatabase)
		{
			using (var conn= new SQLite.SQLiteConnection(pathToDatabase))
			{
				conn.CreateTable<Item>();
				conn.CreateTable<Invoice>();
				conn.CreateTable<InvoiceDtls>();
				conn.CreateTable<Trader> ();
				conn.CreateTable<AdUser> ();
				conn.CreateTable<CompanyInfo> ();
				conn.CreateTable<AdPara> ();
				conn.CreateTable<AdNumDate> ();
				conn.CreateTable<CNNote>();
				conn.CreateTable<CNNoteDtls>();
				conn.CreateTable<SaleOrder>();
				conn.CreateTable<SaleOrderDtls>();
				conn.CreateTable<DelOrder>();
				conn.CreateTable<DelOrderDtls>();
				conn.CreateTable<GeoLocation>();

			}
		}

		private void BeforeReLoginToCloud(AdUser user)
		{
			EditText userid = FindViewById<EditText> (Resource.Id.login_userName);
			EditText passw = FindViewById<EditText> (Resource.Id.login_password);
			EditText code = FindViewById<EditText> (Resource.Id.login_code);
			userid.Text = user.UserID;
			passw.Text = user.Password;
			code.Text = user.CompCode;
			userid.Enabled = false;
			code.Enabled = false;
		}

		private void LoginIntoCloud()
		{
			EditText userid = FindViewById<EditText> (Resource.Id.login_userName);
			EditText passw = FindViewById<EditText> (Resource.Id.login_password);
			EditText code = FindViewById<EditText> (Resource.Id.login_code);
			Button login = FindViewById<Button>(Resource.Id.login);
			login.Enabled = false;
			login.Text = Resources.GetString (Resource.String.msg_plswait);// "Please wait...";
			//_client.LoginAsync (userid.Text, passw.Text, code.Text);
			DownloadHelper download= new DownloadHelper();
			download.Downloadhandle = LoginDoneDlg; 
			download.DownloadAllhandle =LoginDoneDlgEx; 
			download.CallingActivity = this;
			download.startLogin(userid.Text, passw.Text, code.Text);
		}

		private void LoginDoneDlg(Activity callingAct,int count,string msg)
		{
			Button login = FindViewById<Button>(Resource.Id.login);
			login.Enabled = true;
			login.Text =  Resources.GetString(Resource.String.but_login);
			Toast.MakeText (this, msg, ToastLength.Long).Show ();	
		}

		private void LoginDoneDlgEx(Activity callingAct,int count,string msg)
		{
			Button login = FindViewById<Button>(Resource.Id.login);
			login.Enabled = false;
			//login.Text = "LOGIN";
			Toast.MakeText (this, msg, ToastLength.Long).Show ();	
		}

		private void LoginLocal(AdUser user)
		{
			EditText userid = FindViewById<EditText> (Resource.Id.login_userName);
			EditText passw = FindViewById<EditText> (Resource.Id.login_password);
			EditText code = FindViewById<EditText> (Resource.Id.login_code);

			if (user.UserID.ToUpper () == userid.Text.ToUpper ()) {
				if (user.Password == passw.Text) {
					if (isLastConnectExpire (user)) {
						user.Islogon = false;
						RemoveUser (user);
						DownloadCOmpleted (Resources.GetString (Resource.String.msg_faillogin));
					} else {
						user.Islogon = true;
						UpdateLogin (user);
						ShowMainActivity (user.CompCode, user.BranchCode);	
					}
				} else {
					DownloadCOmpleted (Resources.GetString (Resource.String.msg_faillogin));
				}
			}else DownloadCOmpleted (Resources.GetString (Resource.String.msg_faillogin));

		}

		private bool isLastConnectExpire(AdUser user)
		{
			bool isExpiry = false;
			rights = Utility.GetAccessRights (pathToDatabase);
			int Expiry = DataHelper.GetExpiryDay (pathToDatabase);
			if (rights.IsLoginControl) {
				if (user.LastConnect.Year < 2000)
					user.LastConnect = DateTime.Now;
				double day = (DateTime.Now - user.LastConnect).TotalDays;
				isExpiry = (day > Expiry);
			}

			return isExpiry;
		}

		void DownloadCOmpleted (string msg)
		{
			Toast.MakeText (this, msg, ToastLength.Long).Show ();	
			Button login = FindViewById<Button> (Resource.Id.login);
			login.Enabled = true;
			login.Text =  Resources.GetString(Resource.String.but_login);//"LOGIN";
		}

		void UpdateLogin(AdUser user)
		{
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				db.Update (user);
			}
		}

		void RemoveUser(AdUser user)
		{
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				db.Delete (user);
			}
		}

		void ShowMainActivity (string comp,string bran)
		{
			var intent = new Intent (this, typeof(MainActivity));
			//intent.PutExtra ("COMPCODE",comp );
			//intent.PutExtra ("BRANCH",bran );
			((GlobalvarsApp)this.Application).COMPANY_CODE = comp;
			((GlobalvarsApp)this.Application).BRANCH_CODE = bran;
			((GlobalvarsApp)this.Application).ISLOGON = true;
			((GlobalvarsApp)this.Application).USERFUNCTION = DataHelper.GetUserFunctionName (pathToDatabase);

			StartActivity (intent);
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
			case EventID.LOGIN_DOWNCOMPLETE:
				string comp =((GlobalvarsApp)this.Application).COMPANY_CODE;
				string bran = ((GlobalvarsApp)this.Application).BRANCH_CODE;
				ShowMainActivity (comp,bran);
				break;
			case EventID.LOGIN_SUCCESS:
				if (!_donwloadPro) {
					DownloadHelper download = new DownloadHelper ();
					download.Downloadhandle = LoginDoneDlg; 
					download.DownloadAllhandle = LoginDoneDlgEx; 
					download.CallingActivity = this;
					download.StartDownloadAll ();
					_donwloadPro = true;
				}
				break;
			}
		}



	}
}

