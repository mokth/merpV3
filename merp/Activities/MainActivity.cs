using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using SQLite;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using WcfServiceItem;
using Android.Telephony;
using Android.Accounts;
using Android.Text;
using System.Net;
using Android.Content.PM;
using ICSharpCode.SharpZipLib.Zip;


namespace wincom.mobile.erp
{
	[Activity (Label = "WINCOM M-ERP V4",Icon = "@drawable/icon")]
	public class MainActivity :Activity
	{
		//List<Item> items = null;

		private Service1Client _client;
		string pathToDatabase;
		string COMPCODE;
		string BRANCODE;
		string USERID;
		AccessRights rights;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!((GlobalvarsApp)this.Application).ISLOGON) {
				Finish ();
				return;
			}
			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);
			GetDBPath ();
			rights = Utility.GetAccessRights (pathToDatabase);
			AndroidEnvironment.UnhandledExceptionRaiser += AndroidEnvironment_UnhandledExceptionRaiser;
			Button but = FindViewById<Button> (Resource.Id.butSecond);
			but.Click += butClick;
			Button butCash = FindViewById<Button> (Resource.Id.butCash);
			butCash.Click += butCashClick;
			Button butTrxList = FindViewById<Button> (Resource.Id.butInvlist);
			butTrxList.Click += ButInvlist_Click;
			Button butPOS = FindViewById<Button> (Resource.Id.butPOS);
			//butPOS.Visibility = ViewStates.Gone;
			butPOS.Click += ButPOS_Click;
			Button butdown = FindViewById<Button> (Resource.Id.butDown);
			butdown.Click += butDownloadItems;
			Button butup = FindViewById<Button> (Resource.Id.butupload);
			butup.Click += butUploadItems;
			Button butMItem = FindViewById<Button> (Resource.Id.butMaster);
			butMItem.Click += butMasterClick;
			Button butSett = FindViewById<Button> (Resource.Id.butsetting);
			butSett.Click += butSetting;
			Button butlogOff = FindViewById<Button> (Resource.Id.butOut);
			butlogOff.Click += ButlogOff_Click;
			//Button butAbt = FindViewById<Button> (Resource.Id.butAbout);
			//butAbt.Click+= ButAbt_Click;
			Button butCNNote = FindViewById<Button> (Resource.Id.butcnnote);
			butCNNote.Click += ButCNNote_Click;
			Button butSO = FindViewById<Button> (Resource.Id.butso);
			butSO.Click += ButSO_Click;
			Button butDO = FindViewById<Button> (Resource.Id.butdo);
			butDO.Click += ButDO_Click;
			if (!rights.IsSOModule) {
				butSO.Visibility = ViewStates.Gone;
			}
			if (!rights.IsCNModule) {
				butCNNote.Visibility = ViewStates.Gone;
			}
			if (!rights.IsDOModule) {
				butDO.Visibility = ViewStates.Gone;
			}

		}
		

		private void butClick(object sender,EventArgs e)
		{
			var intent =ActivityManager.GetActivity<InvoiceActivity>(this.ApplicationContext);
			StartActivity(intent);
		}

		private void butCashClick(object sender,EventArgs e)
		{
			var intent =ActivityManager.GetActivity<CashActivity>(this.ApplicationContext);
			StartActivity(intent);
		}


		void ButPOS_Click (object sender, EventArgs e)
		{
			var intent =ActivityManager.GetActivity<POSSalesActivity>(this.ApplicationContext);
			StartActivity(intent);
		}

		void ButSO_Click (object sender, EventArgs e)
		{
			//var intent2 = new Intent(this, typeof(SalesOrderActivity));
			var intent =ActivityManager.GetActivity<SalesOrderActivity>(this.ApplicationContext);
			StartActivity(intent);
			
		}

		void ButDO_Click (object sender, EventArgs e)
		{
			//var intent2 = new Intent(this, typeof(SalesOrderActivity));
			var intent =ActivityManager.GetActivity<DelOrderActivity>(this.ApplicationContext);
			StartActivity(intent);

		}

		void ButCNNote_Click (object sender, EventArgs e)
		{
			//var intent = new Intent(this, typeof(CNNoteActivity));
			var intent =ActivityManager.GetActivity<CNNoteActivity>(this.ApplicationContext);
			StartActivity(intent);
		}

		void ButInvlist_Click (object sender, EventArgs e)
		{
			//var intent = new Intent(this, typeof(TransListActivity));
			var intent =ActivityManager.GetActivity<TransListActivity>(this.ApplicationContext);
			StartActivity(intent);
		}

		void butDownloadItems(object sender,EventArgs e)
		{
			StartActivity (typeof(DownloadActivity));

		}

		void butUploadItems(object sender,EventArgs e)
		{
			StartActivity (typeof(UploadActivity));

		}

		private void butMasterClick(object sender,EventArgs e)
		{
			var intent = new Intent(this, typeof(MasterRefActivity));

			StartActivity(intent);
		}

		void butSetting(object sender,EventArgs e)
		{
			//StartActivity (typeof(SettingActivity));
			var intent =ActivityManager.GetActivity<UtilityActivity>(this.ApplicationContext);
			StartActivity(intent);
		}

		void ButAbt_Click (object sender, EventArgs e)
		{
			CompanyInfo comp= DataHelper.GetCompany (pathToDatabase);
			View messageView = LayoutInflater.Inflate(Resource.Layout.About, null, false);
			PackageInfo pInfo = PackageManager.GetPackageInfo (PackageName, 0);
			// When linking text, force to always use default color. This works
			// around a pressed color state bug.
			TextView textView = (TextView) messageView.FindViewById(Resource.Id.about_credits);
			TextView textDesc = (TextView) messageView.FindViewById(Resource.Id.about_descrip);
			TextView textVer = (TextView) messageView.FindViewById(Resource.Id.about_ver);
			//textDesc.Text = Html.FromHtml (Resources.GetString(Resource.String.app_descrip))..ToString();
			textView.Text = "For inquiry, please contact " + comp.SupportContat;
			textVer .Text = "Build Version : "+pInfo.VersionName;
			AlertDialog.Builder builder = new AlertDialog.Builder(this);
			builder.SetIcon(Resource.Drawable.Icon);
			builder.SetTitle(Resource.String.app_name);
			builder.SetView(messageView);
			builder.Create();
			builder.Show();
		}

		void AndroidEnvironment_UnhandledExceptionRaiser (object sender, RaiseThrowableEventArgs e)
		{
			Toast.MakeText (this, e.Exception.Message, ToastLength.Long).Show ();	
			StartActivity (typeof(MainActivity));
			Finish ();
		}

		protected override void Dispose(bool disposing)
		{
			AndroidEnvironment.UnhandledExceptionRaiser -=AndroidEnvironment_UnhandledExceptionRaiser;
			base.Dispose(disposing);
		}

		public override bool  OnCreateOptionsMenu(IMenu menu)
		{
			MenuInflater.Inflate(Resource.Menu.MainMenu,menu);
			return base.OnPrepareOptionsMenu(menu);
		}

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			switch (item.ItemId)
			{
			case Resource.Id.mmenu_back:
				BackupDatabase ();
				return true;
			case Resource.Id.mmenu_downdb:
				var builderd = new AlertDialog.Builder(this);

				builderd.SetMessage(Resources.GetString(Resource.String.msg_confirmoverwrite));
				//builderd.SetMessage("Confirm to download database from server ? All local data will be overwritten by the downloaded data.");
				builderd.SetPositiveButton("OK", (s, e) => { DownlooadDb ();;});
				builderd.SetNegativeButton("Cancel", (s, e) => { /* do something on Cancel click */ });
				builderd.Create().Show();
				return true;
			case Resource.Id.mmenu_downtmp:
				RunOnUiThread(() =>DownlooadTemplate()) ;
				return true; 
			case Resource.Id.mmenu_downlogo:
				RunOnUiThread(() =>DownlooadLogo()) ;
				return true;
			case Resource.Id.mmenu_setting:
				StartActivity (typeof(SettingActivity));
				return true;
			case Resource.Id.mmenu_logoff:
				RunOnUiThread(() =>ExitAndLogOff()) ;
				return true;
			case Resource.Id.mmenu_downcompinfo:
				DownloadCompInfo ();
				return true;
			case Resource.Id.mmenu_clear:
				var builder = new AlertDialog.Builder(this);
				builder.SetMessage(Resources.GetString(Resource.String.msg_confirmcleer));
				builder.SetPositiveButton("OK", (s, e) => { ClearPostedInv () ;});
				builder.SetNegativeButton("Cancel", (s, e) => { /* do something on Cancel click */ });
				builder.Create().Show();

				return true;
			}
		
			return base.OnOptionsItemSelected(item);
		}

		void GetDBPath ()
		{
			COMPCODE = ((GlobalvarsApp)this.Application).COMPANY_CODE;
			BRANCODE = ((GlobalvarsApp)this.Application).BRANCH_CODE;
			USERID = ((GlobalvarsApp)this.Application).USERID_CODE;
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
		}

		protected override void OnResume()
		{
			base.OnResume();
			GetDBPath ();
		}

		void DownlooadDb()
		{
			try {
				//backup db first before upload
				BackupDatabase();

				WebClient myWebClient = new WebClient ();
				var sdcard = Path.Combine (Android.OS.Environment.ExternalStorageDirectory.Path, "erpdata");
				string filename = COMPCODE + "_" + BRANCODE + "_" + USERID + "_erplite.db";
				string url = WCFHelper.GetDownloadDBUrl () + filename;
				string localfilename = Path.Combine (sdcard, "erplite.db");
				if (File.Exists(localfilename))
					File.Delete(localfilename);

				myWebClient.DownloadFile (url, localfilename);  
				File.Copy (localfilename, pathToDatabase, true);

				//delete the file after downloaded
				string urldel = WCFHelper.GeUploadDBUrl()+"/afterdownload.aspx?ver=3&ID="+filename;
				WebRequest request = HttpWebRequest.Create(urldel);
				request.GetResponse();

				Toast.MakeText (this, Resources.GetString(Resource.String.msg_successdowndb), ToastLength.Long).Show ();	
			} catch (Exception ex)
			{
				Toast.MakeText (this, Resources.GetString(Resource.String.msg_faildowndb), ToastLength.Long).Show ();	
			}
		}

		void DownlooadTemplate()
		{
			try {

				WebClient myWebClient = new WebClient ();
				var sdcard = Path.Combine (Android.OS.Environment.ExternalStorageDirectory.Path, "erpdata");
				string filename = COMPCODE + "_template.zip";
				string url = WCFHelper.GetDownloadTemplateUrl () + filename;
				string localfilename = Path.Combine (sdcard, filename);
				if (File.Exists(localfilename))
					File.Delete(localfilename);

				myWebClient.DownloadFile (url, localfilename);  
				string pathname= Path.GetDirectoryName(pathToDatabase);
				ZipHelper.DecompressFiles(localfilename,pathname);

				Toast.MakeText (this, Resources.GetString(Resource.String.msg_successdowntmp), ToastLength.Long).Show ();	
			} catch (Exception ex)
			{
				Toast.MakeText (this, Resources.GetString(Resource.String.msg_faildowntmp), ToastLength.Long).Show ();	
			}
		}

		void DownlooadLogo()
		{
			try {
				//backup db first before upload
				AdPara apara =  DataHelper.GetAdPara (pathToDatabase);
				WebClient myWebClient = new WebClient ();
				string document = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
				string logopath = "";
				string filename ="";
				if (apara.PaperSize=="58mm"){
					logopath = Path.Combine (document, "logo58.png");
				    filename = COMPCODE + "_" + BRANCODE + "_58_logo.png";
				}
				else {
					logopath = Path.Combine (document, "logo80.png");
					filename = COMPCODE + "_" + BRANCODE + "_80_logo.png";
				}
				string url = WCFHelper.GetDownloadDBUrl () + filename;
//				string localfilename = Path.Combine (sdcard, "logo.png");
				if (File.Exists (logopath))
					File.Delete (logopath);

				myWebClient.DownloadFile (url, logopath);  
				//File.Copy (localfilename, logopath, true);

//				Toast.MakeText (this, Resources.GetString (Resource.String.msg_successdownlogo), ToastLength.Long).Show ();	
			} catch (Exception ex) {
				Toast.MakeText (this, Resources.GetString (Resource.String.msg_faildowndb), ToastLength.Long).Show ();	
			}
		}

		void ClearPostedInv()
		{
			CompanyInfo para = DataHelper.GetCompany (pathToDatabase);
			if (!para.AllowClrTrxHis) {
				Toast.MakeText (this, Resources.GetString(Resource.String.msg_accessdenied), ToastLength.Long).Show ();	
				return;
			}
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				db.DeleteAll<InvoiceDtls> ();
				db.DeleteAll<Invoice> ();
				db.DeleteAll<CNNoteDtls> ();
				db.DeleteAll<CNNote> ();
				Toast.MakeText (this,Resources.GetString(Resource.String.msg_trxclear), ToastLength.Long).Show ();	
			}
		}

		void BackupDatabase ()
		{
			var sdcard = Path.Combine (Android.OS.Environment.ExternalStorageDirectory.Path, "erpdata");
			if (!Directory.Exists (sdcard)) {
				Directory.CreateDirectory (sdcard);
			}
			string filename = Path.Combine (sdcard,"erplite"+ DateTime.Now.ToString("yyMMddHHmm") +".db");
			if (File.Exists (pathToDatabase)) {
				File.Copy (pathToDatabase, filename, true);
				filename = ZipHelper.GetZipFileName(filename);
				UploadToErpHostForSupport (filename);
			}
		}
			

		private void UploadToErpHostForSupport(string filename)
		{
			WebClient myWebClient = new WebClient ();
			try {
				
				myWebClient.QueryString ["COMP"] = COMPCODE;
				myWebClient.QueryString ["BRAN"] = BRANCODE;
				myWebClient.QueryString ["USER"] = USERID;
				if (filename.ToLower().Contains(".zip"))
				{
					//upload zip db file and extract
					byte[] responseArray = myWebClient.UploadFile (@"http://www.wincomcloud.com/UploadDb/uploadDbEx.aspx", filename);
				}else{
					//upload db file
					byte[] responseArray = myWebClient.UploadFile (@"http://www.wincomcloud.com/UploadDb/uploadDb.aspx", filename);
				}
			
			} catch {
			
			}
		}

		void butBackUpDb(object sender,EventArgs e)
		{
			BackupDatabase ();
		}


		private void DownloadCompInfo()
		{
			DownloadHelper download= new DownloadHelper();
			download.Downloadhandle = OnDownProfileDoneDlg; 
			download.CallingActivity = this;
			download.startDownloadCompInfo();
		}

		private void OnDownProfileDoneDlg(Activity callingAct,int count,string msg)
		{
			Toast.MakeText (this, msg, ToastLength.Long).Show ();
		}

		void ExitApp ()
		{
			//var intent = new Intent (this, typeof(LoginActivity));
			//StartActivity (intent);
			((GlobalvarsApp)this.Application).ISLOGON = false;
			Finish ();
			Android.OS.Process.KillProcess (Android.OS.Process.MyPid ());
			Parent.Finish ();
			Intent intent = new Intent(Intent.ActionMain);
 			intent.AddCategory(Intent.CategoryHome);
			intent.SetFlags(ActivityFlags.NewTask);
			StartActivity(intent);

		}

		void ExitAndLogOff ()
		{
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list2 = db.Table<AdUser> ().ToList<AdUser> ();
				if (list2.Count > 0) {
					list2 [0].Islogon = false;
					db.Update (list2 [0], typeof(AdUser));
				}
			}
			RunOnUiThread (() => ExitApp ());
		}

		void ButlogOff_Click (object sender, EventArgs e)
		{
			//RunOnUiThread (() => ExitApp ());
			RunOnUiThread (() => ExitAndLogOff());

		}

		public override void OnBackPressed() {
			// do nothing.
		}

	}
}


