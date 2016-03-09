
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
	[Activity (Label = "NEW SALES ORDER",Icon="@drawable/sales")]			
	public class CreateSaleOrder_WT : Activity,IEventListener
	{
		string pathToDatabase;
		List<Trader> items = null;
		ArrayAdapter<String> dataAdapter;
		ArrayAdapter<String> dataAdapter2;
		DateTime _date ;
		AdPara apara = null;
		Spinner spinner;
		Spinner spinnerBill;


		//AccessRights rights;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!((GlobalvarsApp)this.Application).ISLOGON) {
				Finish ();
			}
			SetTitle (Resource.String.title_sonew);
			SetContentView (Resource.Layout.CreateSO);
			EventManagerFacade.Instance.GetEventManager().AddListener(this);

			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			//rights = Utility.GetAccessRights (pathToDatabase);
			apara =  DataHelper.GetAdPara (pathToDatabase);

			// Create your application here
			_date = DateTime.Today;
			spinner = FindViewById<Spinner> (Resource.Id.newinv_custcode);
			spinnerBill = FindViewById<Spinner> (Resource.Id.newinv_billto);
			TextView lbBillto = FindViewById<TextView> (Resource.Id.lbbillto);
			lbBillto.Text = "PAY FROM";

			Button butSave = FindViewById<Button> (Resource.Id.newinv_bsave);
			Button butNew = FindViewById<Button> (Resource.Id.newinv_cancel);
			Button butFind = FindViewById<Button> (Resource.Id.newinv_bfind);
			Button butBillFind = FindViewById<Button> (Resource.Id.newinv_billfind);
			//spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs> (spinner_ItemSelected);
			//spinnerBill.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs> (spinner_ItemSelected);
			butSave.Click += butSaveClick;
			butNew.Click += butCancelClick;
			TextView invno =  FindViewById<TextView> (Resource.Id.newinv_no);
			invno.Text = "AUTO";
			EditText trxdate =  FindViewById<EditText> (Resource.Id.newinv_date);
 			trxdate.Text = _date.ToString ("dd-MM-yyyy");
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

			var icodes = GetCustCode (IsValidCustTo);
			var icodes2 = GetCustCode (IsValidBillTo);

			dataAdapter = new ArrayAdapter<String> (this, Resource.Layout.spinner_item, icodes);
			dataAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
			spinner.Adapter =dataAdapter;

			dataAdapter2 = new ArrayAdapter<String> (this, Resource.Layout.spinner_item, icodes2);
			dataAdapter2.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
			spinnerBill.Adapter =dataAdapter2;


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
			int count1 = 0;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				count1 = db.Table<Item>().Count ();
			}
			if (count1 > 0)
				CreateNewSO ();
			else {
				Toast.MakeText (this,Resources.GetString(Resource.String.msg_noitem), ToastLength.Long).Show ();	
			}
		}

		private void butCancelClick(object sender,EventArgs e)
		{
			base.OnBackPressed();
		}

//		private void spinner_ItemSelected (object sender, AdapterView.ItemSelectedEventArgs e)
//		{
//			Spinner spinner = (Spinner)sender;
//
//			string txt = spinner.GetItemAtPosition (e.Position).ToString();
//			string[] codes = txt.Split (new char[]{ '|' });
//			if (codes.Length == 0)
//				return;
//			
//			Trader item =items.Where (x => x.CustCode ==codes[0].Trim()).FirstOrDefault ();
//			if (item != null) {
//				TextView name = FindViewById<TextView> (Resource.Id.newinv_custname);
//				name.Text = item.CustName;
//
//			}
//
//		}
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


		void ShowItemEntry (SaleOrder so, string[] codes)
		{
			//var intent = new Intent (this, typeof(SOEntryActivity));
			var intent =ActivityManager.GetActivity<SOEntryActivityEx>(this.ApplicationContext);
			intent.PutExtra ("invoiceno", so.sono);
			intent.PutExtra ("customer", codes [1].Trim ());
			intent.PutExtra ("custcode",codes [0].Trim ());
			intent.PutExtra ("itemuid", "-1");
			intent.PutExtra ("editmode", "NEW");
			StartActivity (intent);
		}

		private void CreateNewSO()
		{
			SaleOrder so = new SaleOrder ();
			EditText trxdate =  FindViewById<EditText> (Resource.Id.newinv_date);
			if (!Utility.IsValidDateString (trxdate.Text)) {
				Toast.MakeText (this,Resources.GetString(Resource.String.msg_invaliddate), ToastLength.Long).Show ();	
				return;
			}
			DateTime sodate = Utility.ConvertToDate (trxdate.Text);
			DateTime tmr = sodate.AddDays (1);
			AdNumDate adNum= DataHelper.GetNumDate(pathToDatabase, sodate,"SO");
			Spinner spinner = FindViewById<Spinner> (Resource.Id.newinv_custcode);
			TextView txtinvno =FindViewById<TextView> (Resource.Id.newinv_no);
			TextView custname = FindViewById<TextView> (Resource.Id.newinv_custname);
			EditText custpo =  FindViewById<EditText> (Resource.Id.newinv_po);
			EditText remark =  FindViewById<EditText> (Resource.Id.newinv_remark);
			string prefix = apara.SOPrefix.Trim ().ToUpper ();
			if (spinner.SelectedItem == null) {
				Toast.MakeText (this,  Resources.GetString(Resource.String.msg_invalidcust), ToastLength.Long).Show ();			
				spinner.RequestFocus ();
				return;			
			}

			string[] codes = spinner.SelectedItem.ToString().Split (new char[]{ '|' });
			if (codes.Length == 0)
				return;

			string[] codes2 = spinnerBill.SelectedItem.ToString().Split (new char[]{ '|' });
			string billTo = "";
			if (codes.Length > 0) {
				billTo = codes2 [0].Trim ();
			}
			
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				string sono = "";
				int runno = adNum.RunNo + 1;
				int currentRunNo =DataHelper.GetLastSORunNo(pathToDatabase, sodate);
				if (currentRunNo >= runno)
					runno = currentRunNo + 1;
				
				sono =prefix + sodate.ToString("yyMM") + runno.ToString().PadLeft (4, '0');

				txtinvno.Text= sono;
				so.sodate = sodate;
				so.trxtype ="SO" ;
				so.created = DateTime.Now;
				so.sono = sono;
				so.billTo = billTo;
				so.remark = remark.Text.ToUpper();
				so.custpono = custpo.Text.ToUpper();
				so.amount = 0;
				so.custcode = codes [0].Trim ();
				so.description = codes [1].Trim ();
				so.isUploaded = false;

				txtinvno.Text = sono;
				db.Insert (so);
				adNum.RunNo = runno;
				if (adNum.ID >= 0)
					db.Update (adNum);
				else
					db.Insert (adNum);
			}

			ShowItemEntry (so, codes);
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

