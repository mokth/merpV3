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
using Android.Bluetooth;
using Android.Graphics;

namespace wincom.mobile.erp
{
	[Activity (Label = "SETTINGS",Icon="@drawable/settings")]			
	public class SettingActivity : Activity
	{
		string pathToDatabase;
		Spinner spinner;
		Spinner spinBt;
		ArrayAdapter adapter;
		ArrayAdapter adapterBT;
		BluetoothAdapter mBluetoothAdapter;
		List<string> btdevices = new List<string>();
		AccessRights rights;
		TextView lbInvPrefix;
		TextView lbCashPrefix;
		TextView lbCNPrefix;
		TextView lbSOPrefix;
		TextView lbDOPrefix;
		EditText txtInvPrefix;
		EditText txtCashPrefix;
		EditText txtCNPrefix;
		EditText txtSOPrefix;
		EditText txtDOPrefix;


		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!((GlobalvarsApp)this.Application).ISLOGON) {
				Finish ();
			}
			SetTitle (Resource.String.mainmenu_settings);
			SetContentView (Resource.Layout.AdPara);
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			rights = Utility.GetAccessRights (pathToDatabase);

			spinner = FindViewById<Spinner> (Resource.Id.txtSize);
			spinBt= FindViewById<Spinner> (Resource.Id.txtprinters);
			Button butSave = FindViewById<Button> (Resource.Id.ad_bSave);
			Button butCancel = FindViewById<Button> (Resource.Id.ad_Cancel);
			FindControls ();

			butSave.Click += butSaveClick;
			butCancel.Click += butCancelClick;
			findBTPrinter ();
			//RunOnUiThread(()=>{ findBTPrinter ();});

			adapter = ArrayAdapter.CreateFromResource (this, Resource.Array.papersize_array, Android.Resource.Layout.SimpleSpinnerItem);
			adapter.SetDropDownViewResource (Android.Resource.Layout.SimpleSpinnerDropDownItem);

			spinner.Adapter = adapter;
			spinner.ItemSelected+= Spinner_ItemSelected;
			spinBt.ItemSelected+= Spinner_ItemSelected;
			LoadData ();
			// Create your application here
		}

		void FindControls ()
		{
			lbInvPrefix = FindViewById<TextView> (Resource.Id.lbInvPrefix);
			lbCashPrefix = FindViewById<TextView> (Resource.Id.lbCashPrefix);
			lbCNPrefix = FindViewById<TextView> (Resource.Id.lbCNPrefix);
			lbSOPrefix = FindViewById<TextView> (Resource.Id.lbSOPrefix);
			lbDOPrefix = FindViewById<TextView> (Resource.Id.lbDOPrefix);
			txtInvPrefix = FindViewById<EditText> (Resource.Id.txtInvPrefix);
			txtCashPrefix = FindViewById<EditText> (Resource.Id.txtCashPrefix);
			txtCNPrefix = FindViewById<EditText> (Resource.Id.txtCNPrefix);
			txtSOPrefix = FindViewById<EditText> (Resource.Id.txtSOPrefix);
			txtDOPrefix = FindViewById<EditText> (Resource.Id.txtDOPrefix);

			if (!rights.IsSOModule) {
				lbSOPrefix.Visibility = ViewStates.Gone;
				txtSOPrefix.Visibility = ViewStates.Gone;

			}
			if (!rights.IsCNModule) {
				lbCNPrefix.Visibility = ViewStates.Gone;
				txtCNPrefix.Visibility = ViewStates.Gone;
			}
			if (!rights.IsDOModule) {
				lbDOPrefix.Visibility = ViewStates.Gone;
				txtDOPrefix.Visibility = ViewStates.Gone;
			}
		}

		void SpinBt_ItemClick (object sender,  AdapterView.ItemSelectedEventArgs e)
		{
			string name = adapterBT.GetItem (e.Position).ToString ();
			TextView txtprinter =FindViewById<TextView> (Resource.Id.txtad_printer);
			txtprinter.Text = name;
		}

		void Spinner_ItemSelected (object sender, AdapterView.ItemSelectedEventArgs e)
		{
			((TextView)((Spinner)sender).SelectedView).SetTextColor (Color.Black);

		}

		private void LoadData()
		{
			TextView txtprinter =FindViewById<TextView> (Resource.Id.txtad_printer);

			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			AdPara apra = new AdPara ();
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				var list  = db.Table<AdPara> ().ToList<AdPara> ();
				if (list.Count > 0) {
					apra = list [0];
					string[] prefixs = apra.Prefix.Split (new char[]{ '|' });
					txtInvPrefix.Text = prefixs[0];
					if (prefixs.Length>1)
						txtCashPrefix.Text = prefixs [1];
					else txtCashPrefix.Text = prefixs [0];
					if (rights.IsSOModule) {
						txtSOPrefix.Text = apra.SOPrefix;
					}
					if (rights.IsCNModule) {
						txtCNPrefix.Text = apra.CNPrefix;
					}
					if (rights.IsDOModule) {
						txtDOPrefix.Text = apra.DOPrefix;
					}
					txtprinter.Text = apra.PrinterName;
					int prnpos = adapterBT.GetPosition (apra.PrinterName);
					if (prnpos>0)
						spinBt.SetSelection (prnpos);
					int position=adapter.GetPosition (apra.PaperSize);
					if (position>0)
						spinner.SetSelection (position);
					else spinner.SetSelection (0);
				} else {
					
					txtprinter.Text = "PT-II";

				}
			}
		}
		private void butSaveClick(object sender,EventArgs e)
		{
			TextView txtprinter =FindViewById<TextView> (Resource.Id.txtad_printer);

			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			AdPara apra = new AdPara ();

			apra.PrinterName = txtprinter.Text.ToUpper();
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				var list  = db.Table<AdPara> ().ToList<AdPara> ();
				if (list.Count == 0) {
					db.Insert (apra);		
				} else {
					apra = list [0];
					apra.PrinterName = txtprinter.Text.ToUpper();
					apra.PaperSize = spinner.SelectedItem.ToString ();
					db.Update (apra);
				}
			}
			base.OnBackPressed();
		}

		private void butCancelClick(object sender,EventArgs e)
		{
			base.OnBackPressed();
		}

		private void findBTPrinter(){
			btdevices.Clear ();
		 try{
				mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;
				if (mBluetoothAdapter==null)
				{
					Toast.MakeText (this, "Error initialize bluetooth Adapter. Try again", ToastLength.Long).Show ();
					return;					
				}
				if (!mBluetoothAdapter.Enable()) {
					Intent enableBluetooth = new Intent(
						BluetoothAdapter.ActionRequestEnable);
					StartActivityForResult(enableBluetooth, 0);
				}


				var pair= mBluetoothAdapter.BondedDevices;
				if (pair.Count > 0) {
					foreach (BluetoothDevice dev in pair) {
						btdevices.Add(dev.Name);
					}
				}

				adapterBT = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem, btdevices.ToArray());
				adapterBT.SetDropDownViewResource (Android.Resource.Layout.SimpleSpinnerDropDownItem);
				spinBt.Adapter = adapterBT;
				spinBt.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs> ( SpinBt_ItemClick );
				//txtv.Text ="found device " +mmDevice.Name;
			}catch(Exception ex) {
				
				Toast.MakeText (this, "Error initialize bluetooth Adapter. Try again", ToastLength.Long).Show ();
			}

		}
	}
}

