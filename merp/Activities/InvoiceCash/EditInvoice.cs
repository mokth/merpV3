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
	[Activity (Label = "EDIT INVOICE",Icon="@drawable/shop")]			
	public class EditInvoice : Activity,IEventListener
	{
		string pathToDatabase;
		List<Trader> custs = null;
		ArrayAdapter<String> dataAdapter;
		ArrayAdapter dataAdapter2;
		DateTime _date ;
		AdPara apara = null;
		Spinner spinner;
		Spinner spinnerType ;
		string INVOICENO="";
		Invoice invInfo;
		EditText ccType ;
		EditText ccNo;
		AccessRights rights;
		int CashPos=-1;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!((GlobalvarsApp)this.Application).ISLOGON) {
				Finish ();
			}
			SetTitle (Resource.String.title_invoiceedit);
			SetContentView (Resource.Layout.CreateInvoice);
			EventManagerFacade.Instance.GetEventManager().AddListener(this);
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			rights = Utility.GetAccessRights (pathToDatabase);

			INVOICENO = Intent.GetStringExtra ("invoiceno") ?? "AUTO";
			invInfo =DataHelper.GetInvoice (pathToDatabase, INVOICENO);
			if (invInfo == null) {
				base.OnBackPressed ();
			}
			// Create your application here

			spinner = FindViewById<Spinner> (Resource.Id.newinv_custcode);
			spinnerType = FindViewById<Spinner> (Resource.Id.newinv_type);
			Button butSave = FindViewById<Button> (Resource.Id.newinv_bsave);
			butSave.Text = Resources.GetString(Resource.String.but_save);// "SAVE";
			Button butCancel = FindViewById<Button> (Resource.Id.newinv_cancel);
			Button butFind = FindViewById<Button> (Resource.Id.newinv_bfind);
			spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs> (spinner_ItemSelected);

			butSave.Click += butSaveClick;
			butCancel.Click += butCancelClick;
			spinnerType.Enabled = false;

			EditText trxdate =  FindViewById<EditText> (Resource.Id.newinv_date);
//			trxdate.Click += delegate(object sender, EventArgs e) {
//				ShowDialog (0);
//			};
			//19-Nov-2015
			//disable the date, for edit mode, date should not be editable
			//cuase running number issue.
			trxdate.Enabled = false;

			butFind.Click+= (object sender, EventArgs e) => {
				ShowCustLookUp();
			};

		
			apara =  DataHelper.GetAdPara (pathToDatabase);
			LoadTrader ();

			List<string> icodes = new List<string> ();
			int counter = 0;
			foreach (Trader item in custs) {
				icodes.Add (item.CustCode+" | "+item.CustName.Trim());
				if (item.CustCode.Trim() == "COD" || item.CustCode.Trim() == "CASH") {
					CashPos = counter;
				}
				counter += 1;
			}

			dataAdapter = new ArrayAdapter<String> (this, Resource.Layout.spinner_item, icodes);
			dataAdapter2 =ArrayAdapter.CreateFromResource (
				this, Resource.Array.trxtype, Resource.Layout.spinner_item);
		
			//dataAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
			dataAdapter.SetDropDownViewResource(Resource.Layout.SimpleSpinnerDropDownItemEx);
			dataAdapter2.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);

			// attaching data adapter to spinner
			spinner.Adapter =dataAdapter;
			spinnerType.Adapter =dataAdapter2;
			LoadData ();
		}

		void LoadTrader ()
		{
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				custs = db.Table<Trader> ().ToList();
			}
		}

		public override void OnBackPressed() {
			// do nothing.
		}

		private void LoadData()
		{
			EditText trxdate =  FindViewById<EditText> (Resource.Id.newinv_date);
			DateTime invdate = Utility.ConvertToDate (trxdate.Text);
			DateTime tmr = invdate.AddDays (1);
			Spinner spinner = FindViewById<Spinner> (Resource.Id.newinv_custcode);
			//Spinner spinner2 = FindViewById<Spinner> (Resource.Id.newinv_type);
			TextView txtinvno =FindViewById<TextView> (Resource.Id.newinv_no);
			EditText remark = FindViewById<EditText> (Resource.Id.newinv_remark);
			EditText txtcustname = FindViewById<EditText> (Resource.Id.newinv_custname);

			trxdate.Text = invInfo.invdate.ToString ("dd-MM-yyyy");

			if (invInfo.custcode == "COD" || invInfo.custcode == "CASH") {
				spinner.SetSelection (CashPos);
			} else {
				int pos1= dataAdapter.GetPosition (invInfo.custcode+" | "+invInfo.description);
				spinner.SetSelection (pos1);
			}

			//int pos2 = invInfo.trxtype == "CASH" ? 0 : 1;
			//int pos2= dataAdapter2.GetPosition (invInfo.trxtype);

			//spinnerType.SetSelection(pos2);
			remark.Text = invInfo.remark;
			txtinvno.Text = invInfo.invno;
			txtcustname.Text = invInfo.description;


		}

		private void butSaveClick(object sender,EventArgs e)
		{

			if (SaveInvoice()){
				base.OnBackPressed();	
			}
		}

		private void butCancelClick(object sender,EventArgs e)
		{
			base.OnBackPressed();
		}

		private void spinner_ItemSelected (object sender, AdapterView.ItemSelectedEventArgs e)
		{
			Spinner spinner = (Spinner)sender;
			EditText txtcustname = FindViewById<EditText> (Resource.Id.newinv_custname);
			string txt = spinner.GetItemAtPosition (e.Position).ToString();
			string[] codes = txt.Split (new char[]{ '|' });
			if (codes.Length == 0)
				return;

			Trader item =custs.Where (x => x.CustCode ==codes[0].Trim()).FirstOrDefault ();
			if (item != null) {

				if (codes [0].Trim () == "COD" || codes [0].Trim () == "CASH") {
					txtcustname.Enabled = true;
				} else {
					txtcustname.Enabled = false;
					txtcustname.Text = codes [1].Trim ();
				}


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


		private bool SaveInvoice()
		{
			bool lSave = false;
			Invoice inv = new Invoice ();
			EditText trxdate =  FindViewById<EditText> (Resource.Id.newinv_date);
			if (!Utility.IsValidDateString (trxdate.Text)) {
				Toast.MakeText (this,Resources.GetString(Resource.String.msg_invaliddate), ToastLength.Long).Show ();	
				return lSave;
			}
			DateTime invdate = Utility.ConvertToDate (trxdate.Text);
			Spinner spinner = FindViewById<Spinner> (Resource.Id.newinv_custcode);
			Spinner spinner2 = FindViewById<Spinner> (Resource.Id.newinv_type);
			TextView txtinvno =FindViewById<TextView> (Resource.Id.newinv_no);
			EditText txtcustname = FindViewById<EditText> (Resource.Id.newinv_custname);
			EditText remark = FindViewById<EditText> (Resource.Id.newinv_remark);

			if (spinner.SelectedItem == null) {
				Toast.MakeText (this, Resources.GetString(Resource.String.msg_invalidcust), ToastLength.Long).Show ();			
				spinner.RequestFocus ();
				return lSave;			
			}

			string[] codes = spinner.SelectedItem.ToString().Split (new char[]{ '|' });
			if (codes.Length == 0)
				return lSave;

			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {

				invInfo.invdate = invdate;
				invInfo.trxtype = spinner2.SelectedItem.ToString ();
				invInfo.created = DateTime.Now;
				invInfo.description = codes [1].Trim ();
				invInfo.remark = remark.Text.ToUpper ();
				//inv.amount = 0;
				invInfo.custcode = codes [0].Trim ();
				invInfo.isUploaded = false;

				if (codes [0].Trim () == "COD" || codes [0].Trim () == "CASH") {
					inv.description = txtcustname.Text.ToUpper ();
				}
			
				db.Update (invInfo);
				lSave = true;
			}

			return lSave;
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

