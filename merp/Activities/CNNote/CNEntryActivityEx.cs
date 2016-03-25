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
using Android.Views.InputMethods;

namespace wincom.mobile.erp
{
	[Activity (Label = "CASH SALES",ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]			
	public class CNEntryActivityEx : Activity,IEventListener,Android.Views.View.IOnKeyListener
	{
		string pathToDatabase;
		string EDITMODE ="";
		string CUSTOMER ="";
		string CUSTCODE ="";
		string ITEMUID ="";
		string INVOICENO="";
		string FIRSTLOAD="";
		string TRXTYPE;
		List<Item> items = null;
		List<string> icodes;
		ArrayAdapter<String> dAdapterItem;
		AccessRights rights;
		Trader trd;
		double taxper;
		bool isInclusive;
		AdPara apara ;
		string cnno = "";
		volatile CNNote inv;
		volatile bool IsEdit;
		volatile int IDdtls;
		double ttlamt = 0;
		double ttltax = 0;
		volatile bool IsCashPay;
		volatile bool IsSave;
		volatile bool IsFirePaid;

		Spinner spinItem;
		EditText txtqty ;
		EditText txtprice ;
		TextView txtcnno;
		TextView txtInvDate;
		TextView txtInvMode;
		ListView listView;
		Button butFindItem;
		EditText txtbarcode ;
		Button butAdd ;
		Button butCancel;
		Button butPrint;

		List<CNNoteDtls> listData;
		TextView _tv;
		RelativeLayout.LayoutParams _layoutParamsPortrait;
		RelativeLayout.LayoutParams _layoutParamsLandscape;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!((GlobalvarsApp)this.Application).ISLOGON) {
				Finish ();
			}

			//this.RequestWindowFeature(WindowFeatures.NoTitle);
			TRXTYPE= Intent.GetStringExtra ("trxtype") ?? "CASH";
			INVOICENO = Intent.GetStringExtra ("invoiceno") ?? "AUTO";
			ITEMUID = Intent.GetStringExtra ("itemuid") ?? "AUTO";
			EDITMODE = Intent.GetStringExtra ("editmode") ?? "AUTO";
			CUSTOMER= Intent.GetStringExtra ("customer") ?? "AUTO";
			CUSTCODE= Intent.GetStringExtra ("custcode") ?? "AUTO";
			if (TRXTYPE == "CASH") {
				SetTitle (Resource.String.title_cashitementry);
			}else SetTitle (Resource.String.title_invitementry);
			EventManagerFacade.Instance.GetEventManager().AddListener(this);
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			rights = Utility.GetAccessRights (pathToDatabase);
			SetContentView (Resource.Layout.EntryEx2);
			_layoutParamsPortrait = new RelativeLayout.LayoutParams (ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent);
			_layoutParamsLandscape = new RelativeLayout.LayoutParams (ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent);

			GetControls ();
			LoadMasterTable ();
			SpinnerHandling ();
			ControlHandling ();

			listData = new List<CNNoteDtls> ();
			LoadCashBill ();
			populate (listData);
			SetViewDlg viewdlg = SetViewDelegate;
			listView.Adapter = new GenericListAdapter<CNNoteDtls> (this, listData, Resource.Layout.InvDtlItemViewCS, viewdlg);
			listView.ItemClick += OnListItemClick;
			if (EDITMODE == "EDIT") {
				int id = Convert.ToInt32 (ITEMUID);
				EditItem (INVOICENO, id);
			}
		}

		#region view init
		public override void OnBackPressed() {
			// do nothing.
		}

		void SpinnerHandling ()
		{
			dAdapterItem = new ArrayAdapter<String> (this, Resource.Layout.spinner_item, icodes);
			dAdapterItem.SetDropDownViewResource (Resource.Layout.SimpleSpinnerDropDownItemEx);
			spinItem.Adapter = dAdapterItem;
			spinItem.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs> (spinnerItem_ItemSelected);
		}

		void ControlHandling ()
		{
			txtqty.EditorAction += HandleEditorAction;
			txtqty.AfterTextChanged += Qty_AfterTextChanged;
			txtprice.Enabled = rights.InvEditUPrice;
			txtbarcode.SetOnKeyListener (this);
			butFindItem.Click += (object sender, EventArgs e) => {
				ShowItemLookUp ();
			};

			butPrint.Click += (object sender, EventArgs e) => {
				Printreceipt ();
			};
			butCancel.Click += (object sender, EventArgs e) => {
				CancelReceipt ();
			};
			butAdd.Click += butAddClick;

		}

		public override void OnConfigurationChanged (Android.Content.Res.Configuration newConfig)
		{
			base.OnConfigurationChanged (newConfig);

			if (newConfig.Orientation == Android.Content.Res.Orientation.Portrait) {
				//_tv.LayoutParameters = _layoutParamsPortrait;
				//_tv.Text = "Changed to portrait";
			} else if (newConfig.Orientation == Android.Content.Res.Orientation.Landscape) {
				//_tv.LayoutParameters = _layoutParamsLandscape;
				//_tv.Text = "Changed to landscape";
			}
		}

		#endregion view init

		void LoadCashBill()
		{
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list= db.Table<CNNote> ().Where (x => x.cnno == INVOICENO).ToList ();
				if (list.Count > 0) {
					inv = list [0];	
					txtcnno.Text = inv.cnno;
					txtInvDate.Text = inv.invdate.ToString ("dd-MM-yyyy");
					txtInvMode.Text = "NEW";
				}
			}
			EnableControLs (true, true, false, true, false);

		}

		#region Control stuff
		void GetControls ()
		{
			spinItem = FindViewById<Spinner> (Resource.Id.txtcode);
			butFindItem = FindViewById<Button> (Resource.Id.bfindItem);
			txtqty = FindViewById<EditText> (Resource.Id.txtqty);
			txtprice = FindViewById<EditText> (Resource.Id.txtprice);
			txtcnno = FindViewById<TextView> (Resource.Id.txtInvno);
			txtInvDate = FindViewById<TextView> (Resource.Id.txtInvdate);
			txtInvMode = FindViewById<TextView> (Resource.Id.txtInvmode);
			txtbarcode = FindViewById<EditText> (Resource.Id.txtbarcode);
			butAdd = FindViewById<Button> (Resource.Id.Save);
			butCancel = FindViewById<Button> (Resource.Id.Cancel);
			butPrint = FindViewById<Button> (Resource.Id.Print);
			listView = FindViewById<ListView> (Resource.Id.invitemList);
		
		}

		void EnableControLs(bool lAdd,bool lCancel, bool lPrint, bool lPaid,bool lNew)
		{
			butAdd.Enabled = lAdd;
			butCancel.Enabled = lCancel;
			butPrint.Enabled = lPrint;
		}

		#endregion Control stuff

		#region Item ListView stuff
		private void SetViewDelegate(View view,object clsobj)
		{
			CNNoteDtls item = (CNNoteDtls)clsobj;
			string sqty =item.qty==0?"": item.qty.ToString ();
			string sprice =item.price==0?"": item.price.ToString ("n2");

			if (item.icode == "TAX" || item.icode == "AMOUNT") {
				view.FindViewById<LinearLayout> (Resource.Id.linearLayout1).Visibility = ViewStates.Gone;
				view.FindViewById<LinearLayout> (Resource.Id.linearLayout2).Visibility = ViewStates.Visible;
				view.FindViewById<TextView> (Resource.Id.invitemdesc).Visibility = ViewStates.Gone;
				view.FindViewById<TextView> (Resource.Id.invitemTemp1).Text = item.description;
				view.FindViewById<TextView> (Resource.Id.invitemTemp2).Text =item.netamount.ToString ("n2");
			} else {
				view.FindViewById<LinearLayout> (Resource.Id.linearLayout2).Visibility = ViewStates.Gone;
				view.FindViewById<LinearLayout> (Resource.Id.linearLayout1).Visibility = ViewStates.Visible;
				view.FindViewById<TextView> (Resource.Id.invitemdesc).Visibility = ViewStates.Visible;
				view.FindViewById<TextView> (Resource.Id.invitemdesc).Text = item.description;
				view.FindViewById<TextView> (Resource.Id.invitemcode).Text = item.icode;
				view.FindViewById<TextView> (Resource.Id.invitemtax).Text = item.tax.ToString ("n2");
				view.FindViewById<TextView> (Resource.Id.invitemprice).Text = sprice;
				view.FindViewById<TextView> (Resource.Id.invitemamt).Text = item.netamount.ToString ("n2");
				view.FindViewById<TextView> (Resource.Id.invitemqty).Text = sqty;
				view.FindViewById<TextView> (Resource.Id.invitemtaxgrp).Text = item.taxgrp;
			}
		}

		void OnListItemClick(object sender, AdapterView.ItemClickEventArgs e) {
			CNNoteDtls item = listData.ElementAt (e.Position);
			if (item.icode.IndexOf ("TAX") > -1)
				return;
			if (item.icode.IndexOf ("AMOUNT") > -1)
				return;

			PopupMenu menu = new PopupMenu (e.Parent.Context, e.View);
			menu.Inflate (Resource.Menu.popupItem);
			menu.Menu.RemoveItem (Resource.Id.popadd);

			menu.MenuItemClick += (s1, arg1) => {
				if (arg1.Item.ItemId==Resource.Id.popdelete)
				{
					Delete(item);
				}else if (arg1.Item.ItemId==Resource.Id.popedit)
				{
					EditItem(item.cnno,item.ID);
				}

			};
			menu.Show ();

		}

		void EditItem(string cnno, int id)
		{
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				var list = db.Table<CNNoteDtls>().Where(x=>x.cnno==cnno&& x.ID==id).ToList<CNNoteDtls>();
				if (list.Count > 0) {
					txtqty.Text = list [0].qty.ToString();
					txtprice.Text = list [0].price.ToString("n2");
					int pos1 = 0;
					if (list [0].description.Length > 40) {
						pos1= dAdapterItem.GetPosition (list [0].icode+" | "+list [0].description.Substring(0,40)+"...");
					}else  pos1= dAdapterItem.GetPosition (list [0].icode+" | "+list [0].description);

					spinItem.SetSelection (pos1);
					spinItem.Enabled = false;
					butFindItem.Enabled = false;
					IsEdit = true;
					IDdtls = list [0].ID;
					txtInvMode.Text = "EDIT";
					txtqty.RequestFocus ();
					txtqty.SelectAll ();
				}
			}
		}

		void Delete(CNNoteDtls invdtls)
		{
			var builder = new AlertDialog.Builder(this);
			builder.SetMessage(Resources.GetString(Resource.String.msg_confirmdelete)+"?");
			builder.SetPositiveButton("YES", (s, e) => { DeleteItem(invdtls); });
			builder.SetNegativeButton("Cancel", (s, e) => { /* do something on Cancel click */ });
			builder.Create().Show();
		}

		void DeleteItem(CNNoteDtls invdtls)
		{
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				var list = db.Table<CNNoteDtls>().Where(x=>x.cnno==invdtls.cnno&& x.ID==invdtls.ID).ToList<CNNoteDtls>();
				if (list.Count > 0) {
					db.Delete (list [0]);
					var arrlist= listData.Where(x=>x.cnno==invdtls.cnno&& x.ID==invdtls.ID).ToList<CNNoteDtls>();
					if (arrlist.Count > 0) {
						listData.Remove (arrlist [0]);
						SetViewDlg viewdlg = SetViewDelegate;
						listView.Adapter = new GenericListAdapter<CNNoteDtls> (this, listData, Resource.Layout.InvDtlItemViewCS, viewdlg);
					}
				}
			}
		}

		#endregion Item ListView stuff

		#region Loading Data
		void populate(List<CNNoteDtls> list)
		{
			list.Clear ();
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list2 = db.Table<CNNoteDtls> ().Where (x => x.cnno == inv.cnno)
					.OrderByDescending(x=>x.ID)
					.ToList<CNNoteDtls> ();

				ttlamt = 0;
				ttltax = 0;
				foreach (var item in list2) {
					ttlamt = ttlamt + item.netamount;
					ttltax = ttltax + item.tax;
					list.Add (item);
				}

				double roundVal = 0;
				double ttlNet = Utility.AdjustToNear (ttlamt + ttltax, ref roundVal);


				CNNoteDtls inv1 = new CNNoteDtls ();
				inv1.icode = "TAX";
				inv1.netamount = ttlamt;
				inv1.description = "TOTAL EXCL GST";
				CNNoteDtls inv2 = new CNNoteDtls ();
				inv2.icode = "AMOUNT";
				inv2.netamount = ttltax;
				inv2.description = "TOTAL 6% GST" ;
				CNNoteDtls inv3 = new CNNoteDtls ();
				inv3.icode = "TAX";
				inv3.netamount = ttlamt + ttltax;
				inv3.description = "TOTAL INCL GST" ;
				CNNoteDtls inv4 = new CNNoteDtls ();
				inv4.icode = "AMOUNT";
				inv4.netamount =  roundVal;
				inv4.description = "ROUNDING ADJUST";
				CNNoteDtls inv5 = new CNNoteDtls ();
				inv5.icode = "AMOUNT";
				inv5.netamount = ttlNet;
				inv5.description = "NET TOTAL";


				list.Add (inv1);
				list.Add (inv2);
				list.Add (inv3);
				if (TRXTYPE == "CASH") {
					list.Add (inv4);
					list.Add (inv5);
				}
			}
		}

		void LoadMasterTable()
		{
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				items = db.Table<Item> ().ToList<Item> ();
			}

			icodes = new List<string> ();
			foreach (Item item in items) {
				if (item.IDesc.Length > 40) {
					icodes.Add (item.ICode + " | " + item.IDesc.Substring(0,40)+"...");
				}else icodes.Add (item.ICode + " | " + item.IDesc);
			}

	
		}

		#endregion Loading Data

		#region Dialog Stuff
		void ShowItemLookUp()
		{
			var dialog = ItemDialog.NewInstance();
			dialog.Show(FragmentManager, "dialog");
		}

//		void ShowCustLookUp()
//		{
//			var dialog = TraderDialog.NewInstance();
//			dialog.Show(FragmentManager, "dialog");
//		}
//
//		void SetSelectedCust(string text)
//		{
//			int position=dAdapterCust.GetPosition (text);
//			spinCust.SetSelection (position);
//		}

		void SetSelectedItem(string text)
		{
			int position=dAdapterItem.GetPosition (text);
			spinItem.SetSelection (position);
		}

		#endregion Dialog Stuff

		#region splinner Stuff
		private void spinnerItem_ItemSelected (object sender, AdapterView.ItemSelectedEventArgs e)
		{
			Spinner spinner = (Spinner)sender;

			string []codedesc = spinner.GetItemAtPosition (e.Position).ToString().Split (new char[]{ '|' });
			string icode = codedesc[0].Trim();
			Item item =items.Where (x => x.ICode == icode).FirstOrDefault ();
			TextView tax =  FindViewById<TextView> (Resource.Id.txttax);
			EditText price = FindViewById<EditText> (Resource.Id.txtprice);
			EditText qty = FindViewById<EditText> (Resource.Id.txtqty);

			double uprice= Utility.GetUnitPrice (trd, item);
			price.Text = uprice.ToString ();
			tax.Text = item.taxgrp;
			taxper = item.tax;
			isInclusive = item.isincludesive;
			qty.RequestFocus ();
			//ShowKeyBoard (qty as View);

		}

		#endregion splinner Stuff

		#region Editor Action
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

//		void ShowKeyBoard(View view)
//		{
//			InputMethodManager imm = (InputMethodManager)GetSystemService(Context.InputMethodService);
//			imm.ShowSoftInputFromInputMethod(view.WindowToken, ShowFlags.Forced);
//			imm.ToggleSoftInput (ShowFlags.Forced, 0); 
//		}

		void Qty_AfterTextChanged (object sender, Android.Text.AfterTextChangedEventArgs e)
		{
			CalAmt ();
		}

		private void CalAmt()
		{
			EditText ttlamt = FindViewById<EditText> (Resource.Id.txtamount);
			EditText ttltax = FindViewById<EditText> (Resource.Id.txttaxamt);
			EditText qty = FindViewById<EditText> (Resource.Id.txtqty);
			EditText price = FindViewById<EditText> (Resource.Id.txtprice);
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

		#endregion Editor Action

		#region Barcode Action
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
			RefreshItemList ();
			txtbarcode.Text = "";
			txtbarcode.RequestFocus ();
		}

		private void AddBarCodeItem(Item prd )
		{
			//TextView txtcnno =  FindViewById<TextView> (Resource.Id.txtInvnp);
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

			CNNoteDtls invdtls = new CNNoteDtls ();
			invdtls.cnno = inv.cnno;
			invdtls.amount = amount;
			invdtls.icode = prd.ICode;
			invdtls.price = uprice;
			invdtls.qty = stqQty;
			invdtls.tax = taxamt;
			invdtls.taxgrp = prd.taxgrp;
			invdtls.netamount = netamount;
			invdtls.description = prd.IDesc;
			//int id = Convert.ToInt32 (ITEMUID);				
			//inv..title = spinner.SelectedItem.ToString ();
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list =db.Table<CNNoteDtls> ().Where (x => x.cnno == inv.cnno && x.icode == prd.ICode).ToList ();
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
				}else db.Insert (invdtls);
			}
			spinItem.SetSelection (-1);
			Toast.MakeText (this, Resources.GetString(Resource.String.msg_itemadded), ToastLength.Long).Show ();
		}

		#endregion Barcode Action

		#region button Click Action
		bool IsValidEntry ()
		{
			if (spinItem.SelectedItem == null) {
				Toast.MakeText (this, Resources.GetString (Resource.String.msg_invaliditem), ToastLength.Long).Show ();
				spinItem.RequestFocus ();
				return false;
			}
			if (string.IsNullOrEmpty (txtqty.Text)) {
				Toast.MakeText (this, Resources.GetString (Resource.String.msg_invalidqty), ToastLength.Long).Show ();
				txtqty.RequestFocus ();
				return false;
			}
			if (string.IsNullOrEmpty (txtprice.Text)) {
				Toast.MakeText (this, Resources.GetString (Resource.String.msg_invalidprice), ToastLength.Long).Show ();
				txtprice.RequestFocus ();
				return false;
			}

			return true;
		}

		void RefreshItemList ()
		{
			populate (listData);
			SetViewDlg viewdlg = SetViewDelegate;
			listView.Adapter = new GenericListAdapter<CNNoteDtls> (this, listData, Resource.Layout.InvDtlItemViewCS, viewdlg);
			listView.SetSelection (0);
		}

		private void butAddClick(object sender,EventArgs e)
		{
			GetControls ();
			TextView txttax =  FindViewById<TextView> (Resource.Id.txttax);

			if (!IsValidEntry ())
				return;

			string[] codedesc = spinItem.SelectedItem.ToString ().Split (new char[]{ '|' });
			var itemlist = items.Where (x => x.ICode == codedesc [0].Trim()).ToList<Item> ();
			if (itemlist.Count == 0) {
				Toast.MakeText (this, Resources.GetString(Resource.String.msg_invaliditem), ToastLength.Long).Show ();
				return;
			}
			Item ItemCode = itemlist [0];

			double stqQty = Convert.ToDouble(txtqty.Text);
			double uprice = Convert.ToDouble(txtprice.Text);
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
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				CNNoteDtls invdtl = new CNNoteDtls ();

				if (IsEdit) {
					var list = db.Table<CNNoteDtls> ().Where (x => x.cnno == inv.cnno && x.ID == IDdtls).ToList<CNNoteDtls> ();
					if (list.Count > 0) {
						invdtl = list [0];
					}
				}

				invdtl.cnno =inv.cnno;
				invdtl.amount = amount;
				invdtl.icode = codedesc [0].Trim();// spinner.SelectedItem.ToString ();
				invdtl.price = uprice;
				invdtl.qty = stqQty;
				invdtl.tax = taxamt;
				invdtl.taxgrp = txttax.Text;
				invdtl.netamount = netamount;
				invdtl.description = ItemCode.IDesc;
				invdtl.isincludesive = taxinclusice;
				if (IsEdit)
					db.Update(invdtl);
				else db.Insert (invdtl);
			}


			IsEdit = false;
			IDdtls = -1;
			spinItem.Enabled = true;
			butFindItem.Enabled = true;
			//spinner.SetSelection (-1);
			txtqty.Text = "";
			txtInvMode.Text = "NEW";
			RefreshItemList ();
		}

		void CancelReceipt()
		{
			string cnno = Intent.GetStringExtra ("invoiceno") ?? "AUTO";
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var itemlist = db.Table<CNNoteDtls> ().Where (x => x.cnno == cnno);	
				double ttlamt= itemlist.Sum (x => x.netamount);
				double ttltax= itemlist.Sum (x => x.tax);
				var invlist =db.Table<CNNote> ().Where (x => x.cnno == cnno).ToList<CNNote> ();
				if (invlist.Count > 0) {
					invlist [0].amount = ttlamt;
					invlist [0].taxamt = ttltax;
					db.Update (invlist [0]);
				}
			}
			//base.OnBackPressed();
			string COMPCODE = ((GlobalvarsApp)this.Application).COMPANY_CODE;
			var intent =ActivityManager.GetActivity<CNItemActivity>(this.ApplicationContext);
			//var intent = new Intent(this, typeof(InvItemActivity));

			intent.PutExtra ("invoiceno",INVOICENO );
			intent.PutExtra ("custcode",CUSTCODE );
			intent.PutExtra ("editmode",EDITMODE );
			intent.PutExtra ("trxtype", TRXTYPE);

			StartActivity(intent);
		}

		void Printreceipt()
		{
			CNNoteDtls[] list;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)){
				var ls= db.Table<CNNoteDtls> ().Where (x => x.cnno==inv.cnno).ToList<CNNoteDtls>();
				list = new CNNoteDtls[ls.Count];
				ls.CopyTo (list);
			}
			//mmDevice = null;
			//findBTPrinter ();
			IPrintDocument prtInv = PrintDocManager.GetPrintDocument<PrintCreditNote>();
			prtInv.SetDocument (inv);
			prtInv.SetDocumentDtls(list);
			prtInv.SetNoOfCopy (1);
			prtInv.SetCallingActivity (this);
			if (prtInv.StartPrint ()) {
				updatePrintedStatus (inv);

			} else {
				Toast.MakeText (this, prtInv.GetErrMsg(), ToastLength.Long).Show ();	
			}
		}

		void updatePrintedStatus(CNNote inv)
		{
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list = db.Table<CNNote> ().Where (x => x.cnno == inv.cnno).ToList<CNNote> ();
				if (list.Count > 0) {
					//if only contains items then allow to update the printed status.
					//this to allow the invoice;s item can be added. if not can not be posted(upload)
					var list2 = db.Table<CNNoteDtls> ().Where (x => x.cnno == inv.cnno).ToList<CNNoteDtls> ();
					if (list2.Count > 0) {
						list [0].isPrinted = true;
						db.Update (list [0]);
					}
				}
			}
		}

		void ClearData ()
		{
			EditText txtttlamt = FindViewById<EditText> (Resource.Id.txtamount);
			EditText txtttltax = FindViewById<EditText> (Resource.Id.txttaxamt);
			txtqty.Text = "";
			//txtprice.Text = "";
			txtttltax.Text = "";
			txtttlamt.Text = "";

		}

		private void BacktoMainMenu()
		{
			if (!IsSave)
				DeleteInv ();
			base.OnBackPressed();
		}

		private void DeleteInv()
		{
			try {
				using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
					var list = db.Table<CNNoteDtls> ().Where (x => x.cnno == cnno).ToList<CNNoteDtls> ();
					var list2 = db.Table<CNNote> ().Where (x => x.cnno == cnno).ToList<CNNote> ();
					if (list2.Count > 0) {
						string trxtype = "CS";
						AdNumDate adNum = DataHelper.GetNumDate (pathToDatabase, list2 [0].invdate, trxtype);
						if (cnno.Length > 5) {
							string snum = cnno.Substring (cnno.Length - 4);					
							int num;
							if (int.TryParse (snum, out num)) {
								if (adNum.RunNo == num) {
									adNum.RunNo = num - 1;
									db.Delete (list2 [0]);
									if (list.Count>0)
										db.Delete(list);
									db.Delete (adNum);
								}
							}
						}
					}
				}
			} catch {
			}
		}

		#endregion button Click Action

		#region Event
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
				RunOnUiThread (() => SetSelectedItem (e.Param ["SELECTED"].ToString ()));
				break;
			}
		}
		#endregion Event
	}
}

