﻿
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
using Android.Views.InputMethods;

namespace wincom.mobile.erp
{
	[Activity (Label = "CREDIT NOTE ITEM ENTRY",Icon="@drawable/bill")]			
	public class CNEntryActivity : Activity,IEventListener
	{
		string pathToDatabase;
		List<Item> items = null;
		string EDITMODE ="";
		string CUSTOMER ="";
		string CUSTCODE ="";
		string ITEMUID ="";
		string INVOICENO="";
		string FIRSTLOAD="";
		Spinner spinner;
		ArrayAdapter<String> dataAdapter;
		double taxper;
		bool isInclusive;
		AccessRights rights;
		Trader trd;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!((GlobalvarsApp)this.Application).ISLOGON) {
				Finish ();
			}
			SetTitle (Resource.String.title_cnitementry);
			EventManagerFacade.Instance.GetEventManager().AddListener(this);

			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			rights = Utility.GetAccessRights (pathToDatabase);

			INVOICENO = Intent.GetStringExtra ("invoiceno") ?? "AUTO";
			ITEMUID = Intent.GetStringExtra ("itemuid") ?? "AUTO";
			EDITMODE = Intent.GetStringExtra ("editmode") ?? "AUTO";
			CUSTOMER= Intent.GetStringExtra ("customer") ?? "AUTO";
			CUSTCODE= Intent.GetStringExtra ("custcode") ?? "AUTO";
			trd = DataHelper.GetTrader (pathToDatabase, CUSTCODE);
			// Create your application here
			SetContentView (Resource.Layout.Entry);
			spinner = FindViewById<Spinner> (Resource.Id.txtcode);
			EditText qty = FindViewById<EditText> (Resource.Id.txtqty);
			EditText price = FindViewById<EditText> (Resource.Id.txtprice);
			TextView txtInvNo =  FindViewById<TextView> (Resource.Id.txtInvnp);
			TextView txtcust =  FindViewById<TextView> (Resource.Id.txtInvcust);
			Button butFind = FindViewById<Button> (Resource.Id.newinv_bfind);
			EditText txtbarcode = FindViewById<EditText> (Resource.Id.txtbarcode);
			TextView lbInvNo =  FindViewById<TextView> (Resource.Id.lbinvno);
			lbInvNo.Text = Resources.GetString( Resource.String.invform_cnno);
			txtbarcode.Visibility = ViewStates.Invisible;
			txtInvNo.Text = INVOICENO;
			txtcust.Text = CUSTOMER;
			Button but = FindViewById<Button> (Resource.Id.Save);
			Button but2 = FindViewById<Button> (Resource.Id.Cancel);
			spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs> (spinner_ItemSelected);
			but.Click += butSaveClick;
			but2.Click += butCancelClick;
			butFind.Click+= (object sender, EventArgs e) => {
				ShowItemLookUp();
			};
			qty.EditorAction += HandleEditorAction;
			qty.AfterTextChanged+= Qty_AfterTextChanged;
			price.EditorAction += HandleEditorAction; 

			price.Enabled = rights.CNEditUPrice;

			//SqliteConnection.CreateFile(pathToDatabase);
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				items = db.Table<Item> ().ToList<Item> ();
			}

			List<string> icodes = new List<string> ();
			foreach (Item item in items) {
				//icodes.Add (item.ICode+" | "+item.IDesc);
				if (item.IDesc.Length > 40) {
					icodes.Add (item.ICode + " | " + item.IDesc.Substring(0,40)+"...");
				}else icodes.Add (item.ICode + " | " + item.IDesc);
			}

			dataAdapter = new ArrayAdapter<String>(this,Resource.Layout.spinner_item, icodes);

			// Drop down layout style - list view with radio button
			//dataAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
			dataAdapter.SetDropDownViewResource(Resource.Layout.SimpleSpinnerDropDownItemEx);

			// attaching data adapter to spinner
			spinner.Adapter =dataAdapter;
			if (EDITMODE == "EDIT") {
				FIRSTLOAD="1";
				LoadData (INVOICENO, ITEMUID);
			}
		}
		void Qty_AfterTextChanged (object sender, Android.Text.AfterTextChangedEventArgs e)
		{
			CalAmt ();
		}

		private void HandleEditorAction(object sender, TextView.EditorActionEventArgs e)
		{
			e.Handled = false;
			if ((e.ActionId == ImeAction.Done)||(e.ActionId == ImeAction.Next))
			{
				CalAmt ();
				e.Handled = true;   
//				View view = sender as View;
//				InputMethodManager imm = (InputMethodManager)GetSystemService(Context.InputMethodService);
//				imm.HideSoftInputFromWindow(view.WindowToken, 0);
			}
		}

		void ShowKeyBoard(View view)
		{
			InputMethodManager imm = (InputMethodManager)GetSystemService(Context.InputMethodService);
			imm.ShowSoftInputFromInputMethod(view.WindowToken, ShowFlags.Forced);
			imm.ToggleSoftInput (ShowFlags.Forced, 0); 
		}

		private void CalAmt()
		{
			EditText ttlamt = FindViewById<EditText> (Resource.Id.txtamount);
			EditText ttltax = FindViewById<EditText> (Resource.Id.txttaxamt);
			EditText qty = FindViewById<EditText> (Resource.Id.txtqty);
			EditText price = FindViewById<EditText> (Resource.Id.txtprice);
			//EditText taxper = FindViewById<EditText> (Resource.Id.txtinvtaxper);
			//CheckBox isincl = FindViewById<CheckBox> (Resource.Id.txtinvisincl);
			TextView txttax =  FindViewById<TextView> (Resource.Id.txttax);
			try{
				double taxval = taxper;// Convert.ToDouble(taxper.Text);
			double stqQty = Convert.ToDouble(qty.Text);
			double uprice = Convert.ToDouble(price.Text);
			double amount = Math.Round((stqQty * uprice),2);
			double netamount = amount;
				bool taxinclusice =  isInclusive;// isincl.Checked;
			double taxamt = 0;
			if (taxinclusice) {
				double percent = (taxval/100) + 1;
				double amt2 =Math.Round( amount / percent,2,MidpointRounding.AwayFromZero);
				taxamt = amount - amt2;
				netamount = amount - taxamt;

			} else {
				taxamt = Math.Round(amount * (taxval / 100),2,MidpointRounding.AwayFromZero);
			}
			ttlamt.Text = netamount.ToString("n2");
			ttltax.Text = taxamt.ToString("n2");
			}catch{
			}
		}

		private void LoadData(string invno,string uid)
		{
			TextView txtInvNo =  FindViewById<TextView> (Resource.Id.txtInvnp);
			Spinner spinner = FindViewById<Spinner> (Resource.Id.txtcode);
			EditText qty = FindViewById<EditText> (Resource.Id.txtqty);
			//TextView desc =  FindViewById<TextView> (Resource.Id.txtdesc);
			EditText price = FindViewById<EditText> (Resource.Id.txtprice);
			EditText amount = FindViewById<EditText> (Resource.Id.txtamount);
			//EditText taxper = FindViewById<EditText> (Resource.Id.txtinvtaxper);
			EditText taxamt = FindViewById<EditText> (Resource.Id.txttaxamt);
			//CheckBox isincl = FindViewById<CheckBox> (Resource.Id.txtinvisincl);
			TextView tax =  FindViewById<TextView> (Resource.Id.txttax);

			int id = Convert.ToInt32 (uid);

			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var invlist =db.Table<CNNoteDtls> ().Where (x => x.cnno == invno&& x.ID==id).ToList<CNNoteDtls> ();
				if (invlist.Count > 0) {
					CNNoteDtls invItem = invlist [0];
					//int index = dataAdapter.GetPosition (invItem.icode + " | " + invItem.description);
					int index = -1;
					if (invItem.description.Length > 40)
						index = dataAdapter.GetPosition (invItem.icode + " | " + invItem.description.Substring (0, 40) + "...");
					else
						index = dataAdapter.GetPosition (invItem.icode + " | " + invItem.description);
					
					Item item =items.Where (x => x.ICode == invItem.icode).FirstOrDefault ();
					spinner.SetSelection (index);
					qty.Text = invItem.qty.ToString ();
					price.Text = invItem.price.ToString ();
					taxamt.Text = invItem.tax.ToString ();

					tax.Text = item.taxgrp;
					taxper = item.tax;
					isInclusive = item.isincludesive;
					//taxper.Text = item.tax.ToString ();
				//	isincl.Checked = item.isincludesive;
					amount.Text = invItem.amount.ToString ();
					price.Text = invItem.price.ToString ();
				}
			}

		}

		private void butSaveClick(object sender,EventArgs e)
		{
			TextView txtInvNo =  FindViewById<TextView> (Resource.Id.txtInvnp);
			Spinner spinner = FindViewById<Spinner> (Resource.Id.txtcode);
			EditText qty = FindViewById<EditText> (Resource.Id.txtqty);
		//	TextView desc =  FindViewById<TextView> (Resource.Id.txtdesc);
			EditText price = FindViewById<EditText> (Resource.Id.txtprice);
			//EditText taxper = FindViewById<EditText> (Resource.Id.txtinvtaxper);
			//CheckBox isincl = FindViewById<CheckBox> (Resource.Id.txtinvisincl);
			TextView txttax =  FindViewById<TextView> (Resource.Id.txttax);
			EditText ttlamt = FindViewById<EditText> (Resource.Id.txtamount);
			EditText ttltax = FindViewById<EditText> (Resource.Id.txttaxamt);
			if (spinner.SelectedItem == null) {
				Toast.MakeText (this,Resources.GetString(Resource.String.msg_invaliditem), ToastLength.Long).Show ();			
				spinner.RequestFocus ();
				return;			
			}

			if (string.IsNullOrEmpty(qty.Text)) {
				Toast.MakeText (this, Resources.GetString(Resource.String.msg_invalidqty), ToastLength.Long).Show ();			
				qty.RequestFocus ();
				return;
			}
			if (string.IsNullOrEmpty(price.Text)) {
				Toast.MakeText (this, Resources.GetString(Resource.String.msg_invalidprice), ToastLength.Long).Show ();			
				price.RequestFocus ();
				return;
			}
			double stqQty = Convert.ToDouble(qty.Text);
			double uprice = Convert.ToDouble(price.Text);
			double taxval = taxper;//Convert.ToDouble(taxper.Text);
			double amount = Math.Round((stqQty * uprice),2);
			double netamount = amount;
			bool taxinclusice = isInclusive;// isincl.Checked;
			double taxamt = 0;
			if (taxinclusice) {
				double percent = (taxval/100) + 1;
				double amt2 =Math.Round( amount / percent,2,MidpointRounding.AwayFromZero);
				taxamt = amount - amt2;
				netamount = amount - taxamt;
			
			} else {
				taxamt = Math.Round(amount * (taxval / 100),2,MidpointRounding.AwayFromZero);
			}

			CNNoteDtls inv = new CNNoteDtls ();
			string[] codedesc = spinner.SelectedItem.ToString ().Split (new char[]{ '|' });
			inv.cnno = txtInvNo.Text;
			inv.amount = amount;
			//inv.description = codedesc [1].Trim();
			inv.icode = codedesc [0].Trim();// spinner.SelectedItem.ToString ();
			inv.price = uprice;
			inv.qty = stqQty;
			inv.tax = taxamt;
			inv.taxgrp = txttax.Text;
			inv.netamount = netamount;

			var itemlist = items.Where (x => x.ICode == inv.icode).ToList<Item> ();
			if (itemlist.Count == 0) {
				Toast.MakeText (this, Resources.GetString(Resource.String.msg_invaliditem), ToastLength.Long).Show ();
				return;
			}
			Item ItemCode = itemlist [0];
			inv.description = ItemCode.IDesc;
			int id = Convert.ToInt32 (ITEMUID);				
			//inv..title = spinner.SelectedItem.ToString ();
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var invlist =db.Table<CNNoteDtls> ().Where (x => x.cnno == inv.cnno&& x.ID==id).ToList<CNNoteDtls> ();
				if (invlist.Count > 0) {
					CNNoteDtls invItem = invlist [0];
					invItem.amount = amount;
					invItem.netamount = netamount;
					invItem.tax = taxamt;
					invItem.taxgrp = txttax.Text;
					//invItem.description =  codedesc [1].Trim();
					invItem.description = ItemCode.IDesc;
					invItem.icode =  codedesc [0].Trim(); //spinner.SelectedItem.ToString ();
					invItem.price = uprice;
					invItem.qty = stqQty;
					db.Update (invItem);
				}else db.Insert (inv);
			}

		//spinner.SetSelection (-1);
			qty.Text = "";
			//price.Text = "";
			ttltax.Text = "";
			ttlamt.Text = "";
			Toast.MakeText (this, Resources.GetString(Resource.String.msg_itemadded), ToastLength.Long).Show ();
		}
		public override void OnBackPressed() {
			// do nothing.
		}

		private void butCancelClick(object sender,EventArgs e)
		{
			string invno = Intent.GetStringExtra ("invoiceno") ?? "AUTO";
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var itemlist = db.Table<CNNoteDtls> ().Where (x => x.cnno == invno);	
				double ttlamt= itemlist.Sum (x => x.netamount);
				double ttltax= itemlist.Sum (x => x.tax);
				var invlist =db.Table<CNNote> ().Where (x => x.cnno == invno).ToList<CNNote> ();
				if (invlist.Count > 0) {
					invlist [0].amount = ttlamt;
					invlist [0].taxamt = ttltax;
					db.Update (invlist [0]);
				}
			}
			//base.OnBackPressed();
			//var intent = new Intent(this, typeof(CNItemActivity)); // need to change
			var intent =ActivityManager.GetActivity<CNItemActivity>(this.ApplicationContext);
			intent.PutExtra ("invoiceno",INVOICENO );
			intent.PutExtra ("custcode",CUSTCODE );
			intent.PutExtra ("editmode",EDITMODE );
			StartActivity(intent);
		}
		private void spinner_ItemSelected (object sender, AdapterView.ItemSelectedEventArgs e)
		{
			Spinner spinner = (Spinner)sender;

			string []codedesc = spinner.GetItemAtPosition (e.Position).ToString().Split (new char[]{ '|' });
			string icode = codedesc[0].Trim();
			Item item =items.Where (x => x.ICode == icode).FirstOrDefault ();
			TextView tax =  FindViewById<TextView> (Resource.Id.txttax);
			EditText price = FindViewById<EditText> (Resource.Id.txtprice);

			EditText qty = FindViewById<EditText> (Resource.Id.txtqty);
			if (FIRSTLOAD == "") {
				double uprice= Utility.GetUnitPrice (trd, item);
				price.Text = uprice.ToString ();
			}
			else FIRSTLOAD="";
			tax.Text = item.taxgrp;
			taxper = item.tax;
			isInclusive = item.isincludesive;
			qty.RequestFocus ();
			//ShowKeyBoard (qty);

		}

		void ShowItemLookUp()
		{
			var dialog = ItemDialog.NewInstance();
			dialog.Show(FragmentManager, "dialog");
		}

		void SetSelectedItem(string text)
		{
			string[] selected = text.Split(new char[]{'|'});
			if (selected.Length <= 1)
				return;

			string temp = "";
			if (selected [1].Trim ().Length > 40) {
				temp = selected [0].Trim () + " | " + selected [1].Trim ().Substring (0, 40) + "...";
			} else
				temp = text;
			
			int position=dataAdapter.GetPosition (temp);
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
			case 102:
				RunOnUiThread (() => SetSelectedItem(e.Param["SELECTED"].ToString()));
				break;
			}
		}
	}
}

