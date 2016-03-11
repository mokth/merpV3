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
using System.Collections;

namespace wincom.mobile.erp
{
	[Activity (Label = "INVOICE",ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]			
	public class InvoiceActivityEx : Activity,IEventListener,Android.Views.View.IOnKeyListener
	{
		string pathToDatabase;
		List<Item> items = null;
		List<Trader> trds = null;
		List<string> custcodes;
		List<string> icodes;
		ArrayAdapter<String> dAdapterItem;
		ArrayAdapter<String> dAdapterCust;
		ArrayAdapter dAdapterQty;
		AccessRights rights;
		Trader trd;
		double taxper;
		bool isInclusive;
		AdPara apara ;
		string invno = "";
		volatile Invoice inv;
		double ttlamt = 0;
		double ttltax = 0;
		volatile bool IsCashPay;
		volatile bool IsSave;
		volatile bool IsEdit;
		volatile int IDdtls;
		volatile bool IsFirePaid;
		volatile bool IsFirePaidOnly;

		Spinner spinItem;
		Spinner spinCust;
		Spinner spinQty;
		EditText txtqty ;
		EditText txtprice ;
		string txtcustname;
		ListView listView;
		TextView txtcust;
		TextView txtInvNo;
		EditText txtInvDate;
		EditText txtRemark;
		TextView txtInvMode;
		TextView txttax;
		Button butFindCust;
		Button butFindItem;
		Button butNew;
		Button butHome;
		EditText txtbarcode ;
		Button butAdd ;
		Button butCancel;
		Button butPaid ;
		Button butList;
		Button butPrint;
		DateTime _date ;

		List<InvoiceDtls> listData;
		TextView _tv;
		RelativeLayout.LayoutParams _layoutParamsPortrait;
		RelativeLayout.LayoutParams _layoutParamsLandscape;
		bool isNotAllowEditAfterPrinted  ;
		string INVOICENO ;
		string INVACTION;
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!((GlobalvarsApp)this.Application).ISLOGON) {
				Finish ();
			}
			SetTitle (Resource.String.submenu_inv);
		//	this.RequestWindowFeature(WindowFeatures.NoTitle);
			INVOICENO = Intent.GetStringExtra ("invoiceno") ?? "";
			INVACTION = Intent.GetStringExtra ("action") ?? "";
			_date = DateTime.Today;

			EventManagerFacade.Instance.GetEventManager().AddListener(this);
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			rights = Utility.GetAccessRights (pathToDatabase);
			isNotAllowEditAfterPrinted  = DataHelper.GetInvoicePrintStatus (pathToDatabase,invno,rights);

			SetContentView (Resource.Layout.InvoiceEntry);
			_layoutParamsPortrait = new RelativeLayout.LayoutParams (ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent);
			_layoutParamsLandscape = new RelativeLayout.LayoutParams (ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent);

			GetControls ();
			LoadMasterTable ();
			SpinnerHandling ();
			ControlHandling ();

			listData = new List<InvoiceDtls> ();
			//CreateCashBill ();
			LoadInvoice ();
			populate (listData);
			SetViewDlg viewdlg = SetViewDelegate;
			listView.Adapter = new GenericListAdapterEx<InvoiceDtls> (this, listData, Resource.Layout.InvDtlItemViewCS, viewdlg);
			listView.ItemClick += OnListItemClick;
		}

		void LoadInvoice ()
		{
			if (INVOICENO == "") {
				if (INVACTION == "create") {
					CreateCashBill ();
				} else {
					EnableControLs (false, false, true, false, true);
					inv = new Invoice ();
				}
			}
			else {
				loadInvouce (INVOICENO);
				if (inv != null) {
					txtInvMode.Text = "EDIT";
					txtInvNo.Text = inv.invno;
					txtInvDate.Text = inv.invdate.ToString ("dd-MM-yyyy");
					txtRemark.Text = inv.remark.ToUpper ();
					if (!string.IsNullOrEmpty (inv.custcode)) {
						int pos1 = dAdapterCust.GetPosition (inv.custcode + " | " + inv.description.Trim ());
						if (pos1 > 0)
							spinCust.SetSelection (pos1);
						else
							spinCust.SetSelection (0);
					} else
						spinCust.SetSelection (0);
					EnableControLs (true, false, true, true, false);
				} else {
					EnableControLs (false, false, true, false, true);
					inv = new Invoice ();
				}
			}
		}

		#region view init
		public override void OnBackPressed() {
			// do nothing.
		}

		void SpinnerHandling ()
		{
			dAdapterQty  = ArrayAdapter.CreateFromResource (this, Resource.Array.qtytype,Resource.Layout.spinner_item);
			dAdapterQty.SetDropDownViewResource (Resource.Layout.SimpleSpinnerDropDownItemEx);
			dAdapterItem = new ArrayAdapter<String> (this, Resource.Layout.spinner_item, icodes);
			dAdapterItem.SetDropDownViewResource (Resource.Layout.SimpleSpinnerDropDownItemEx);
			dAdapterCust = new ArrayAdapter<String> (this, Resource.Layout.spinner_item, custcodes);
			dAdapterCust.SetDropDownViewResource (Resource.Layout.SimpleSpinnerDropDownItemEx);
			spinCust.Adapter = dAdapterCust;
			spinItem.Adapter = dAdapterItem;
			spinQty.Adapter = dAdapterQty;
			spinItem.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs> (spinnerItem_ItemSelected);
			spinCust.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs> (spinnerCust_ItemSelected);

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
			butFindCust.Click += (object sender, EventArgs e) => {
				ShowCustLookUp ();
			};
			butList.Click += (object sender, EventArgs e) => {
				//Printreceipt ();
				CashBillList();
			};
			butCancel.Click += (object sender, EventArgs e) => {
				CancelReceipt ();
			};
			butHome.Click += (object sender, EventArgs e) => {
				BacktoMainMenu ();
			};
			butNew.Click += (object sender, EventArgs e) => {
				NewBill ();
			};
			butPrint.Click += (object sender, EventArgs e) => {
				Printreceipt();
			};
			butAdd.Click += butAddClick;
			butPaid.Click += ButPaid_Click;
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
		void CreateCashBill()
		{
			if (!Utility.IsValidDateString (txtInvDate.Text)) {
				Toast.MakeText (this,Resources.GetString(Resource.String.msg_invaliddate), ToastLength.Long).Show ();	
				return;
			}
			DateTime invdate = Utility.ConvertToDate (txtInvDate.Text);
			ValidateInvDate (invdate);
		}

		void ValidateInvDate(DateTime invdate)
		{
			bool diffYearMonth = false;
			DateTime today = DateTime.Today;
			if (invdate.Year != today.Year) {
				diffYearMonth = true;
			}
			if (invdate.Month != today.Month) {
				diffYearMonth = true;
			}
			if (!diffYearMonth) {
				CreateNewInv ();
				return;
			}

			var builder = new AlertDialog.Builder(this);
			builder.SetMessage("Invoice date is not in the current month. Confirm to use this date?");
			builder.SetPositiveButton("YES", (s, e) => { CreateNewInv();});
			builder.SetNegativeButton("NO", (s, e) => {
				txtInvDate.Text =DateTime.Today.ToString("dd-MM-yyyy");
				CreateNewInv();
			});
			builder.Create().Show();
		}

		void CreateNewInv()
		{
			
			DateTime invdate = Utility.ConvertToDate (txtInvDate.Text);
			DateTime tmr = invdate.AddDays (1);
			AdNumDate adNum;// = DataHelper.GetNumDate (pathToDatabase, invdate);
			apara =  DataHelper.GetAdPara (pathToDatabase);
			string[] prefixs = apara.Prefix.Trim ().ToUpper ().Split(new char[]{'|'});
			string prefix = "";
			adNum = DataHelper.GetNumDate (pathToDatabase, invdate, "INV");
			if (prefixs.Length > 1) {
				prefix = prefixs [0];
			}else prefix = prefixs [0];

			invno = "";
			int runno = adNum.RunNo + 1;
			int currentRunNo =DataHelper.GetLastInvRunNo (pathToDatabase, invdate,"INVOICE");
			if (currentRunNo >= runno)
				runno = currentRunNo + 1;

			invno =prefix + invdate.ToString("yyMM") + runno.ToString().PadLeft (4, '0');

			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				inv = new Invoice ();
				inv.invno= invno;
				inv.invdate = invdate;
				inv.trxtype = "INVOICE";
				inv.created = DateTime.Now;
				inv.amount = 0;
				inv.isUploaded = false;
				inv.remark ="";
				db.Insert (inv);
				adNum.RunNo = runno;
				if (adNum.ID >= 0)
					db.Update (adNum);
				else
					db.Insert (adNum);
			}
			txtInvNo.Text = inv.invno;
			txtInvDate.Text = inv.invdate.ToString ("dd-MM-yyyy");
			txtInvMode.Text = "NEW";
			IsSave = false;
			IsFirePaid = false;
			IsFirePaidOnly = false;
			EnableControLs (true, true, false, true, false);

		}

		#region Control stuff
		void GetControls ()
		{
			spinItem = FindViewById<Spinner> (Resource.Id.txtcode);
			spinCust = FindViewById<Spinner> (Resource.Id.custcode);
			spinQty = FindViewById<Spinner> (Resource.Id.qty_type);
			butFindCust = FindViewById<Button> (Resource.Id.bfindCust);
			butFindItem = FindViewById<Button> (Resource.Id.bfindItem);
			txtqty = FindViewById<EditText> (Resource.Id.txtqty);
			txtprice = FindViewById<EditText> (Resource.Id.txtprice);
			txttax =  FindViewById<TextView> (Resource.Id.txttax);
			txtInvNo = FindViewById<TextView> (Resource.Id.txtInvno);
			txtInvDate = FindViewById<EditText> (Resource.Id.txtInvdate);
			txtInvMode = FindViewById<TextView> (Resource.Id.txtInvmode);
			txtcust = FindViewById<TextView> (Resource.Id.txtInvcust);
			txtbarcode = FindViewById<EditText> (Resource.Id.txtbarcode);
			txtRemark = FindViewById<EditText> (Resource.Id.newinv_remark);
			butAdd = FindViewById<Button> (Resource.Id.Save);
			butCancel = FindViewById<Button> (Resource.Id.Cancel);
	        butPaid = FindViewById<Button> (Resource.Id.Paid);
			butList = FindViewById<Button> (Resource.Id.List);
			butPrint = FindViewById<Button> (Resource.Id.Print);
			butNew = FindViewById<Button> (Resource.Id.NewBill);
			butHome= FindViewById<Button> (Resource.Id.Home);
			listView = FindViewById<ListView> (Resource.Id.invitemList);
			txtInvDate.Text = DateTime.Today.ToString("dd-MM-yyyy");
			if (rights.InvEditTrxDate) {
				txtInvDate.Click += delegate(object sender, EventArgs e) {
					ShowDialog (0);
				};
			} else
				txtInvDate.Enabled = false;

			if (!rights.InvMultiType)
				spinQty.Enabled = false;


			//txtcustname = FindViewById<EditText> (Resource.Id.newinv_custname);
		}

		void EnableControLs(bool lAdd,bool lCancel, bool lPrint, bool lPaid,bool lNew)
		{
			butAdd.Visibility = (lAdd)?ViewStates.Visible:ViewStates.Gone ;
			butCancel.Visibility = (lCancel)?ViewStates.Visible:ViewStates.Gone;
			butList.Visibility = (lPrint)?ViewStates.Visible:ViewStates.Gone;
			butPaid.Visibility = (lPaid)?ViewStates.Visible:ViewStates.Gone;
			butNew.Visibility = (lNew)?ViewStates.Visible:ViewStates.Gone;
			butHome.Visibility = (lNew)?ViewStates.Visible:ViewStates.Gone;
			butPrint.Visibility = ViewStates.Gone;

			spinCust.Enabled = lAdd;
			spinItem.Enabled = lAdd;
			txtqty.Enabled = lAdd;
			txtprice.Enabled = lAdd;
			txtbarcode.Enabled = lAdd;
			butFindCust.Enabled = lAdd;
			butFindItem.Enabled = lAdd;
			txtInvDate.Enabled = lNew;

			if (!rights.InvAllowAdd)
				butNew.Visibility = ViewStates.Gone;
		}

		#endregion Control stuff

		#region Item ListView stuff
		private void SetViewDelegate(View view,object clsobj)
		{
			InvoiceDtls item = (InvoiceDtls)clsobj;
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

			//piad, can not delete item
			if (IsCashPay)
				return;
			
			InvoiceDtls item = listData.ElementAt (e.Position);
			if (item.icode.IndexOf ("TAX") > -1)
				return;
			if (item.icode.IndexOf ("AMOUNT") > -1)
				return;

			PopupMenu menu = new PopupMenu (e.Parent.Context, e.View);
			menu.Inflate (Resource.Menu.popupItem);
			menu.Menu.RemoveItem (Resource.Id.popadd);
			//menu.Menu.RemoveItem (Resource.Id.popedit);
			//			if (!rights.InvAllowDelete) {
			//				menu.Menu.RemoveItem (Resource.Id.popdelete);
			//			}

			menu.MenuItemClick += (s1, arg1) => {
				if (arg1.Item.ItemId==Resource.Id.popdelete)
				{
					Delete(item);
				}else if (arg1.Item.ItemId==Resource.Id.popedit)
				{
					EditItem(item);
				}

			};
			menu.Show ();

		}

		void EditItem(InvoiceDtls invdtls)
		{
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				var list = db.Table<InvoiceDtls>().Where(x=>x.invno==invdtls.invno&& x.ID==invdtls.ID).ToList<InvoiceDtls>();
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
					//txtprice.Text = list [0].price.ToString("n2");
					spinQty.SetSelection (0);
					if (list [0].qty < 0)
						spinQty.SetSelection (1);

					if (list [0].price == 0)
						spinQty.SetSelection (2);
				}
			}
		}

		void Delete(InvoiceDtls invdtls)
		{
			var builder = new AlertDialog.Builder(this);
			builder.SetMessage(Resources.GetString(Resource.String.msg_confirmdelete)+"?");
			builder.SetPositiveButton("YES", (s, e) => { DeleteItem(invdtls); });
			builder.SetNegativeButton("Cancel", (s, e) => { /* do something on Cancel click */ });
			builder.Create().Show();
		}

		void DeleteItem(InvoiceDtls invdtls)
		{
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				var list = db.Table<InvoiceDtls>().Where(x=>x.invno==invdtls.invno&& x.ID==invdtls.ID).ToList<InvoiceDtls>();
				if (list.Count > 0) {
					db.Delete (list [0]);
					RefreshItemList ();
//					var arrlist= listData.Where(x=>x.invno==invdtls.invno&& x.ID==invdtls.ID).ToList<InvoiceDtls>();
//					if (arrlist.Count > 0) {
//						listData.Remove (arrlist [0]);
//						SetViewDlg viewdlg = SetViewDelegate;
//						listView.Adapter = new GenericListAdapterEx<InvoiceDtls> (this, listData, Resource.Layout.InvDtlItemViewCS, viewdlg);
//					}
				}
			}
		}

		#endregion Item ListView stuff

		#region Loading Data
		void loadInvouce(string invno){
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list = db.Table<Invoice> ().Where (x => x.invno == invno).ToList<Invoice> ();
				if (list.Count > 0) {
					inv = list [0];
				}
			}
		}

		void populate(List<InvoiceDtls> list)
		{
			list.Clear ();
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list2 = db.Table<InvoiceDtls> ()
					.Where (x => x.invno == inv.invno)
					.OrderByDescending(x=>x.ID)
					.ToList<InvoiceDtls> ();
		
				ttlamt = 0;
				ttltax = 0;
				foreach (var item in list2) {
					ttlamt = ttlamt + item.netamount;
					ttltax = ttltax + item.tax;
					list.Add (item);
				}

				double roundVal = 0;
				double ttlNet = Utility.AdjustToNear (ttlamt + ttltax, ref roundVal);


				InvoiceDtls inv1 = new InvoiceDtls ();
				inv1.icode = "TAX";
				inv1.netamount = ttlamt;
				inv1.description = "TOTAL EXCL GST";
				InvoiceDtls inv2 = new InvoiceDtls ();
				inv2.icode = "AMOUNT";
				inv2.netamount = ttltax;
				inv2.description = "TOTAL 6% GST" ;
				InvoiceDtls inv3 = new InvoiceDtls ();
				inv3.icode = "TAX";
				inv3.netamount = ttlamt + ttltax;
				inv3.description = "TOTAL INCL GST" ;
//				InvoiceDtls inv4 = new InvoiceDtls ();
//				inv4.icode = "AMOUNT";
//				inv4.netamount =  roundVal;
//				inv4.description = "ROUNDING ADJUST";
//				InvoiceDtls inv5 = new InvoiceDtls ();
//				inv5.icode = "AMOUNT";
//				inv5.netamount = ttlNet;
//				inv5.description = "NET TOTAL";


				list.Add (inv1);
				list.Add (inv2);
				list.Add (inv3);
				//list.Add (inv4);
				//list.Add (inv5);
			}
		}

		void LoadMasterTable()
		{
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				items = db.Table<Item> ().ToList<Item> ();
				trds = db.Table<Trader> ().ToList<Trader> ();
			}

			icodes = new List<string> ();
			icodes.Add (Resources.GetString(Resource.String.msg_select_item));
			foreach (Item item in items) {
				if (item.IDesc.Length > 40) {
					icodes.Add (item.ICode + " | " + item.IDesc.Substring(0,40)+"...");
				}else icodes.Add (item.ICode + " | " + item.IDesc);
			}

			custcodes = new List<string> ();
			custcodes.Add (Resources.GetString(Resource.String.msg_select_cust));
			foreach (Trader trd in trds) {
				custcodes.Add (trd.CustCode+" | "+trd.CustName.Trim());
			}
		}

		#endregion Loading Data

		#region Dialog Stuff
		void ShowItemLookUp()
		{
			var dialog = ItemDialog.NewInstance();
			dialog.Show(FragmentManager, "dialog");
		}

		void ShowCustLookUp()
		{
			var dialog = TraderDialog.NewInstance();
			dialog.Show(FragmentManager, "dialog");
		}

		void SetSelectedCust(string text)
		{
			int position=dAdapterCust.GetPosition (text);
			spinCust.SetSelection (position);
		}

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
			if (codedesc.Length <2)
				return;
			string icode = codedesc[0].Trim();
			Item item =items.Where (x => x.ICode == icode).FirstOrDefault ();
//			TextView tax =  FindViewById<TextView> (Resource.Id.txttax);
//			EditText price = FindViewById<EditText> (Resource.Id.txtprice);
//			EditText qty = FindViewById<EditText> (Resource.Id.txtqty);

			double uprice= Utility.GetUnitPrice (trd, item);
			if (txtInvMode.Text!="EDIT")
				txtprice.Text = uprice.ToString ();
			
			txttax.Text = item.taxgrp;
			taxper = item.tax;
			isInclusive = item.isincludesive;
			txtqty.RequestFocus ();
			//ShowKeyBoard (qty as View);

		}

		private void spinnerCust_ItemSelected (object sender, AdapterView.ItemSelectedEventArgs e)
		{
			Spinner spinner = (Spinner)sender;

			string txt = spinner.GetItemAtPosition (e.Position).ToString();
			string[] codes = txt.Split (new char[]{ '|' });
			if (codes.Length <2)
				return;

			trd  =trds.Where (x => x.CustCode ==codes[0].Trim()).FirstOrDefault ();
			if (trd == null) {
				trd = new Trader ();
			}

		
			txtcustname = trd.CustName.Trim ();
		}

		#endregion splinner Stuff

		#region Editor Action

		[Obsolete]
		protected override Dialog OnCreateDialog (int id)
		{
			return new DatePickerDialog (this, HandleDateSet, _date.Year,
				_date.Month - 1, _date.Day);
		}

		void HandleDateSet (object sender, DatePickerDialog.DateSetEventArgs e)
		{
			_date = e.Date;
			txtInvDate.Text = _date.ToString ("dd-MM-yyyy");
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

		void Qty_AfterTextChanged (object sender, Android.Text.AfterTextChangedEventArgs e)
		{
			CalAmt ();
		}

		private void CalAmt()
		{
			//EditText ttlamt = FindViewById<EditText> (Resource.Id.txtamount);
		//	EditText ttltax = FindViewById<EditText> (Resource.Id.txttaxamt);
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
				//ttlamt.Text = netamount.ToString("n2");
				//ttltax.Text = taxamt.ToString("n2");
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
			//TextView txtInvNo =  FindViewById<TextView> (Resource.Id.txtInvno);
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

			InvoiceDtls invdtls = new InvoiceDtls ();
			invdtls.invno = inv.invno;
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
				var list =db.Table<InvoiceDtls> ().Where (x => x.invno == inv.invno && x.icode == prd.ICode).ToList ();
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
			//spinItem.SetSelection (-1);
			ClearItemData ();
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
		
			double qty =Convert2NumTool<double>.ConvertVal (txtqty.Text);
			if (qty == 0) {
				Toast.MakeText (this, Resources.GetString (Resource.String.msg_invalidqty), ToastLength.Long).Show ();
				txtqty.RequestFocus ();
				return false;
			}

			return true;
		}

		void RefreshItemList ()
		{
			populate (listData);
			SetViewDlg viewdlg = SetViewDelegate;
			listView.Adapter = new GenericListAdapterEx<InvoiceDtls> (this, listData, Resource.Layout.InvDtlItemViewCS, viewdlg);
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
			int qtytye = spinQty.SelectedItemPosition;
			double stqQty = Convert.ToDouble(txtqty.Text);
			if (qtytye == 1) //return
				stqQty = stqQty * -1;
			
			double uprice = Convert.ToDouble(txtprice.Text);
			if (qtytye == 2) //FOC
				uprice = 0;
			
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
				InvoiceDtls invdtl = new InvoiceDtls ();

				if (IsEdit) {
					var list = db.Table<InvoiceDtls> ().Where (x => x.invno == inv.invno && x.ID == IDdtls).ToList<InvoiceDtls> ();
					if (list.Count > 0) {
						invdtl = list [0];
					}
				}

				invdtl.invno =inv.invno;
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
			//txtprice.Text = ""; 
			//txtttltax.Text = "";
			//txtttlamt.Text = "";
			ClearItemData ();
			RefreshItemList ();
		}

		void ButPaid_Click (object sender, EventArgs e)
		{
			if (spinCust.SelectedItem == null) {
				Toast.MakeText (this, Resources.GetString(Resource.String.msg_invalidcust), ToastLength.Long).Show ();			
				spinCust.RequestFocus ();
				return;			
			}

			if (spinCust.SelectedItem.ToString () == "") {
				Toast.MakeText (this, Resources.GetString(Resource.String.msg_invalidcust), ToastLength.Long).Show ();			
				spinCust.RequestFocus ();
				return;			
			}

			if (ttlamt == 0) {
				Toast.MakeText (this, Resources.GetString(Resource.String.msg_invalidqty), ToastLength.Long).Show ();			
				return;
			}
			SaveCashBill ();
			//ShowPaidment ();
		}

//		void ShowPaidment()
//		{
//			var dialog = CashDialog.NewInstance();
//			string[] codes = spinCust.SelectedItem.ToString ().Split (new char[]{ '|' });
//			if (codes.Length < 2) {
//				Toast.MakeText (this, Resources.GetString (Resource.String.msg_invalidcust), ToastLength.Long).Show ();
//				return;
//			}
//			dialog.Amount =ttlamt+ttltax;
//			dialog.CustName = codes [1];
//			dialog.InvNo = txtInvNo.Text;
//			dialog.Remark = string.IsNullOrEmpty (inv.remark) ? "" : inv.remark;
//			dialog.Show(FragmentManager, "dialogPaid");
//
//		}

		void CancelReceipt()
		{
			DeleteInv ();
			ttlamt = 0;
			ttltax = 0;
			inv = new Invoice ();
			ClearData ();
			ClearAllData ();
			EnableControLs (false, false, true, false, true);
			RefreshItemList ();
		}

		void CashBillList()
		{
			//var intent = new Intent (this, typeof(EditInvoice));
			var intent =ActivityManager.GetActivity<InvoiceListingEx>(this.ApplicationContext);
			StartActivity (intent);
		}

		void Printreceipt()
		{  
			InvoiceDtls[] list;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)){
				var ls= db.Table<InvoiceDtls> ().Where (x => x.invno==inv.invno).ToList<InvoiceDtls>();
				list = new InvoiceDtls[ls.Count];
				ls.CopyTo (list);
			}

			if (inv.custcode == null || inv.custcode == "") {
			
				string[] codes = spinCust.SelectedItem.ToString ().Split (new char[]{ '|' });
				if (codes.Length < 2) {
					Toast.MakeText (this, Resources.GetString (Resource.String.msg_invalidcust), ToastLength.Long).Show ();
					return;
				}
				inv.custcode = codes [0].Trim();
				inv.description = txtcustname;
			}
			//mmDevice = null;
			//findBTPrinter ();
			IPrintDocument prtInv = PrintDocManager.GetPrintDocument<PrintInvoice>();
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

		void updatePrintedStatus(Invoice inv)
		{
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list = db.Table<Invoice> ().Where (x => x.invno == inv.invno).ToList<Invoice> ();
				if (list.Count > 0) {
					//if only contains items then allow to update the printed status.
					//this to allow the invoice;s item can be added. if not can not be posted(upload)
					var list2 = db.Table<InvoiceDtls> ().Where (x => x.invno == inv.invno).ToList<InvoiceDtls> ();
					if (list2.Count > 0) {
						list [0].isPrinted = true;
						db.Update (list [0]);
					}
				}
			}
		}

		void SaveCashBill()
		{
			
			string[] codes = spinCust.SelectedItem.ToString ().Split (new char[]{ '|' });
			if (codes.Length < 2) {
				Toast.MakeText (this, Resources.GetString (Resource.String.msg_invalidcust), ToastLength.Long).Show ();
				return;
			}
			string remark = txtRemark.Text.ToUpper();
			bool needPrint = false;


			try {
				//txtInvNo.Text = "BILL No: " + inv.invno + " (PAID)";
				txtInvMode.Text ="SAVED";
				inv.description = txtcustname;//  codes [1].Trim ();//custname.Text;
				inv.amount = ttlamt;
				inv.taxamt = ttltax;
				inv.custcode = codes [0].Trim ();
				inv.remark= remark;
				using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
					var list2 = db.Table<InvoiceDtls> ().Where (x => x.invno == inv.invno).ToList<InvoiceDtls> ();
					ttlamt = 0;
					ttltax = 0;
					foreach (var item in list2) {
						ttlamt = ttlamt + item.netamount;
						ttltax = ttltax + item.tax;
					}
					var temp = db.Table<Invoice> ().Where (x => x.invno == inv.invno).ToList ();
					if (temp.Count > 0) {
						temp [0].description = txtcustname;
						temp [0].amount = ttlamt;
						temp [0].taxamt = ttltax;
						temp [0].custcode = codes [0].Trim ();
						temp [0].remark= remark;
						db.Update (temp [0]);
					} else
						db.Insert (inv);
				}

				IsCashPay = true;
				IsSave = true;
				IsFirePaidOnly = true;
				EnableControLs (false, false, true, false, true);
				butPrint.Visibility= ViewStates.Visible;
				//if (needPrint)
				//	Printreceipt ();
			}
			catch (Exception ex) {
			
			}
		}

		void ClearData ()
		{
			txtqty.Text = "";
			spinCust.SetSelection (0);
		}

		void ClearItemData ()
		{
			txtqty.Text = "";
			txtprice.Text = "";
			txttax.Text = "";
			spinItem.SetSelection (0);
		}

		void ClearAllData ()
		{
			txtqty.Text = "";
			txtprice.Text = "";
			txttax.Text = "";
			txtInvNo.Text = "";
			txtInvMode.Text = "";
			txtInvDate.Text = DateTime.Today.ToString ("dd-MM-yyyy");
			//	txtttlamt.Text = "";

		}

		private void NewBill()
		{
			if (DataHelper.IsUploadExpired (rights, pathToDatabase)) {
				Toast.MakeText (this,Resources.GetString(Resource.String.msg_mustupload), ToastLength.Long).Show ();	
				return;
			}

			ClearData ();
			CreateCashBill ();
			IsCashPay = false;
			IsSave = false;
			RefreshItemList ();
		}

		private void BacktoMainMenu()
		{
			if (!IsSave)
				DeleteInv ();
		
			var intent =ActivityManager.GetActivity<MainActivity>(this.ApplicationContext);
			StartActivity (intent);
		}

		private void DeleteInv()
		{
			try {
				using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
					var list = db.Table<InvoiceDtls> ().Where (x => x.invno == invno).ToList<InvoiceDtls> ();
					var list2 = db.Table<Invoice> ().Where (x => x.invno == invno).ToList<Invoice> ();
					if (list2.Count > 0) {
						string trxtype = "INV";
						AdNumDate adNum = DataHelper.GetNumDate (pathToDatabase, list2 [0].invdate, trxtype);
						if (invno.Length > 5) {
							string snum = invno.Substring (invno.Length - 4);					
							int num;
							if (int.TryParse (snum, out num)) {
								if (adNum.RunNo == num) {
									adNum.RunNo = num - 1;
									db.Update(adNum);
									db.Delete (list2 [0]);
									if (list.Count>0)
									{
										foreach(var itmdtls in list)
											db.Delete(itmdtls);
									}
								}
							}
						}
					}
				}
			} catch (Exception ex)
			{
				Toast.MakeText (this,ex.Message , ToastLength.Long).Show ();
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
			case EventID.CUSTCODE_SELECTED:
				RunOnUiThread (() => SetSelectedCust(e.Param["SELECTED"].ToString()));
				break;
			case EventID.ICODE_SELECTED:
				RunOnUiThread (() => SetSelectedItem(e.Param["SELECTED"].ToString()));
				break;
//			case EventID.PAYMENT_PAID:
//				if (!IsFirePaid) {
//					IsFirePaid = true;
//					RunOnUiThread (() => SaveCashBill (e.Param));
//				}
//				break;
//			case EventID.PAYMENT_PRINT:
//				if (!IsFirePaidOnly) {
//					IsFirePaidOnly = true;
//					RunOnUiThread (() => SaveCashBill (e.Param));
//				}
//				break;
			
			}
		}
		#endregion Event
	}
}

