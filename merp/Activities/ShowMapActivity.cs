using System;
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
using Android.Webkit;
using Java.Net;
using NVelocity.App;
using System.IO;
using NVelocity;
using Android.Views.InputMethods;

namespace wincom.mobile.erp
{
	[Activity (Label = "Show Map",Icon="@drawable/ic_location")]			
	public class ShowMapActivity : Activity
	{
		WebView wv1;
		string latlng;
		string pathToDatabase;

		Button btnBack;
		Button btnShow;
		EditText txtdate ;
		DateTime date ;
		private InputMethodManager imm;
		const int DATE_DIALOG_ID1 = 0;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			SetContentView (Resource.Layout.ShowLocation);
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			latlng= Intent.GetStringExtra ("location") ?? "";

			date =  DateTime.Today;
			wv1= FindViewById<WebView> (Resource.Id.webView1);
			btnBack =FindViewById<Button> (Resource.Id.butBack);
			btnShow =FindViewById<Button> (Resource.Id.butMaP);
			txtdate = FindViewById<EditText> (Resource.Id.date);
			wv1.SetWebViewClient (new myWebView ());

			imm = (InputMethodManager)GetSystemService(Context.InputMethodService);
			txtdate.Click += delegate(object sender, EventArgs e) {
				imm.HideSoftInputFromWindow(txtdate.WindowToken, 0);
				ShowDialog (DATE_DIALOG_ID1);
			};

			btnShow.Click+= (object sender, EventArgs e) => {
				ShowMap();	
			};
			btnBack.Click+= (object sender, EventArgs e) => {
				base.OnBackPressed();
			};
		
			// Create your application here

		}
		protected override void OnResume ()
		{
			base.OnResume ();
			latlng= Intent.GetStringExtra ("location") ?? "";
			ShowMap();

		}

		protected override Dialog OnCreateDialog (int id)
		{
			switch (id) {
			case DATE_DIALOG_ID1:
					return new DatePickerDialog (this, OnDateSet1, date.Year, date.Month - 1, date.Day); 
	
			}
			return null;
		}
		void OnDateSet1 (object sender, DatePickerDialog.DateSetEventArgs e)
		{
			EditText frd = FindViewById<EditText> (Resource.Id.trxdatefr);
			txtdate.Text = e.Date.ToString("dd-MM-yyyy");
		}

		void ShowMap()
		{
			List<GeoLocationModel> list = GetData ();
			if (list.Count == 0) {
				Toast.MakeText (this,"No location found...", ToastLength.Long).Show ();	
				return;
			}
			string furl = CreateTemplate (list);
			Uri uri = new Uri(furl);
			//string url = "http://maps.google.com/?q=" + URLEncoder.Encode (latlng);

			wv1.Settings.LoadsImagesAutomatically = true;
			wv1.Settings.JavaScriptEnabled = true;
			wv1.ScrollBarStyle = Android.Views.ScrollbarStyles.InsideOverlay;
			wv1.LoadUrl (uri.ToString());
		}

		List<GeoLocationModel> GetData()
		{
			DateTime sdate =DateTime.Today;
			DateTime edate;
			if (txtdate.Text!="")
			  sdate = Utility.ConvertToDate (txtdate.Text);
			edate = sdate.AddDays (1);
			List<GeoLocationModel> list= new System.Collections.Generic.List<GeoLocationModel>();
			using (var db = new SQLite.SQLiteConnection(pathToDatabase, true))
			{
				var geos = db.Table<GeoLocation> ().Where (x => x.Date >= sdate && x.Date <= edate).OrderBy (x => x.Date).ToList();
					foreach(var geo in geos){
					GeoLocationModel m = new GeoLocationModel ();
					m.Altitude = geo.Altitude;
					m.Date = geo.Date.ToString ("dd-MM-yy hh:mm tt");
					m.lat = geo.lat;
					m.lng = geo.lng;
					m.Desc= geo.Date.ToString ("dd-MM-yy hh:mm tt");
					list.Add (m);
					}
			}

			return list;
		}

	     string CreateTemplate(List<GeoLocationModel> list)
		{
			string path = Path.Combine (Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "erpdata");
			VelocityEngine fileEngine = new VelocityEngine();
			fileEngine.Init();
			string template;//= File.ReadAllText(@"d:\invoice.vm");
			using (Stream sr = Assets.Open(@"mylocation.html"))
			{
				using (MemoryStream ms = new MemoryStream())
				{
					sr.CopyTo(ms);
					byte[] data2 = ms.ToArray();
					template = Encoding.UTF8.GetString(ms.ToArray(), 0, data2.Length);
				}
			}
			VelocityContext context = new VelocityContext();

			// put the states and the transitions into the context
			StreamWriter ws = new StreamWriter(new MemoryStream());

			context.Put("util", new CustomTool());
			context.Put("model", list);

			fileEngine.Evaluate(context, ws, null, template);
			string text = "";
			ws.Flush();
			byte[] data = new byte[ws.BaseStream.Length];
			ws.BaseStream.Position = 0;
			int nread = ws.BaseStream.Read(data, 0, data.Length);
			text = Encoding.GetEncoding("GB18030").GetString(data, 0, nread);
			string tempfile = Path.Combine (path, "showmap.html");
			if (File.Exists(tempfile)){
				File.Delete(tempfile);
			}

			File.WriteAllBytes(tempfile, data);
			ws.Close();

			return tempfile;
		}

	}
}



