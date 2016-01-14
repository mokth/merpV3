
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
using System.IO;

namespace wincom.mobile.erp
{
	[Activity (Label = "NEW DELIVERY ORDER",Icon="@drawable/delivery")]			
	public class CreateDelOrder : Activity,IEventListener
	{
		string pathToDatabase;
		List<Trader> items = null;
		ArrayAdapter<String> dataAdapter;
		ArrayAdapter dataAdapter2;
		DateTime _date ;
		AdPara apara = null;
		Spinner spinner;
		string compcode;
		AccessRights rights;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!((GlobalvarsApp)this.Application).ISLOGON) {
				Finish ();
			}
			SetTitle (Resource.String.title_donew);
			SetContentView (Resource.Layout.CreateDO);
			EventManagerFacade.Instance.GetEventManager().AddListener(this);
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			compcode = ((GlobalvarsApp)this.Application).COMPANY_CODE;
			apara =  DataHelper.GetAdPara (pathToDatabase);
			rights = Utility.GetAccessRights (pathToDatabase);

			// Create your application here
			_date = DateTime.Today;
			spinner = FindViewById<Spinner> (Resource.Id.newinv_custcode);
			Spinner spinnerType = FindViewById<Spinner> (Resource.Id.newinv_type);
			Button butSave = FindViewById<Button> (Resource.Id.newinv_bsave);
			Button butNew = FindViewById<Button> (Resource.Id.newinv_cancel);
			Button butFind = FindViewById<Button> (Resource.Id.newinv_bfind);
			EditText remark = FindViewById<EditText> (Resource.Id.newinv_custname);
			spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs> (spinner_ItemSelected);
			butSave.Click += butSaveClick;
			butNew.Click += butCancelClick;
			TextView invno =  FindViewById<TextView> (Resource.Id.newinv_no);
			invno.Text = "AUTO";
			EditText trxdate =  FindViewById<EditText> (Resource.Id.newinv_date);
 			trxdate.Text = _date.ToString ("dd-MM-yyyy");
//			trxdate.Click += delegate(object sender, EventArgs e) {
//				ShowDialog (0);
//			};
			if (rights.DOEditTrxDate) {
				trxdate.Click += delegate(object sender, EventArgs e) {
					ShowDialog (0);
				};
			} else
				trxdate.Enabled = false;
			butFind.Click+= (object sender, EventArgs e) => {
				ShowCustLookUp();
			};


			//SqliteConnection.CreateFile(pathToDatabase);
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				items = db.Table<Trader> ().ToList<Trader> ();
			}

			List<string> icodes = new List<string> ();
			foreach (Trader item in items) {
				icodes.Add (item.CustCode+" | "+item.CustName);
			}

			dataAdapter2 =ArrayAdapter.CreateFromResource (
				this, Resource.Array.term, Resource.Layout.spinner_item);
			

			dataAdapter = new ArrayAdapter<String> (this, Resource.Layout.spinner_item, icodes);
			dataAdapter.SetDropDownViewResource(Resource.Layout.SimpleSpinnerDropDownItemEx);
			dataAdapter2.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
			spinner.Adapter =dataAdapter;
			spinnerType.Adapter =dataAdapter2;

			remark.RequestFocus ();

		}

		public override void OnBackPressed() {
			// do nothing.
		}

		private void butSaveClick(object sender,EventArgs e)
		{
			if (DataHelper.IsUploadExpired (rights, pathToDatabase)) {
				Toast.MakeText (this,Resources.GetString(Resource.String.msg_mustupload), ToastLength.Long).Show ();	
				return;
			}

			int count1 = 0;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				count1 = db.Table<Item>().Count ();
			}
			if (count1 > 0)
				CreateNewDO ();
			else {
				Toast.MakeText (this,"Please download Master Item before proceed... ", ToastLength.Long).Show ();	
			}
		}

		private void butCancelClick(object sender,EventArgs e)
		{
			base.OnBackPressed();
		}

		private void spinner_ItemSelected (object sender, AdapterView.ItemSelectedEventArgs e)
		{
			Spinner spinner = (Spinner)sender;

			string txt = spinner.GetItemAtPosition (e.Position).ToString();
			string[] codes = txt.Split (new char[]{ '|' });
			if (codes.Length == 0)
				return;
			
			Trader item =items.Where (x => x.CustCode ==codes[0].Trim()).FirstOrDefault ();
			if (item != null) {
				//TextView name = FindViewById<TextView> (Resource.Id.newinv_custname);
				//name.Text = item.CustName;

			}

		}
		[Obsolete]
		protected override Dialog OnCreateDialog (int id)
		{
			return new DatePickerDialog (this, HandleDateSet, _date.Year,
				_date.Month - 1, _date.Day);
		}

		void HandleDateSet (object sender, DatePickerDialog.DateSetEventArgs e)
		{
			_date = e.Date;
			EditText trxdate =  FindViewById<EditText> (Resource.Id.newinv_date);
			trxdate.Text = _date.ToString ("dd-MM-yyyy");
		}


		void ShowItemEntry (DelOrder so, string[] codes)
		{
			//var intent = new Intent (this, typeof(DOEntryActivity));
			var intent =ActivityManager.GetActivity<DOEntryActivity>(this.ApplicationContext);
			intent.PutExtra ("invoiceno", so.dono);
			intent.PutExtra ("customer", codes [1].Trim ());
			intent.PutExtra ("itemuid", "-1");
			intent.PutExtra ("editmode", "NEW");
			StartActivity (intent);
		}

		private void CreateNewDO()
		{
			DelOrder dorder = new DelOrder ();
			EditText trxdate =  FindViewById<EditText> (Resource.Id.newinv_date);
			if (!Utility.IsValidDateString (trxdate.Text)) {
				Toast.MakeText (this,Resources.GetString(Resource.String.msg_invaliddate), ToastLength.Long).Show ();	
				return;
			}
			DateTime dodate = Utility.ConvertToDate (trxdate.Text);
			DateTime tmr = dodate.AddDays (1);
			AdNumDate adNum= DataHelper.GetNumDate(pathToDatabase, dodate,"DO");
			Spinner spinner = FindViewById<Spinner> (Resource.Id.newinv_custcode);
			Spinner spinner2 = FindViewById<Spinner> (Resource.Id.newinv_type);
			TextView txtinvno =FindViewById<TextView> (Resource.Id.newinv_no);
			EditText remark = FindViewById<EditText> (Resource.Id.newinv_custname);
			//EditText custpo =  FindViewById<EditText> (Resource.Id.newinv_po);
			//EditText remark =  FindViewById<EditText> (Resource.Id.newinv_remark);
			string prefix = apara.DOPrefix.Trim ().ToUpper ();
			if (spinner.SelectedItem == null) {
				Toast.MakeText (this, Resources.GetString(Resource.String.msg_invalidcust), ToastLength.Long).Show ();			
				spinner.RequestFocus ();
				return;			
			}


			string[] codes = spinner.SelectedItem.ToString().Split (new char[]{ '|' });
			if (codes.Length == 0)
				return;
			
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				string dono = "";
				int runno = adNum.RunNo + 1;
				int currentRunNo =DataHelper.GetLastDORunNo(pathToDatabase, dodate);
				if (currentRunNo >= runno)
					runno = currentRunNo + 1;
				
				dono =prefix + dodate.ToString("yyMM") + runno.ToString().PadLeft (4, '0');

				txtinvno.Text= dono;
				dorder.dodate = dodate;
				dorder.trxtype ="DO" ;
				dorder.created = DateTime.Now;
				dorder.dono = dono;
				dorder.description =  codes [1].Trim ();
				dorder.remark = remark.Text;
				dorder.term =  spinner2.SelectedItem.ToString ();
				dorder.amount = 0;
				dorder.custcode = codes [0].Trim ();
				dorder.isUploaded = false;

				txtinvno.Text = dono;
				db.Insert (dorder);
				adNum.RunNo = runno;
				if (adNum.ID >= 0)
					db.Update (adNum);
				else
					db.Insert (adNum);
			}

			ShowItemEntry (dorder, codes);
		}

		void ShowCustLookUp()
		{
			var dialog = TraderDialog.NewInstance();
			dialog.Show(FragmentManager, "dialog");
		}

		void SetSelectedItem(string text)
		{
			int position=dataAdapter.GetPosition (text);
			spinner.SetSelection (position);
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
			case EventID.CUSTCODE_SELECTED:
				RunOnUiThread (() => SetSelectedItem(e.Param["SELECTED"].ToString()));
				break;
			}
		}
	}
}

