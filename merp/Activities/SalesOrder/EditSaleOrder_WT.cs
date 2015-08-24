
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
	[Activity (Label = "EDIT SALES ORDER",Icon="@drawable/sales")]			
	public class EditSaleOrder_WT : Activity,IEventListener
	{
		string pathToDatabase;
		List<Trader> items = null;
		ArrayAdapter<String> dataAdapter;
		ArrayAdapter<String> dataAdapter2;
		DateTime _date ;
		AdPara apara = null;
		Spinner spinner;
		Spinner spinnerBill;
		string SONO;
		SaleOrder soInfo;
		//AccessRights rights;
		List<string> icodes ; 
		List<string> icodes2;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!((GlobalvarsApp)this.Application).ISLOGON) {
				Finish ();
			}
			SetTitle (Resource.String.title_soedit);
			SetContentView (Resource.Layout.CreateSO);
			EventManagerFacade.Instance.GetEventManager().AddListener(this);
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			SONO = Intent.GetStringExtra ("sono") ?? "AUTO";
			soInfo =DataHelper.GetSO(pathToDatabase, SONO);
			if (soInfo == null) {
				base.OnBackPressed ();
			}

			//rights = Utility.GetAccessRights (pathToDatabase);
			apara =  DataHelper.GetAdPara (pathToDatabase);

			// Create your application here
			_date = DateTime.Today;
			spinner = FindViewById<Spinner> (Resource.Id.newinv_custcode);
			spinnerBill = FindViewById<Spinner> (Resource.Id.newinv_billto);
			TextView lbBillto = FindViewById<TextView> (Resource.Id.lbbillto);
			lbBillto.Text = "PAY FOR";

			Button butSave = FindViewById<Button> (Resource.Id.newinv_bsave);
			butSave.Text = Resources.GetString(Resource.String.but_save);// "SAVE";
			Button butNew = FindViewById<Button> (Resource.Id.newinv_cancel);
			Button butFind = FindViewById<Button> (Resource.Id.newinv_bfind);
			Button butBillFind = FindViewById<Button> (Resource.Id.newinv_billfind);
			//spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs> (spinner_ItemSelected);
			//spinnerBill.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs> (spinner_ItemSelected);
			butSave.Click += butSaveClick;
			butNew.Click += butCancelClick;
			EditText trxdate =  FindViewById<EditText> (Resource.Id.newinv_date);
 			//trxdate.Text = _date.ToString ("dd-MM-yyyy");
			trxdate.Click += delegate(object sender, EventArgs e) {
				ShowDialog (0);
			};
			butFind.Click+= (object sender, EventArgs e) => {
				ShowCustLookUp();
			};
			butBillFind.Click+= (object sender, EventArgs e) => {
				ShowBillToLookUp();
			};


			//SqliteConnection.CreateFile(pathToDatabase);
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				items = db.Table<Trader> ().ToList<Trader> ();
			}

			icodes = GetCustCode (IsValidCustTo);
			icodes2 = GetCustCode (IsValidBillTo);

			dataAdapter = new ArrayAdapter<String> (this, Resource.Layout.spinner_item, icodes);
			dataAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
			spinner.Adapter =dataAdapter;

			dataAdapter2 = new ArrayAdapter<String> (this, Resource.Layout.spinner_item, icodes2);
			dataAdapter2.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
			spinnerBill.Adapter =dataAdapter2;

			LoadData ();
		}

		private void LoadData()
		{
			EditText trxdate =  FindViewById<EditText> (Resource.Id.newinv_date);
			DateTime invdate = Utility.ConvertToDate (trxdate.Text);
			DateTime tmr = invdate.AddDays (1);
			Spinner spinner = FindViewById<Spinner> (Resource.Id.newinv_custcode);
			Spinner spinner2 = FindViewById<Spinner> (Resource.Id.newinv_type);
			TextView txtinvno =FindViewById<TextView> (Resource.Id.newinv_no);
			TextView custname = FindViewById<TextView> (Resource.Id.newinv_custname);
			EditText custpo =  FindViewById<EditText> (Resource.Id.newinv_po);
			EditText remark =  FindViewById<EditText> (Resource.Id.newinv_remark);

			trxdate.Text = soInfo.sodate.ToString ("dd-MM-yyyy");
			var found1 =icodes.Where (x => x.IndexOf (soInfo.custcode + " | ") > -1).ToList ();
			var found2 =icodes2.Where (x => x.IndexOf (soInfo.billTo + " | ") > -1).ToList ();

			if (found1.Count > 0) {
				int pos= dataAdapter.GetPosition (found1[0]);
				spinner.SetSelection (pos);
			}
			if (found2.Count > 0) {
				int pos= dataAdapter2.GetPosition (found2[0]);
				spinnerBill.SetSelection (pos);
			}

			txtinvno.Text = soInfo.sono;
			custpo.Text = soInfo.custpono;
			remark.Text = soInfo.remark;

		}

		private List<string> GetCustCode(IsValidTraderFilter filter)
		{
			List<string> icodes = new List<string> ();
			foreach (Trader item in items) {
				if (filter.Invoke (item)) {
					icodes.Add (item.CustCode + " | " + item.CustName);
				}
			}
			return icodes;
		}

		public override void OnBackPressed() {
			// do nothing.
		}

		private void butSaveClick(object sender,EventArgs e)
		{
			if (SaveSO()){
				base.OnBackPressed();	
			}
		}

		private void butCancelClick(object sender,EventArgs e)
		{
			base.OnBackPressed();
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

		private bool SaveSO()
		{
			bool lSave = false;
			EditText trxdate =  FindViewById<EditText> (Resource.Id.newinv_date);
			if (!Utility.IsValidDateString (trxdate.Text)) {
				Toast.MakeText (this,Resources.GetString(Resource.String.msg_invaliddate), ToastLength.Long).Show ();	
				return false;
			}
			DateTime sodate = Utility.ConvertToDate (trxdate.Text);
			 Spinner spinner = FindViewById<Spinner> (Resource.Id.newinv_custcode);
			TextView txtinvno =FindViewById<TextView> (Resource.Id.newinv_no);
			TextView custname = FindViewById<TextView> (Resource.Id.newinv_custname);
			EditText custpo =  FindViewById<EditText> (Resource.Id.newinv_po);
			EditText remark =  FindViewById<EditText> (Resource.Id.newinv_remark);

			if (spinner.SelectedItem == null) {
				Toast.MakeText (this,Resources.GetString(Resource.String.msg_invalidcust), ToastLength.Long).Show ();			
				spinner.RequestFocus ();
				return lSave;			
			}

			string[] codes = spinner.SelectedItem.ToString().Split (new char[]{ '|' });
			if (codes.Length == 0)
				return lSave;

			string[] codes2 = spinnerBill.SelectedItem.ToString().Split (new char[]{ '|' });
			string billTo = "";
			if (codes.Length > 0) {
				billTo = codes2 [0].Trim ();
			}
			
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {

				soInfo.sodate = sodate;
				soInfo.trxtype ="SO" ;
				soInfo.created = DateTime.Now;
				soInfo.billTo = billTo;
				soInfo.remark = remark.Text.ToUpper();
				soInfo.custpono = custpo.Text.ToUpper();
				soInfo.custcode = codes [0].Trim ();
				soInfo.description = codes [1].Trim ();
				soInfo.isUploaded = false;

				db.Update (soInfo);
				lSave = true;
			}

			return lSave;
		}

		void ShowCustLookUp()
		{
			var dialog = TraderDialog.NewInstance(IsValidCustTo,EventID.CUSTCODE_SELECTED);
			dialog.Show(FragmentManager, "dialog");
		}

		void ShowBillToLookUp()
		{
			//IsValidTraderFilter 
			var dialog = TraderDialog.NewInstance(IsValidBillTo,EventID.CUSTCODE2_SELECTED);
			dialog.Show(FragmentManager, "dialog");
		}

		private bool IsValidBillTo(Trader trd)
		{
			return trd.CustType.ToUpper ()!= "BILLTO";
		}

		private bool IsValidCustTo(Trader trd)
		{
			return trd.CustType.ToUpper () != "BILLTO";
		}


		void SetSelectedItem(string text)
		{
			int position=dataAdapter.GetPosition (text);
			spinner.SetSelection (position);
		}

		void SetSelectedItem2(string text)
		{
			int position=dataAdapter2.GetPosition (text);
			spinnerBill.SetSelection (position);
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
			case EventID.CUSTCODE2_SELECTED:
				RunOnUiThread (() => SetSelectedItem2(e.Param["SELECTED"].ToString()));
				break;
			}
		}
	}
}

