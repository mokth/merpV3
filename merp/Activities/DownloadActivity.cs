
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
	[Activity (Label = "DOWNLOAD/UPLOAD",Icon="@drawable/download")]			
	//[Activity (Label = Resources.GetString(Resource.String.mainmenu_downupload))]			
	public class DownloadActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetTitle (Resource.String.mainmenu_downupload);
			SetContentView (Resource.Layout.Download);

			Button butdownItem = FindViewById<Button> (Resource.Id.butDown);
			butdownItem.Click += butDownloadItems;

			Button butdownCust = FindViewById<Button> (Resource.Id.butDownCust);
			butdownCust.Click += butDownloadCusts;

			Button butupload = FindViewById<Button> (Resource.Id.butupload);
			butupload.Click += butUploadBills;

			Button butuploadso = FindViewById<Button> (Resource.Id.butuploadso);
			butuploadso.Click += butUploadSO;

			Button butuploadcn = FindViewById<Button> (Resource.Id.butuploadcn);
			butuploadcn.Click += butUploadCN;

			Button butdownSetting = FindViewById<Button> (Resource.Id.butDownSetting);
			butdownSetting.Click+= ButdownSetting_Click;

			Button butback = FindViewById<Button> (Resource.Id.butMain);
			butback.Click+= (object sender, EventArgs e) => {
				base.OnBackPressed();
			};
		}

		void ButdownSetting_Click (object sender, EventArgs e)
		{
			Button butDown =  FindViewById<Button> (Resource.Id.butDownSetting);
			butDown.Enabled = false;
			DownloadHelper download= new DownloadHelper();
			download.Downloadhandle = DownSettingDoneDlg; 
			download.CallingActivity = this;
			download.startDownloadCompInfo();
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
			upload.SetUploadHandel(OnUploadDoneDlg); 
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
			upload.SetUploadHandel(OnUploadDoneDlg); 
			upload.SetCallingActivity(this);
			upload.startUpload ();	
		}


		void butDownloadItems(object sender,EventArgs e)
		{
			Button butDown =  FindViewById<Button> (Resource.Id.butDown);
			butDown.Enabled = false;
			DownloadHelper download= new DownloadHelper();
			download.Downloadhandle = DownItemsDoneDlg; 
			download.CallingActivity = this;
			download.NotDownloadAll ();
			download.startDownloadItem ();
		}

		void butDownloadCusts(object sender,EventArgs e)
		{
			Button butDown =  FindViewById<Button> (Resource.Id.butDownCust);
			butDown.Enabled = false;
			DownloadHelper download= new DownloadHelper();
			download.Downloadhandle =  DownCustDoneDlg; 
			download.CallingActivity = this;
			download.NotDownloadAll ();
			download.startDownloadCustomer ();
		}

		private void DownCustDoneDlg(Activity callingAct,int count,string msg)
		{
			Button butdown = FindViewById<Button> (Resource.Id.butDownCust);
			butdown.Enabled = true;
			if (count > 0) {
				//string dispmsg = "Total " + count.ToString () + " Customers downloaded.";
				string dispmsg =Resources.GetString(Resource.String.msg_downcust);
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
			if (count > 0) {
				//string dispmsg = "Total " + count.ToString () + " invoices uploaded.";
				string dispmsg =Resources.GetString(Resource.String.msg_uploadinv);
				dispmsg = dispmsg.Replace ("xx", count.ToString ());
				Toast.MakeText (this, dispmsg, ToastLength.Long).Show ();	
			} else {
				Toast.MakeText (this, msg, ToastLength.Long).Show ();	
			}
		}

		private void DownItemsDoneDlg(Activity callingAct,int count,string msg)
		{
			Button butdown = FindViewById<Button> (Resource.Id.butDown);
			butdown.Enabled = true;
			if (count > 0) {
				string dispmsg =Resources.GetString(Resource.String.msg_downitem);
				dispmsg = dispmsg.Replace ("xx", count.ToString ());
				//string dispmsg = "Total " + count.ToString () + " Items downloaded.";
				Toast.MakeText (this, dispmsg, ToastLength.Long).Show ();	
			} else {
				Toast.MakeText (this, msg, ToastLength.Long).Show ();	
			}
		}

		private void DownSettingDoneDlg(Activity callingAct,int count,string msg)
		{
			Button butdown = FindViewById<Button> (Resource.Id.butDownSetting);
			butdown.Enabled = true;
			if (count > 0) {
				string dispmsg =  Resources.GetString(Resource.String.msg_downsettings);
				Toast.MakeText (this, dispmsg, ToastLength.Long).Show ();	
			} else {
				Toast.MakeText (this, msg, ToastLength.Long).Show ();	
			}
		}
	}
}

