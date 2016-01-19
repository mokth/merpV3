using System;
using System.IO;
using Android.App;
using System.Threading;
using Android.Widget;
using System.Net;

namespace wincom.mobile.erp
{
	public delegate void OnFinishDownload(string filename);

	public class DownloadFileHelper:Activity
	{
		ProgressDialog progress;
		private Activity CallingActivity;
		private string filename;
		public event OnFinishDownload OnFinishDownloadHandle;
		public bool NeedBackupDb { get; set; }

		public  DownloadFileHelper(Activity parent)
		{
			CallingActivity = parent;
			progress = new ProgressDialog(CallingActivity);
			progress.Indeterminate = false;
			progress.SetProgressStyle(ProgressDialogStyle.Horizontal);
			progress.SetMessage("Downloading. Please wait...");
			progress.SetCancelable(false);
			progress.Max = 100;
			//OnFinishDownloadHandle += new OnFinishDownload (FinishDownload);
		}

		public void StartDownload (string url, string localfilename)
		{
			try {
				filename =localfilename;
				progress.Show();

				var progressDialog = ProgressDialog.Show(CallingActivity, "Please wait...", "Downloading. Please wait..", true);
				new Thread(new ThreadStart(delegate
					{
						RunOnUiThread(() =>statDownload(new Uri(url),localfilename));
						RunOnUiThread(() => progressDialog.Hide());
					})).Start();


			} catch (Exception ex) {
				Toast.MakeText (this, Resources.GetString (Resource.String.msg_faildowndb), ToastLength.Long).Show ();	
			}

		}

		void statDownload(Uri url, string localfilename)
		{
			if (NeedBackupDb)
				UploadDbHelper.BackupDatabase();
			
			WebClient myWebClient = new WebClient ();
			myWebClient.DownloadProgressChanged += MyWebClient_DownloadProgressChanged;
			myWebClient.DownloadFileCompleted += MyWebClient_DownloadFileCompleted;
			Thread.Sleep (50);
			try {
				myWebClient.DownloadFileAsync (url, localfilename);
			} catch (Exception ex) {
				filename = "";
				Console.WriteLine (ex);
			}
		}

		void MyWebClient_DownloadProgressChanged (object sender, DownloadProgressChangedEventArgs e)
		{
			double bytesIn = double.Parse(e.BytesReceived.ToString());
			double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
			double percentage = bytesIn / totalBytes * 100;
			progress.Progress = int.Parse (Math.Truncate (percentage).ToString ());
		}

		void MyWebClient_DownloadFileCompleted (object sender, System.ComponentModel.AsyncCompletedEventArgs e)
		{
			if (e.Cancelled)
				filename = "";
			if (e.Error != null)
				filename = "";
			
			progress.Progress= 100;
			FinishDownload (filename);		
		}

		void FinishDownload(string filename)
		{
			if (OnFinishDownloadHandle != null)
				OnFinishDownloadHandle (filename);

			progress.Dismiss ();
		}

		public void StartBackupDb ()
		{
			try {
				progress.SetMessage("Uploading. Please wait...");
				progress.Show();

				var progressDialog = ProgressDialog.Show(CallingActivity, "Please wait...", "Uploading Database..", true);
				new Thread(new ThreadStart(delegate
					{
						RunOnUiThread(() =>BackupDatabase());
						RunOnUiThread(() => progressDialog.Hide());
					})).Start();


			} catch (Exception ex) {
				Toast.MakeText (this, Resources.GetString (Resource.String.msg_faildowndb), ToastLength.Long).Show ();	
			}

		}

		public void BackupDatabase ()
		{
			string pathToDatabase = ((GlobalvarsApp)Application.Context).DATABASE_PATH;
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
				
				myWebClient.QueryString ["COMP"] = ((GlobalvarsApp)Application.Context).COMPANY_CODE;
				myWebClient.QueryString ["BRAN"] = ((GlobalvarsApp)Application.Context).BRANCH_CODE;
				myWebClient.QueryString ["USER"] = ((GlobalvarsApp)Application.Context).USERID_CODE;
				myWebClient.UploadProgressChanged += MyWebClient_UploadProgressChanged;
				myWebClient.UploadFileCompleted += MyWebClient_UploadFileCompleted;
				if (filename.ToLower().Contains(".zip"))
				{
					//upload zip db file and extract
					myWebClient.UploadFileAsync(new Uri(@"http://www.wincomcloud.com/UploadDb/uploadDbEx.aspx"), filename);
				}else{
					//upload db file
					myWebClient.UploadFileAsync (new Uri(@"http://www.wincomcloud.com/UploadDb/uploadDb.aspx"), filename);
				}

			} catch {

			}

		}	

		void MyWebClient_UploadFileCompleted (object sender, UploadFileCompletedEventArgs e)
		{
			if (e.Cancelled)
				filename = "";
			if (e.Error != null)
				filename = "";

			progress.Progress= 100;
			FinishDownload (filename);	
			try{
				File.Delete(filename);	
			}catch{
			}
		}

		void MyWebClient_UploadProgressChanged (object sender, UploadProgressChangedEventArgs e)
		{
			double bytesIn = double.Parse(e.BytesSent.ToString());
			double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
			double percentage = bytesIn / totalBytes * 100;
			progress.Progress = int.Parse (Math.Truncate (percentage).ToString ());
		}
	}
}

