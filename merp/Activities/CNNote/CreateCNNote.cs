﻿using System;
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
	[Activity (Label = "NEW CREDIT NOTE",Icon="@drawable/bill")]				
	public class CreateCNNote : Activity,IEventListener
	{
		string pathToDatabase;
		List<Trader> items = null;
		ArrayAdapter<String> dataAdapter;
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
			SetTitle (Resource.String.title_creditnotenew);
			SetContentView (Resource.Layout.CreateCNote);
			EventManagerFacade.Instance.GetEventManager().AddListener(this);
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			compcode = ((GlobalvarsApp)this.Application).COMPANY_CODE;
			apara =  DataHelper.GetAdPara (pathToDatabase);
			rights = Utility.GetAccessRights (pathToDatabase);

			// Create your application here
			_date = DateTime.Today;
			spinner = FindViewById<Spinner> (Resource.Id.newinv_custcode);

			Button butSave = FindViewById<Button> (Resource.Id.newinv_bsave);
			Button butNew = FindViewById<Button> (Resource.Id.newinv_cancel);
			Button butFind = FindViewById<Button> (Resource.Id.newinv_bfind);
			Button butFindInv = FindViewById<Button> (Resource.Id.newinv_bfindinv);
			spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs> (spinner_ItemSelected);
			butSave.Click += butSaveClick;
			butNew.Click += butCancelClick;
			TextView cnno =  FindViewById<TextView> (Resource.Id.newinv_no);
			cnno.Text = "AUTO";
			EditText trxdate =  FindViewById<EditText> (Resource.Id.newinv_date);
 			trxdate.Text = _date.ToString ("dd-MM-yyyy");
//			trxdate.Click += delegate(object sender, EventArgs e) {
//				ShowDialog (0);
//			};
			if (rights.CNEditTrxDate) {
				trxdate.Click += delegate(object sender, EventArgs e) {
					ShowDialog (0);
				};
			} else
				trxdate.Enabled = false;

			butFind.Click+= (object sender, EventArgs e) => {
				ShowCustLookUp();
			};

			butFindInv.Click+= (object sender, EventArgs e) => {
				ShowInvLookUp();
			};

			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			apara =  DataHelper.GetAdPara (pathToDatabase);
			//SqliteConnection.CreateFile(pathToDatabase);
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				items = db.Table<Trader> ().ToList<Trader> ();
			}

			List<string> icodes = new List<string> ();
			foreach (Trader item in items) {
				icodes.Add (item.CustCode+" | "+item.CustName.Trim());
			}
			dataAdapter = new ArrayAdapter<String>(this,Resource.Layout.spinner_item, icodes);
			// Drop down layout style - list view with radio button
			//dataAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
			dataAdapter.SetDropDownViewResource(Resource.Layout.SimpleSpinnerDropDownItemEx);
			// attaching data adapter to spinner
			spinner.Adapter =dataAdapter;

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
				CreateNewCN ();
			else {
				Toast.MakeText (this,Resources.GetString(Resource.String.msg_noitem), ToastLength.Long).Show ();	
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
//			
//			Trader item =items.Where (x => x.CustCode ==codes[0].Trim()).FirstOrDefault ();
//			if (item != null) {
//				TextView name = FindViewById<TextView> (Resource.Id.newinv_custname);
//				name.Text = item.CustName;
//			}

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


		void ShowItemEntry (CNNote inv, string[] codes)
		{
			//var intent = new Intent (this, typeof(CNEntryActivity)); //need to change
			var intent =ActivityManager.GetActivity<CNEntryActivityEx>(this.ApplicationContext);
			intent.PutExtra ("invoiceno", inv.cnno);
			intent.PutExtra ("customer", codes [1].Trim ());
			intent.PutExtra ("custcode",codes [0].Trim ());
			intent.PutExtra ("itemuid", "-1");
			intent.PutExtra ("editmode", "NEW");
			StartActivity (intent);
		}

		private void CreateNewCN()
		{
			CNNote inv = new CNNote ();
			EditText trxdate =  FindViewById<EditText> (Resource.Id.newinv_date);
			if (!Utility.IsValidDateString (trxdate.Text)) {
				Toast.MakeText (this,Resources.GetString(Resource.String.msg_invaliddate), ToastLength.Long).Show ();	
				return;
			}
			DateTime invdate = Utility.ConvertToDate (trxdate.Text);
			DateTime tmr = invdate.AddDays (1);
			AdNumDate adNum= DataHelper.GetNumDate (pathToDatabase, invdate,"CN");
			Spinner spinner = FindViewById<Spinner> (Resource.Id.newinv_custcode);
			TextView txtinvno =FindViewById<TextView> (Resource.Id.newinv_no);
			EditText remark = FindViewById<EditText> (Resource.Id.newinv_custname);
			TextView cninvno =  FindViewById<TextView> (Resource.Id.newcninv_no);
			string prefix = apara.CNPrefix.Trim ().ToUpper ();
			if (spinner.SelectedItem == null) {
				Toast.MakeText (this, Resources.GetString(Resource.String.msg_invalidcust), ToastLength.Long).Show ();				
				spinner.RequestFocus ();
				return;			
			}

			string[] codes = spinner.SelectedItem.ToString().Split (new char[]{ '|' });
			if (codes.Length == 0)
				return;
			Invoice invInfo = DataHelper.GetInvoice (pathToDatabase, cninvno.Text);
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				string invno = "";
				int runno = adNum.RunNo + 1;
				int currentRunNo =DataHelper.GetLastCNRunNo (pathToDatabase, invdate);
				if (currentRunNo >= runno)
					runno = currentRunNo + 1;
				if (invInfo != null) {
					//string[] prefixs = apara.Prefix.Trim ().ToUpper ().Split(new char[]{'|'});
					if (invInfo.trxtype == "CASH") {
						invno =prefix+ invdate.ToString ("yyMM") + runno.ToString ().PadLeft (4, '0')+"(CS)";
					}else  invno =prefix+ invdate.ToString ("yyMM") + runno.ToString ().PadLeft (4, '0')+"(INV)";
				}else	invno =prefix + invdate.ToString("yyMM") + runno.ToString().PadLeft (4, '0')+"(INV)";;
				txtinvno.Text= invno;
				inv.invdate = invdate;
				inv.trxtype = "";
				inv.created = DateTime.Now;
				inv.cnno = invno;
				inv.description = codes [1].Trim ();
				inv.amount = 0;
				inv.custcode = codes [0].Trim ();
				inv.isUploaded = false;
				inv.remark = remark.Text.ToUpper();
				inv.invno = cninvno.Text;
				if (invInfo != null) {
					inv.trxtype = invInfo.trxtype;	   		
				} else {
					Trader trd=	DataHelper.GetTrader (pathToDatabase, inv.custcode);
					if (trd != null) {
						string paycode =trd.PayCode.ToUpper ().Trim ();
						inv.trxtype = paycode;
						if (paycode.Contains ("CASH") || paycode.Contains ("COD")) {
							inv.trxtype = "CASH";	   		
						}
					}						
				}

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

		void ShowInvLookUp()
		{
			var dialog = InvoiceDialog.NewInstance();
			dialog.Show(FragmentManager, "dialog");
		}

		void SetSelectedItem(string text)
		{
			int position=dataAdapter.GetPosition (text);
			spinner.SetSelection (position);
		}

		void SetSelectedInvoice(string text)
		{
			TextView invno =  FindViewById<TextView> (Resource.Id.newcninv_no);
			invno.Text = text;
			Invoice inv =DataHelper.GetInvoice (pathToDatabase, text);
			int pos = dataAdapter.GetPosition (inv.custcode+" | "+inv.description);
			if (pos > -1) {
				spinner.SetSelection (pos);
				spinner.Enabled = false;
				Button butFind = FindViewById<Button> (Resource.Id.newinv_bfind);
				butFind.Enabled = false;
			}
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
			case EventID.CUSTCODE_SELECTED :
				RunOnUiThread (() => SetSelectedItem(e.Param["SELECTED"].ToString()));
				break;
			case EventID.INVNO_SELECTED:
				RunOnUiThread (() => SetSelectedInvoice(e.Param["SELECTED"].ToString()));
				break;
			}
		}
	}
}

