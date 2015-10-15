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
	[Activity (Label = "CashSalesActivity",ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]			
	public class CashSalesActivity : Activity,IEventListener,Android.Views.View.IOnKeyListener
	{
		string pathToDatabase;
		List<Item> items = null;
		List<Trader> trds = null;
		List<string> custcodes;
		List<string> icodes;
		ArrayAdapter<String> dAdapterItem;
		ArrayAdapter<String> dAdapterCust;
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
		volatile bool IsFirePaid;

		Spinner spinItem;
		Spinner spinCust;
		EditText txtqty ;
		EditText txtprice ;
		EditText txtcustname;
		ListView listView;
		TextView txtcust;
		Button butFindCust;
		Button butFindItem;
		Button butNew;
		Button butHome;
		EditText txtbarcode ;
		Button butAdd ;
		Button butCancel;
		Button butPaid ;
		Button butPrint;

		List<InvoiceDtls> listData;
		TextView _tv;
		RelativeLayout.LayoutParams _layoutParamsPortrait;
		RelativeLayout.LayoutParams _layoutParamsLandscape;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			EventManagerFacade.Instance.GetEventManager().AddListener(this);
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			rights = Utility.GetAccessRights (pathToDatabase);
			SetContentView (Resource.Layout.CashEntry);
			_layoutParamsPortrait = new RelativeLayout.LayoutParams (ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent);
			_layoutParamsLandscape = new RelativeLayout.LayoutParams (ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent);

			GetControls ();
			LoadMasterTable ();
			SpinnerHandling ();
			ControlHandling ();

			listData = new List<InvoiceDtls> ();
			CreateCashBill ();
			populate (listData);
			SetViewDlg viewdlg = SetViewDelegate;
			listView.Adapter = new GenericListAdapter<InvoiceDtls> (this, listData, Resource.Layout.InvDtlItemViewCS, viewdlg);
			listView.ItemClick += OnListItemClick;
		}

		#region view init
		public override void OnBackPressed() {
			// do nothing.
		}

		void SpinnerHandling ()
		{
			dAdapterItem = new ArrayAdapter<String> (this, Resource.Layout.spinner_item, icodes);
			dAdapterItem.SetDropDownViewResource (Resource.Layout.SimpleSpinnerDropDownItemEx);
			dAdapterCust = new ArrayAdapter<String> (this, Resource.Layout.spinner_item, custcodes);
			dAdapterCust.SetDropDownViewResource (Resource.Layout.SimpleSpinnerDropDownItemEx);
			spinCust.Adapter = dAdapterCust;
			spinItem.Adapter = dAdapterItem;
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
			butPrint.Click += (object sender, EventArgs e) => {
				Printreceipt ();
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
			DateTime invdate = DateTime.Now;
			DateTime tmr = invdate.AddDays (1);
			AdNumDate adNum;// = DataHelper.GetNumDate (pathToDatabase, invdate);
			apara =  DataHelper.GetAdPara (pathToDatabase);
			string[] prefixs = apara.Prefix.Trim ().ToUpper ().Split(new char[]{'|'});
			string prefix = "";
			adNum = DataHelper.GetNumDate (pathToDatabase, invdate, "CS");
			if (prefixs.Length > 1) {
				prefix = prefixs [1];
			}else prefix = prefixs [0];

			invno = "";
			int runno = adNum.RunNo + 1;
			int currentRunNo =DataHelper.GetLastInvRunNo (pathToDatabase, invdate,"CASH");
			if (currentRunNo >= runno)
				runno = currentRunNo + 1;

			invno =prefix + invdate.ToString("yyMM") + runno.ToString().PadLeft (4, '0');

			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				inv = new Invoice ();
				inv.invno= invno;
				inv.invdate = DateTime.Now;
				inv.trxtype = "CASH";
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
			IsSave = false;
			IsFirePaid = false;
			EnableControLs (true, true, false, true, false);

		}

		#region Control stuff
		void GetControls ()
		{
			spinItem = FindViewById<Spinner> (Resource.Id.txtcode);
			spinCust = FindViewById<Spinner> (Resource.Id.custcode);
			butFindCust = FindViewById<Button> (Resource.Id.bfindCust);
			butFindItem = FindViewById<Button> (Resource.Id.bfindItem);
			txtqty = FindViewById<EditText> (Resource.Id.txtqty);
			txtprice = FindViewById<EditText> (Resource.Id.txtprice);
			//txtInvNo = FindViewById<TextView> (Resource.Id.txtInvnp);
			txtcust = FindViewById<TextView> (Resource.Id.txtInvcust);
			txtbarcode = FindViewById<EditText> (Resource.Id.txtbarcode);
			butAdd = FindViewById<Button> (Resource.Id.Save);
			butCancel = FindViewById<Button> (Resource.Id.Cancel);
			butPaid = FindViewById<Button> (Resource.Id.Paid);
			butPrint = FindViewById<Button> (Resource.Id.Print);
			butNew = FindViewById<Button> (Resource.Id.NewBill);
			butHome= FindViewById<Button> (Resource.Id.Home);
			listView = FindViewById<ListView> (Resource.Id.invitemList);
			txtcustname = FindViewById<EditText> (Resource.Id.newinv_custname);
		}

		void EnableControLs(bool lAdd,bool lCancel, bool lPrint, bool lPaid,bool lNew)
		{
			butAdd.Enabled = lAdd;
			butCancel.Enabled = lCancel;
			butPrint.Enabled = lPrint;
			butPaid.Enabled = lPaid;
			butNew.Enabled = lNew;
		}

		#endregion Control stuff

		#region Item ListView stuff
		private void SetViewDelegate(View view,object clsobj)
		{
			InvoiceDtls item = (InvoiceDtls)clsobj;
			string sqty =item.qty==0?"": item.qty.ToString ();
			string sprice =item.price==0?"": item.price.ToString ("n2");

			view.FindViewById<TextView> (Resource.Id.invitemcode).Text = item.icode;
			view.FindViewById<TextView> (Resource.Id.invitemdesc).Text = item.description;
			view.FindViewById<TextView> (Resource.Id.invitemqty).Text = sqty;
			view.FindViewById<TextView> (Resource.Id.invitemtaxgrp).Text = item.taxgrp;
			if (item.icode == "TAX" || item.icode == "AMOUNT") {
				view.FindViewById<TextView> (Resource.Id.invitemtax).Text = "";
			}else view.FindViewById<TextView> (Resource.Id.invitemtax).Text = item.tax.ToString ("n2");
			view.FindViewById<TextView> (Resource.Id.invitemprice).Text = sprice;
			view.FindViewById<TextView> (Resource.Id.invitemamt).Text = item.netamount.ToString ("n2");


		}

		void OnListItemClick(object sender, AdapterView.ItemClickEventArgs e) {
			InvoiceDtls item = listData.ElementAt (e.Position);
			if (item.icode.IndexOf ("TAX") > -1)
				return;
			if (item.icode.IndexOf ("AMOUNT") > -1)
				return;

			PopupMenu menu = new PopupMenu (e.Parent.Context, e.View);
			menu.Inflate (Resource.Menu.popupItem);
			menu.Menu.RemoveItem (Resource.Id.popadd);
			menu.Menu.RemoveItem (Resource.Id.popedit);
			//			if (!rights.InvAllowDelete) {
			//				menu.Menu.RemoveItem (Resource.Id.popdelete);
			//			}

			menu.MenuItemClick += (s1, arg1) => {
				if (arg1.Item.ItemId==Resource.Id.popdelete)
				{
					Delete(item);
				}

			};
			menu.Show ();

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
					var arrlist= listData.Where(x=>x.invno==invdtls.invno&& x.ID==invdtls.ID).ToList<InvoiceDtls>();
					if (arrlist.Count > 0) {
						listData.Remove (arrlist [0]);
						SetViewDlg viewdlg = SetViewDelegate;
						listView.Adapter = new GenericListAdapter<InvoiceDtls> (this, listData, Resource.Layout.InvDtlItemViewCS, viewdlg);
					}
				}
			}
		}

		#endregion Item ListView stuff

		#region Loading Data
		void populate(List<InvoiceDtls> list)
		{
			list.Clear();
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				var list2 = db.Table<InvoiceDtls>().Where(x=>x.invno==inv.invno).ToList<InvoiceDtls>();
		
				ttlamt = 0;
				ttltax = 0;
				foreach(var item in list2)
				{
					ttlamt = ttlamt + item.netamount;
					ttltax = ttltax + item.tax;
					list.Add(item);
				}

				InvoiceDtls inv1 = new InvoiceDtls ();
				inv1.icode = "TAX";
				inv1.netamount = ttltax;
				InvoiceDtls inv2 = new InvoiceDtls ();
				inv2.icode = "AMOUNT";
				inv2.netamount = ttlamt;

				list.Add (inv1);
				list.Add (inv2);
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
			foreach (Item item in items) {
				if (item.IDesc.Length > 40) {
					icodes.Add (item.ICode + " | " + item.IDesc.Substring(0,40)+"...");
				}else icodes.Add (item.ICode + " | " + item.IDesc);
			}

			custcodes = new List<string> ();
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
			ShowKeyBoard (qty as View);

		}

		private void spinnerCust_ItemSelected (object sender, AdapterView.ItemSelectedEventArgs e)
		{
			Spinner spinner = (Spinner)sender;

			string txt = spinner.GetItemAtPosition (e.Position).ToString();
			string[] codes = txt.Split (new char[]{ '|' });
			if (codes.Length == 0)
				return;

			trd  =trds.Where (x => x.CustCode ==codes[0].Trim()).FirstOrDefault ();
			if (trd == null) {
				trd = new Trader ();
			}

			if (codes [0].Trim () == "COD" || codes [0].Trim () == "CASH") {
				txtcustname.Enabled = true;
			}else txtcustname.Enabled =false;
			txtcustname.Text = trd.CustName.Trim ();
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
				View view = sender as View;
				InputMethodManager imm = (InputMethodManager)GetSystemService(Context.InputMethodService);
				imm.HideSoftInputFromWindow(view.WindowToken, 0);
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
			txtbarcode.Text = "";
			txtbarcode.RequestFocus ();
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
			listView.Adapter = new GenericListAdapter<InvoiceDtls> (this, listData, Resource.Layout.InvDtlItemViewCS, viewdlg);
			listView.SetSelection (listView.Count - 1);
		}

		private void butAddClick(object sender,EventArgs e)
		{
			GetControls ();
			//TextView txtInvNo =  FindViewById<TextView> (Resource.Id.txtInvnp);
			TextView txttax =  FindViewById<TextView> (Resource.Id.txttax);
			EditText txtttlamt = FindViewById<EditText> (Resource.Id.txtamount);
			EditText txtttltax = FindViewById<EditText> (Resource.Id.txttaxamt);

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
				InvoiceDtls invdtl = new InvoiceDtls ();
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
				db.Insert (invdtl);
			}

			//spinner.SetSelection (-1);
			txtqty.Text = "";
			//txtprice.Text = ""; 
			txtttltax.Text = "";
			txtttlamt.Text = "";
			RefreshItemList ();
		}

		void ButPaid_Click (object sender, EventArgs e)
		{
			if (spinCust.SelectedItem == null) {
				Toast.MakeText (this, Resources.GetString(Resource.String.msg_invalidcust), ToastLength.Long).Show ();			
				spinCust.RequestFocus ();
				return;			
			}
			if (ttlamt == 0) {
				return;
			}
			ShowPaidment ();
		}

		void ShowPaidment()
		{
			var dialog = CashDialog.NewInstance();
			dialog.Amount =ttlamt+ttltax;
			dialog.Show(FragmentManager, "dialogPaid");
		}

		void CancelReceipt()
		{
			DeleteInv ();
			ttlamt = 0;
			ttltax = 0;
			inv = new Invoice ();
			ClearData ();
			EnableControLs (false, false, false, false, true);
			RefreshItemList ();
		}

		void Printreceipt()
		{
			InvoiceDtls[] list;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)){
				var ls= db.Table<InvoiceDtls> ().Where (x => x.invno==inv.invno).ToList<InvoiceDtls>();
				list = new InvoiceDtls[ls.Count];
				ls.CopyTo (list);
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
			if (IsSave)
				return;
			
			string[] codes = spinCust.SelectedItem.ToString().Split (new char[]{ '|' });
			if (codes.Length == 0) {
				Toast.MakeText (this, Resources.GetString (Resource.String.msg_invalidcust), ToastLength.Long).Show ();
				return;
			}

			EnableControLs(false, false, true, false, true);
			inv.description = txtcustname.Text.Trim ();//  codes [1].Trim ();//custname.Text;
			inv.amount = ttlamt;
			inv.taxamt = ttltax;
			inv.custcode = codes [0].Trim ();

			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var temp = db.Table<Invoice> ().Where (x => x.invno == inv.invno).ToList ();
				if (temp.Count > 0) {
					temp [0].description = txtcustname.Text.Trim ();
					temp [0].amount = ttlamt;
					temp [0].taxamt = ttltax;
					temp [0].custcode = codes [0].Trim ();
					db.Update (temp [0]);
				} else	db.Insert (inv);


				IsCashPay = true;
				IsSave = true;
				Printreceipt ();
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

		private void NewBill()
		{
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
			base.OnBackPressed();
		}

		private void DeleteInv()
		{
			try {
				using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
					var list = db.Table<InvoiceDtls> ().Where (x => x.invno == invno).ToList<InvoiceDtls> ();
					var list2 = db.Table<Invoice> ().Where (x => x.invno == invno).ToList<Invoice> ();
					if (list2.Count > 0) {
						string trxtype = "CS";
						AdNumDate adNum = DataHelper.GetNumDate (pathToDatabase, list2 [0].invdate, trxtype);
						if (invno.Length > 5) {
							string snum = invno.Substring (invno.Length - 4);					
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
			case EventID.CUSTCODE_SELECTED:
				RunOnUiThread (() => SetSelectedCust(e.Param["SELECTED"].ToString()));
				break;
			case EventID.ICODE_SELECTED:
				RunOnUiThread (() => SetSelectedItem(e.Param["SELECTED"].ToString()));
				break;
			case EventID.PAYMENT_PAID:
				if (!IsFirePaid) {
					IsFirePaid = true;
					RunOnUiThread (() => SaveCashBill ());
				}
				break;
			
			}
		}
		#endregion Event
	}
}

