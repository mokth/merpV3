using System;
using Android.App;
using Android.Runtime;
using System.IO;
using Plugin.CurrentActivity;
using Android.OS;
using Plugin.Geolocator.Abstractions;
using Plugin.Geolocator;
using System.Threading;
using Android.Content;

namespace wincom.mobile.erp
{
	[Application(Label = "M-ERP V5")]
	public class GlobalvarsApp:Application, Application.IActivityLifecycleCallbacks
	{
		public string COMPANY_CODE;
		public string BRANCH_CODE;
		public string USERID_CODE;
		public string DATABASE_PATH;
		public string USERFUNCTION;
		public string VERSION;
		public string ITEMCLASS;
		public bool ISLOGON;
		IGeolocator locator;
		AccessRights rights;

		public GlobalvarsApp(IntPtr handle, JniHandleOwnership transfer)
			: base(handle, transfer)
		{
		}
			
		public override void OnCreate()
		{
			string path = Path.Combine (Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "erpdata");
			if (!Directory.Exists (path))
				Directory.CreateDirectory (path);
			//var documents = System.Environment.GetFolderPath (System.Environment.GetFolderPath());
			//DATABASE_PATH = Path.Combine (path, "db_adonet.db");
			var documents = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
			DATABASE_PATH = Path.Combine (documents, "db_adonet.db");
			base.OnCreate();
			RegisterActivityLifecycleCallbacks(this);

			SetupGRPS ();
		}

		private void SetupGRPS()
		{
			if (!File.Exists (DATABASE_PATH))
				return;
			
			if (!IsGPSTrackingEnbale())
				return;
			
			GetMyLocation ();
			new Thread (new ThreadStart (delegate {	
					Uploadlocs ();
				})).Start ();

		}

		private bool IsGPSTrackingEnbale(){
		
			if (rights == null) {
				rights = Utility.GetAccessRights (DATABASE_PATH);	
			}
			Console.WriteLine ("rights.IsGPSTracking {0}",rights.IsGPSTracking);
			return rights.IsGPSTracking;
		}

		public override void OnTerminate()
		{
			base.OnTerminate();
			UnregisterActivityLifecycleCallbacks(this);
			Console.WriteLine ("OnTerminate()");
		}

		public void OnActivityCreated(Activity activity, Bundle savedInstanceState)
		{
			CrossCurrentActivity.Current.Activity = activity;
		}

		public void OnActivityDestroyed(Activity activity)
		{
		}

		public void OnActivityPaused(Activity activity)
		{
		}

		public void OnActivityResumed(Activity activity)
		{
			CrossCurrentActivity.Current.Activity = activity;
		}

		public void OnActivitySaveInstanceState(Activity activity, Bundle outState)
		{
		}

		public void OnActivityStarted(Activity activity)
		{
		    CrossCurrentActivity.Current.Activity = activity;
     	}

		public void OnActivityStopped(Activity activity)
		{
		}

		private void GetMyLocation()
		{
			Console.WriteLine ("Start MyLocation");
			locator = CrossGeolocator.Current;
			if (!locator.IsGeolocationEnabled) {
				enableGPS ();
			}
			locator.DesiredAccuracy = 20;
			locator.AllowsBackgroundUpdates = true;
			locator.PositionChanged+= Locator_PositionChanged;
			locator.PositionError+= Locator_PositionError;
			locator.StartListeningAsync(300000, 100, false); //5 min , 100 meter
			Console.WriteLine ("IsGeolocationAvailable {0}",locator.IsGeolocationAvailable);
			Console.WriteLine ("IsGeolocationEnabled {0}",locator.IsGeolocationEnabled);
			Console.WriteLine ("IsListening {0}",locator.IsListening);
		}

		void Locator_PositionError (object sender, Plugin.Geolocator.Abstractions.PositionErrorEventArgs e)
		{
			Console.WriteLine (e.Error);
		}

		void Locator_PositionChanged (object sender, Plugin.Geolocator.Abstractions.PositionEventArgs e)
		{
			Console.WriteLine (e.Position.Latitude.ToString () + "   " + e.Position.Longitude.ToString ());
			try {
				using (var db = new SQLite.SQLiteConnection (DATABASE_PATH)) {
							
					GeoLocation geo = new GeoLocation ();
					geo.Date = DateTime.Now;
					geo.BranchCode = BRANCH_CODE;
					geo.CompCode = COMPANY_CODE;
					geo.UserID = USERID_CODE;
					geo.lat = e.Position.Latitude;
					geo.lng = e.Position.Longitude;
					geo.Altitude = e.Position.Altitude;
					geo.Heading = e.Position.Heading;
					db.Insert (geo);
				}
			} catch {
			}
		}

		void Uploadlocs()
		{
			Console.WriteLine ("Upload loc");
			try {
				UploadLocHelper upload = new UploadLocHelper ();
				//upload.SetUploadHandel(OnUploadDoneDlg); 
				upload.SetCallingActivity (this);
				upload.startUpload ();		
			} catch (Exception ex) {
				Console.WriteLine (ex);
			}
		}

		void enableGPS()
		{
			Intent gpsOptionsIntent = new Intent(Android.Provider.Settings.ActionLocationSourceSettings);  
			gpsOptionsIntent.SetFlags(ActivityFlags.NewTask);
			StartActivity(gpsOptionsIntent);
		}

		public void EnableGPSTracking(AccessRights rights)
		{
			Console.WriteLine ("EnableGPSTracking rights.IsGPSTracking {0}",rights.IsGPSTracking);
			if (rights.IsGPSTracking) {
				locator = CrossGeolocator.Current;
				if (locator.IsGeolocationAvailable) {
					if (!locator.IsListening) {
						GetMyLocation ();
					}
				}
			}
		}

	}
}

