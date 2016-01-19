using System;
using System.IO;
using System.Net;
using Android.App;

namespace wincom.mobile.erp
{
	public class UploadDbHelper
	{
		public static void BackupDatabase ()
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


		private  static void UploadToErpHostForSupport(string filename)
		{
			WebClient myWebClient = new WebClient ();
			try {

				myWebClient.QueryString ["COMP"] = ((GlobalvarsApp)Application.Context).COMPANY_CODE;
				myWebClient.QueryString ["BRAN"] = ((GlobalvarsApp)Application.Context).BRANCH_CODE;
				myWebClient.QueryString ["USER"] = ((GlobalvarsApp)Application.Context).USERID_CODE;
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
			finally{
				try{
				
					File.Delete (filename);
				}catch
				{}
			}
		}	
	}
}

