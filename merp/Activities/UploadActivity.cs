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

namespace wincom.mobile.erp
{
	[Activity (Label = "UPLOAD",Icon="@drawable/upload")]			
	//[Activity (Label = Resources.GetString(Resource.String.mainmenu_downupload))]			
	public class UploadActivity : Activity
	{
		AccessRights rights;
		string pathToDatabase;
		Button butupload;
		Button butuploadso;
		Button butuploadcn;
		Button butuploaddo;
		Button butuploadall;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetTitle (Resource.String.mainmenu_upload);
			SetContentView (Resource.Layout.Upload);
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			rights = Utility.GetAccessRights (pathToDatabase);

			butupload = FindViewById<Button> (Resource.Id.butupload);
			butupload.Click += butUploadBills;

			butuploadso = FindViewById<Button> (Resource.Id.butuploadso);
			butuploadso.Click += butUploadSO;

			butuploadcn = FindViewById<Button> (Resource.Id.butuploadcn);
			butuploadcn.Click += butUploadCN;

			butuploaddo = FindViewById<Button> (Resource.Id.butuploaddo);
			butuploaddo.Click += butUploadDO;

			butuploadall = FindViewById<Button> (Resource.Id.butuploadAll);
			butuploadall.Click += butUploadAll;

			Button butback = FindViewById<Button> (Resource.Id.butMain);
			butback.Click+= (object sender, EventArgs e) => {
				base.OnBackPressed();
			};

			if (!rights.IsSOModule) {
				butuploadso.Visibility = ViewStates.Gone;
			}
			if (!rights.IsCNModule) {
				butuploadcn.Visibility = ViewStates.Gone;
			}
			if (!rights.IsDOModule) {
				butuploaddo.Visibility = ViewStates.Gone;
			}
			GetUnLoadNumber ();
		}

		void GetUnLoadNumber()
		{   
			int ttlNum = 0;
			int invnum =DataHelper.GetTotalUnUpLoadInv (pathToDatabase);
			ttlNum = ttlNum + invnum;
			butupload.Text = Resources.GetString (Resource.String.submenu_upinv)+" ("+invnum.ToString()+")";
			if (rights.IsSOModule) {
			   int invSO =DataHelper.GetTotalUnUpLoadSO (pathToDatabase);
				ttlNum = ttlNum + invSO;
				butuploadso.Text = Resources.GetString (Resource.String.submenu_upso)+" ("+invSO.ToString()+")";
			}
			if (rights.IsCNModule) {
				int invCN =DataHelper.GetTotalUnUpLoadCN (pathToDatabase);
				ttlNum = ttlNum + invCN;
				butuploadcn.Text = Resources.GetString (Resource.String.submenu_upcn)+" ("+invCN.ToString()+")";
			}
			if (rights.IsDOModule) {
				int invDO =DataHelper.GetTotalUnUpLoadDO (pathToDatabase);
				ttlNum = ttlNum + invDO;
				butuploaddo.Text = Resources.GetString (Resource.String.submenu_updo)+" ("+invDO.ToString()+")";
			}

			butuploadall.Text = Resources.GetString (Resource.String.submenu_upall)+" ("+ttlNum.ToString()+")";
		}

		void butUploadAll(object sender,EventArgs e)
		{
			Button butupload =  FindViewById<Button> (Resource.Id.butuploadAll);
			butupload.Enabled = false;
			butupload.Text = Resources.GetString(Resource.String.msg_uploading);// "Uploading, please wait...";
			//UploadBillsToServer();
			//IUploadHelper upload =UploadManager.GetUploadHepler< UploadDOHelper>();
			IUploadHelper upload =UploadManager.GetUploadHepler<UploadAllTrxHelper>();
			upload.SetUploadHandel(OnUploadAllDoneDlg); 
			upload.SetCallingActivity(this);
			upload.startUpload ();	
		}
	
		void butUploadBills(object sender,EventArgs e)
		{
			Button butupload =  FindViewById<Button> (Resource.Id.butupload);
			butupload.Enabled = false;
			butupload.Text = Resources.GetString(Resource.String.msg_uploading);// "Uploading, please wait...";
			//UploadBillsToServer();
			IUploadHelper upload =UploadManager.GetUploadHepler< UploadInvHelper>();
			upload.SetUploadHandel(OnUploadDoneDlg); 
			upload.SetCallingActivity(this);
			upload.startUpload ();		
		}

		void butUploadSO(object sender,EventArgs e)
		{
			Button butupload =  FindViewById<Button> (Resource.Id.butuploadso);
			butupload.Enabled = false;
			butupload.Text = Resources.GetString(Resource.String.msg_uploading);// "Uploading, please wait...";
			//UploadBillsToServer();
			IUploadHelper upload =UploadManager.GetUploadHepler< UploadSOHelper>();
			upload.SetUploadHandel(OnUploadSODoneDlg); 
			upload.SetCallingActivity(this);
			upload.startUpload ();	
		}

		void butUploadCN(object sender,EventArgs e)
		{
			Button butupload =  FindViewById<Button> (Resource.Id.butuploadcn);
			butupload.Enabled = false;
			butupload.Text = Resources.GetString(Resource.String.msg_uploading);// "Uploading, please wait...";
			//UploadBillsToServer();
			IUploadHelper upload =UploadManager.GetUploadHepler< UploadCNHelper>();
			upload.SetUploadHandel(OnUploadCNDoneDlg); 
			upload.SetCallingActivity(this);
			upload.startUpload ();	
		}


		void butUploadDO(object sender,EventArgs e)
		{
			Button butupload =  FindViewById<Button> (Resource.Id.butuploaddo);
			butupload.Enabled = false;
			butupload.Text = Resources.GetString(Resource.String.msg_uploading);// "Uploading, please wait...";
			//UploadBillsToServer();
			IUploadHelper upload =UploadManager.GetUploadHepler< UploadDOHelper>();
			upload.SetUploadHandel(OnUploadDODoneDlg); 
			upload.SetCallingActivity(this);
			upload.startUpload ();	
		}

		private void OnUploadAllDoneDlg(Activity callingAct,int count,string msg)
		{
			Button butupload = callingAct.FindViewById<Button> (Resource.Id.butuploadAll);
			butupload.Text =  Resources.GetString(Resource.String.submenu_upall);// "UPLOAD INVOICE";
			butupload.Enabled = true;
			GetUnLoadNumber ();
			if (count > 0) {
				//string dispmsg = "Total " + count.ToString () + " invoices uploaded.";
				string dispmsg =Resources.GetString(Resource.String.msg_upload);
				dispmsg = dispmsg.Replace ("xx", count.ToString ());
				Toast.MakeText (this, dispmsg, ToastLength.Long).Show ();	
			} else {
				Toast.MakeText (this, msg, ToastLength.Long).Show ();	
			}
		}

		private void OnUploadDoneDlg(Activity callingAct,int count,string msg)
		{
			Button butupload = callingAct.FindViewById<Button> (Resource.Id.butupload);
			butupload.Text =  Resources.GetString(Resource.String.submenu_upinv);// "UPLOAD INVOICE";
			butupload.Enabled = true;
			GetUnLoadNumber ();
			if (count > 0) {
				//string dispmsg = "Total " + count.ToString () + " invoices uploaded.";
				string dispmsg =Resources.GetString(Resource.String.msg_uploadinv);
				dispmsg = dispmsg.Replace ("xx", count.ToString ());
				AutoDownloadSettings ();
				Toast.MakeText (this, dispmsg, ToastLength.Long).Show ();	
			} else {
				Toast.MakeText (this, msg, ToastLength.Long).Show ();	
			}
		}

		void AutoDownloadSettings ()
		{
			DownloadHelper download= new DownloadHelper();
			download.Downloadhandle = DownSettingDoneDlg; 
			download.CallingActivity = this;
			download.startDownloadCompInfo();
		}

		private void DownSettingDoneDlg(Activity callingAct,int count,string msg)
		{
			if (count > 0) {
				string dispmsg =  Resources.GetString(Resource.String.msg_downsettings);
				Toast.MakeText (this, dispmsg, ToastLength.Long).Show ();	
			} else {
				Toast.MakeText (this, msg, ToastLength.Long).Show ();	
			}
		}

		private void OnUploadSODoneDlg(Activity callingAct,int count,string msg)
		{
			Button butupload = callingAct.FindViewById<Button> (Resource.Id.butuploadso);
			butupload.Text =  Resources.GetString(Resource.String.submenu_upso);// "UPLOAD INVOICE";
			butupload.Enabled = true;
			GetUnLoadNumber ();
			if (count > 0) {
				//string dispmsg = "Total " + count.ToString () + " invoices uploaded.";
				string dispmsg =Resources.GetString(Resource.String.msg_uploadso);
				dispmsg = dispmsg.Replace ("xx", count.ToString ());
				Toast.MakeText (this, dispmsg, ToastLength.Long).Show ();	
			} else {
				Toast.MakeText (this, msg, ToastLength.Long).Show ();	
			}
		}

		private void OnUploadDODoneDlg(Activity callingAct,int count,string msg)
		{
			Button butupload = callingAct.FindViewById<Button> (Resource.Id.butuploaddo);
			butupload.Text =  Resources.GetString(Resource.String.submenu_updo);// "UPLOAD INVOICE";
			butupload.Enabled = true;
			GetUnLoadNumber ();
			if (count > 0) {
				//string dispmsg = "Total " + count.ToString () + " invoices uploaded.";
				string dispmsg =Resources.GetString(Resource.String.msg_uploaddo);
				dispmsg = dispmsg.Replace ("xx", count.ToString ());
				Toast.MakeText (this, dispmsg, ToastLength.Long).Show ();	
			} else {
				Toast.MakeText (this, msg, ToastLength.Long).Show ();	
			}
		}

		private void OnUploadCNDoneDlg(Activity callingAct,int count,string msg)
		{
			Button butupload = callingAct.FindViewById<Button> (Resource.Id.butuploadcn);
			butupload.Text =  Resources.GetString(Resource.String.submenu_upcn);// "UPLOAD INVOICE";
			butupload.Enabled = true;
			GetUnLoadNumber ();
			if (count > 0) {
				//string dispmsg = "Total " + count.ToString () + " invoices uploaded.";
				string dispmsg =Resources.GetString(Resource.String.msg_uploadcn);
				dispmsg = dispmsg.Replace ("xx", count.ToString ());
				Toast.MakeText (this, dispmsg, ToastLength.Long).Show ();	
			} else {
				Toast.MakeText (this, msg, ToastLength.Long).Show ();	
			}
		}
	
	}
}

