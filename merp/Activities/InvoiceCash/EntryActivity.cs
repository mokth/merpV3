
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
	
	
	[Activity (Label = "INVOICE ITEM ENTRY",Icon="@drawable/shop")]			
	public class EntryActivity : Activity,IEventListener,Android.Views.View.IOnKeyListener
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
		Trader  trd;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!((GlobalvarsApp)this.Application).ISLOGON) {
				Finish ();
			}
			SetTitle (Resource.String.title_invitementry);
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

			price.Enabled = rights.InvEditUPrice;

			txtbarcode.SetOnKeyListener (this);


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
		
		#region IOnKeyListener implementation
		public bool OnKey (View v, Keycode keyCode, KeyEvent e)
		{
			EditText barcode = (EditText)v;
			if (keyCode == Keycode.Enter||e.KeyCode== Keycode.NumpadEnter) {
				Txtbarcode_AfterTextChanged (barcode);
				return true;
			}

			//barcode.Text = barcode.Text + keyCode;
			return false;
		}
		#endregion
//		#region IDisposable implementation
//		public void Dispose ()
//		{
//
//		}
//		#endregion
//		#region IJavaObject implementation
//		public IntPtr Handle {
//			get {
//				
//			}
//		}
//		#endregion


		void Txtbarcode_AfterTextChanged (EditText txtbarcode)
		{			
			//EditText txtbarcode = FindViewById<EditText> (Resource.Id.txtbarcode);
			if (string.IsNullOrEmpty (txtbarcode.Text)) {
				txtbarcode.RequestFocus ();
				return;
			}
			
			var found= items.Where(x=>x.Barcode == txtbarcode.Text).ToList();
			if (found.Count == 0) {
				txtbarcode.Text = "";
				return;
			}
			var item = found [0];
			AddBarCodeItem (item);
			txtbarcode.Text = "";
			txtbarcode.RequestFocus ();
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
				var invlist =db.Table<InvoiceDtls> ().Where (x => x.invno == invno&& x.ID==id).ToList<InvoiceDtls> ();
				if (invlist.Count > 0) {
					InvoiceDtls invItem = invlist [0];
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
			EditText price = FindViewById<EditText> (Resource.Id.txtprice);
			TextView txttax =  FindViewById<TextView> (Resource.Id.txttax);
			EditText ttlamt = FindViewById<EditText> (Resource.Id.txtamount);
			EditText ttltax = FindViewById<EditText> (Resource.Id.txttaxamt);
		
			if (spinner.SelectedItem == null) {
				Toast.MakeText (this, Resources.GetString(Resource.String.msg_invaliditem), ToastLength.Long).Show ();			
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

			string[] codedesc = spinner.SelectedItem.ToString ().Split (new char[]{ '|' });
			var itemlist = items.Where (x => x.ICode == codedesc [0].Trim()).ToList<Item> ();
			if (itemlist.Count == 0) {
				Toast.MakeText (this, Resources.GetString(Resource.String.msg_invaliditem), ToastLength.Long).Show ();
				return;
			}
			Item ItemCode = itemlist [0];


			double stqQty = Convert.ToDouble(qty.Text);
			double uprice = Convert.ToDouble(price.Text);
			//double uprice= Utility.GetUnitPrice (trd,ItemCode);
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

			InvoiceDtls inv = new InvoiceDtls ();
			inv.invno = txtInvNo.Text;
			inv.amount = amount;
			inv.icode = codedesc [0].Trim();// spinner.SelectedItem.ToString ();
			inv.price = uprice;
			inv.qty = stqQty;
			inv.tax = taxamt;
			inv.taxgrp = txttax.Text;
			inv.netamount = netamount;
			inv.description = ItemCode.IDesc;

			int id = Convert.ToInt32 (ITEMUID);				
			//inv..title = spinner.SelectedItem.ToString ();
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var invlist =db.Table<InvoiceDtls> ().Where (x => x.invno == inv.invno&& x.ID==id).ToList<InvoiceDtls> ();
				if (invlist.Count > 0) {
					InvoiceDtls invItem = invlist [0];
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
				var itemlist = db.Table<InvoiceDtls> ().Where (x => x.invno == invno);	
				double ttlamt= itemlist.Sum (x => x.netamount);
				double ttltax= itemlist.Sum (x => x.tax);
			   var invlist =db.Table<Invoice> ().Where (x => x.invno == invno).ToList<Invoice> ();
				if (invlist.Count > 0) {
					invlist [0].amount = ttlamt;
					invlist [0].taxamt = ttltax;
					db.Update (invlist [0]);
				}
			}
			//base.OnBackPressed();
			string COMPCODE = ((GlobalvarsApp)this.Application).COMPANY_CODE;
			var intent =ActivityManager.GetActivity<InvItemActivity>(this.ApplicationContext);
			//var intent = new Intent(this, typeof(InvItemActivity));

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
			//EditText taxper = FindViewById<EditText> (Resource.Id.txtinvtaxper);
			//CheckBox isincl = FindViewById<CheckBox> (Resource.Id.txtinvisincl);
			EditText qty = FindViewById<EditText> (Resource.Id.txtqty);
		//	desc.Text = item.IDesc;
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
			case EventID.ICODE_SELECTED:
				RunOnUiThread (() => SetSelectedItem(e.Param["SELECTED"].ToString()));
				break;
			}
		}

		private void AddBarCodeItem(Item prd )
		{
			TextView txtInvNo =  FindViewById<TextView> (Resource.Id.txtInvnp);
			double stqQty = 1;
			double uprice= Utility.GetUnitPrice (trd, prd);
			double taxval = prd.tax;
			double amount = Math.Round((stqQty * uprice),2);
			double netamount = amount;
			bool taxinclusice = prd.isincludesive;
			double taxamt = 0;
			if (taxinclusice) {
				double percent = (taxval/100) + 1;
				double amt2 =Math.Round( amount / percent,2,MidpointRounding.AwayFromZero);
				taxamt = amount - amt2;
				netamount = amount - taxamt;

			} else {
				taxamt = Math.Round(amount * (taxval / 100),2,MidpointRounding.AwayFromZero);
			}

			InvoiceDtls inv = new InvoiceDtls ();
			inv.invno = txtInvNo.Text;
			inv.amount = amount;
			inv.icode = prd.ICode;
			inv.price = uprice;
			inv.qty = stqQty;
			inv.tax = taxamt;
			inv.taxgrp = prd.taxgrp;
			inv.netamount = netamount;
			inv.description = prd.IDesc;
			//int id = Convert.ToInt32 (ITEMUID);				
			//inv..title = spinner.SelectedItem.ToString ();
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list =db.Table<InvoiceDtls> ().Where (x => x.invno == txtInvNo.Text && x.icode == prd.ICode).ToList ();
				if (list.Count > 0) {
					list [0].qty = list [0].qty + 1;
					stqQty = list [0].qty;
					amount = Math.Round((stqQty * uprice),2);
					netamount = amount;
					if (taxinclusice) {
						double percent = (taxval/100) + 1;
						double amt2 =Math.Round( amount / percent,2,MidpointRounding.AwayFromZero);
						taxamt = amount - amt2;
						netamount = amount - taxamt;

					} else {
						taxamt = Math.Round(amount * (taxval / 100),2,MidpointRounding.AwayFromZero);
					}
					list [0].tax = taxamt;
					list [0].amount =amount;
					list [0].netamount = netamount;

					db.Update (list [0]);
				}else db.Insert (inv);
			}
			spinner.SetSelection (-1);
			Toast.MakeText (this, Resources.GetString(Resource.String.msg_itemadded), ToastLength.Long).Show ();
		}
	}
}

