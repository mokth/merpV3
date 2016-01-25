using System;
using System.Linq;
using WcfServiceItem;
using System.Collections.Generic;
using Android.App;
using SQLite;
using Android.Widget;

namespace wincom.mobile.erp
{
	public class UploadLocHelper
	{
		Service1Client _client;
		WCFHelper _wfc = new WCFHelper();
		volatile List<MapLocation> locs = new List<MapLocation> ();
		volatile string _errmsg;
		volatile int invcount =0;
		public OnUploadDoneDlg Uploadhandle;
		public Application CallingActivity=null;

		public void startUpload()
		{
			invcount =0;
			_errmsg = "";
			_client = _wfc.GetServiceClient ();	
			if (_client != null) {
				_client.UpdateLocationCompleted+= ClientOnUploadLocationCompleted;
				UploadlocsToServer ();
			}
		}

		public void SetUploadHandel(OnUploadDoneDlg uploadhandle)
		{
			Uploadhandle = uploadhandle;
		}

		public void SetCallingActivity(Application callingActivity)
		{
			CallingActivity = callingActivity;
		}

		private void UploadlocsToServer()
		{
			string pathToDatabase = ((GlobalvarsApp)CallingActivity).DATABASE_PATH;
			AdUser user = new AdUser ();
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				user = db.Table<AdUser> ().FirstOrDefault ();
			}
			if (user == null)
				return;
			
			string comp =user.CompCode;
			string brn = user.BranchCode;
			string userid = user.UserID;

			locs = Getlocs();

			invcount += locs.Count;
			if (locs.Count > 0) {
				_client.UpdateLocationAsync (locs.ToArray (), comp, brn, userid);
			}
//			else {
//				RunOnUiThread (() => Uploadhandle.Invoke(CallingActivity,invcount,_errmsg));
//			}
		}


		public void UpdateUploadStat()
		{
			string pathToDatabase = ((GlobalvarsApp)CallingActivity).DATABASE_PATH;
			DateTime now = DateTime.Now;
			try {
				using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
					var list1 = db.Table<GeoLocation> ().Where (x => x.isUploaded == false).ToList();
					List<GeoLocation> geolist= new List<GeoLocation>();
					foreach (MapLocation bill in locs) {
						var found = list1.Where(x=>x.ID==bill.UID).ToList();
						if (found.Count>0)
						{  
							found[0].isUploaded = true;

							geolist.Add(found[0]);
						}
					}	

					if (geolist.Count>0)
						db.UpdateAll (geolist);
					
					UploadlocsToServer ();
				}
			} catch (Exception ex) {
				_errmsg = "Update status Error." + ex.Message;
			}
		}

		public List<MapLocation> Getlocs()
		{
			string pathToDatabase = ((GlobalvarsApp)CallingActivity).DATABASE_PATH;

			locs = new List<MapLocation> ();
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list1 = db.Table<GeoLocation> ().Where(x=>x.isUploaded==false)
					.OrderBy(x=>x.Date)
					.Take(30)
					.ToList<GeoLocation> ();
			
				foreach (GeoLocation loc in list1) {
					MapLocation bill = new MapLocation ();
					bill.UID = loc.ID;
					bill.Altitude = loc.Altitude;
					bill.Date = loc.Date;
					bill.Heading = loc.Heading;
					bill.Lat = loc.lat;
					bill.Lng = loc.lng;

					locs.Add (bill);
				}
			}

			return locs;
		}

		public void ClientOnUploadLocationCompleted(object sender, UpdateLocationCompletedEventArgs e)
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

			//if (!success)
			//	RunOnUiThread (() => Uploadhandle.Invoke(CallingActivity,0,_errmsg));
			

		}

	}
}

