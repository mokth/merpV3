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
	[Activity (Label = "NEW CASH BILL",Icon="@drawable/shop")]			
	public class CreateCashBill : Activity,IEventListener
	{
		string pathToDatabase;
		string compcode;
		List<Trader> items = null;
		ArrayAdapter<String> dataAdapter;
		ArrayAdapter dataAdapter2;
		DateTime _date ;
		AdPara apara = null;
		Spinner spinner;
		AccessRights rights;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!((GlobalvarsApp)this.Application).ISLOGON) {
				Finish ();
			}
			this.Window.SetSoftInputMode (SoftInput.StateAlwaysHidden);
			SetTitle (Resource.String.title_cashnew);
			SetContentView (Resource.Layout.CreateInvoice);
			EventManagerFacade.Instance.GetEventManager().AddListener(this);

			_date = DateTime.Today;
		

			// Create your application here

			spinner = FindViewById<Spinner> (Resource.Id.newinv_custcode);
			Spinner spinnerType = FindViewById<Spinner> (Resource.Id.newinv_type);
			Button butSave = FindViewById<Button> (Resource.Id.newinv_bsave);
			Button butNew = FindViewById<Button> (Resource.Id.newinv_cancel);
			Button butFind = FindViewById<Button> (Resource.Id.newinv_bfind);
			//spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs> (spinner_ItemSelected);
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

			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			compcode = ((GlobalvarsApp)this.Application).COMPANY_CODE;
			apara =  DataHelper.GetAdPara (pathToDatabase);
			rights = Utility.GetAccessRights (pathToDatabase);
			//SqliteConnection.CreateFile(pathToDatabase);
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				items = db.Table<Trader> ().ToList<Trader> ();
			}

			List<string> icodes = new List<string> ();
			foreach (Trader item in items) {
				icodes.Add (item.CustCode+" | "+item.CustName);
			}

			dataAdapter = new ArrayAdapter<String> (this, Resource.Layout.spinner_item, icodes);


			dataAdapter2 =ArrayAdapter.CreateFromResource (
							this, Resource.Array.trxtype, Resource.Layout.spinner_item);

			// Drop down layout style - list view with radio button
			//dataAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
			dataAdapter.SetDropDownViewResource(Resource.Layout.SimpleSpinnerDropDownItemEx);
			dataAdapter2.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);

			// attaching data adapter to spinner
			spinner.Adapter =dataAdapter;
			spinnerType.Adapter =dataAdapter2;
			EditText remark = FindViewById<EditText> (Resource.Id.newinv_custname);
			remark.RequestFocus ();
			spinnerType.SetSelection (0);
			spinnerType.Enabled = false;

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
				CreateNewCashBill ();
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
//			EditText txtcustname = FindViewById<EditText> (Resource.Id.newinv_custname);
//			string txt = spinner.GetItemAtPosition (e.Position).ToString();
//			string[] codes = txt.Split (new char[]{ '|' });
//			if (codes.Length == 0)
//				return;
//			
//			Trader item =items.Where (x => x.CustCode ==codes[0].Trim()).FirstOrDefault ();
//			if (item != null) {
//				//TextView name = FindViewById<TextView> (Resource.Id.newinv_custname);
//				//name.Text = item.CustName;
//				Spinner spinnerType = FindViewById<Spinner> (Resource.Id.newinv_type);
//				int pos = -1;
//				string paycode = item.PayCode.ToUpper ().Trim ();
//				if (!string.IsNullOrEmpty (paycode)) {
//					if (paycode.Contains ("CASH")|| paycode.Contains ("COD")) {
//						pos = dataAdapter2.GetPosition ("CASH");
//					} else {
//						pos = dataAdapter2.GetPosition ("INVOICE");
//					}
//				} else
//					pos = dataAdapter2.GetPosition ("CASH");
//
//
//				if (pos > -1) {
//					spinnerType.SetSelection (pos);
//					if (!rights.InvEditTrxType) {
//						spinnerType.Enabled = false;
//					}
//				}//else spinnerType.Enabled = true;
//
//				if (codes [0].Trim () == "COD" || codes [0].Trim () == "CASH") {
//					txtcustname.Enabled = true;
//				}else txtcustname.Enabled =false;
//
//				txtcustname.Text = codes [1].Trim ();
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


		void ShowItemEntry (Invoice inv, string[] codes)
		{
			string COMPCODE = ((GlobalvarsApp)this.Application).COMPANY_CODE;
			var intent =ActivityManager.GetActivity<EntryActivity>(this.ApplicationContext);
			//var intent = new Intent (this, typeof(EntryActivity));

			intent.PutExtra ("trxtype", inv.trxtype);
			intent.PutExtra ("invoiceno", inv.invno);
			intent.PutExtra ("customer", codes [1].Trim ());
			intent.PutExtra ("custcode",codes [0].Trim ());
			intent.PutExtra ("itemuid", "-1");
			intent.PutExtra ("editmode", "NEW");
			StartActivity (intent);
		}


		private void CreateNewCashBill()
		{
			Invoice inv = new Invoice ();
			EditText trxdate =  FindViewById<EditText> (Resource.Id.newinv_date);
			if (!Utility.IsValidDateString (trxdate.Text)) {
				Toast.MakeText (this,Resources.GetString(Resource.String.msg_invaliddate), ToastLength.Long).Show ();	
				return;
			}
			DateTime invdate = Utility.ConvertToDate (trxdate.Text);
			DateTime tmr = invdate.AddDays (1);
			AdNumDate adNum;// = DataHelper.GetNumDate (pathToDatabase, invdate);
			Spinner spinner = FindViewById<Spinner> (Resource.Id.newinv_custcode);
			//Spinner spinner2 = FindViewById<Spinner> (Resource.Id.newinv_type);

			TextView txtinvno =FindViewById<TextView> (Resource.Id.newinv_no);
			EditText remark = FindViewById<EditText> (Resource.Id.newinv_remark);
			EditText custname = FindViewById<EditText> (Resource.Id.newinv_custname);
			string[] prefixs = apara.Prefix.Trim ().ToUpper ().Split(new char[]{'|'});
			string prefix = "";
			string trxtype = "CASH";
			adNum = DataHelper.GetNumDate (pathToDatabase, invdate, "CS");
			if (prefixs.Length > 1) {
				prefix = prefixs [1];
			}else prefix = prefixs [0];



			if (spinner.SelectedItem == null) {
				Toast.MakeText (this, Resources.GetString(Resource.String.msg_invalidcust), ToastLength.Long).Show ();			
				spinner.RequestFocus ();
				return;			
			}

			string[] codes = spinner.SelectedItem.ToString().Split (new char[]{ '|' });
			if (codes.Length == 0)
				return;
			
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {

				string invno = "";
				int runno = adNum.RunNo + 1;
				int currentRunNo =DataHelper.GetLastInvRunNo (pathToDatabase, invdate,"CASH");

				if (currentRunNo >= runno)
					runno = currentRunNo + 1;
				
				invno =prefix + invdate.ToString("yyMM") + runno.ToString().PadLeft (4, '0');

				txtinvno.Text= invno;
				inv.invdate = invdate;
				inv.trxtype = "CASH";
				inv.created = DateTime.Now;
				inv.invno = invno;
				inv.description = codes [1].Trim ();//custname.Text;
				inv.amount = 0;
				inv.custcode = codes [0].Trim ();
				inv.isUploaded = false;
				inv.remark = remark.Text.ToUpper();
				if (codes [0].Trim () == "COD" || codes [0].Trim () == "CASH") {
					inv.description = custname.Text.ToUpper ();
				}
				txtinvno.Text = invno;
				db.Insert (inv);
				adNum.RunNo = runno;
				if (adNum.ID >= 0)
					db.Update (adNum);
				else
					db.Insert (adNum);
			}

			ShowItemEntry (inv, codes);
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

