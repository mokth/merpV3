
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
	[Activity (Label = "INVOICE ITEM(S)",Icon="@drawable/shop")]			
	public class InvItemActivity : Activity,Android.Views.View.IOnKeyListener
	{
		ListView listView ;
		List<InvoiceDtls> listData = new List<InvoiceDtls> ();
		Invoice invoice ;
		Trader  trd;
		string pathToDatabase;
		string invno ="";
		string CUSTCODE ="";
		string CUSTNAME ="";
		string EDITMODE="";
		string TRXTYPE;
		EditText txtbarcode;
		CompanyInfo comp;
		bool isNotAllowEditAfterPrinted  ;
		AccessRights rights;
		string COMPCODE;

		List<Item> items = null;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!((GlobalvarsApp)this.Application).ISLOGON) {
				Finish ();
			}
			SetContentView (Resource.Layout.InvDtlView);
			TRXTYPE= Intent.GetStringExtra ("trxtype") ?? "CASH";
			invno = Intent.GetStringExtra ("invoiceno") ?? "AUTO";
			CUSTCODE = Intent.GetStringExtra ("custcode") ?? "AUTO";
			EDITMODE = Intent.GetStringExtra ("editmode") ?? "AUTO";

			if (TRXTYPE == "CASH") {
				SetTitle (Resource.String.title_cashitems);
			} else SetTitle (Resource.String.title_invoiceitems);
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			COMPCODE = ((GlobalvarsApp)this.Application).COMPANY_CODE;
			rights = Utility.GetAccessRights (pathToDatabase);



			isNotAllowEditAfterPrinted  = DataHelper.GetInvoicePrintStatus (pathToDatabase,invno,rights);
			txtbarcode = FindViewById<EditText> (Resource.Id.txtbarcode);
			txtbarcode.Visibility = ViewStates.Visible;
			Button butNew= FindViewById<Button> (Resource.Id.butnewItem); 
			butNew.Click += (object sender, EventArgs e) => {
				NewItem(invno);
			};
			if (isNotAllowEditAfterPrinted)
				butNew.Enabled = false;
			if (!rights.InvAllowAdd)
				butNew.Enabled = false;

			Button butInvBack= FindViewById<Button> (Resource.Id.butInvItmBack); 
			butInvBack.Click += (object sender, EventArgs e) => {
				Invoice temp = new Invoice();
				UpdateInvoiceAmount(invno,ref temp);

				if (EDITMODE.ToLower()=="new")
				{
					DeleteInvWithEmptyInovItem();
				}
				Intent intent =null;
				if (temp.trxtype=="CASH"){
				 intent =ActivityManager.GetActivity<CashActivity>(this.ApplicationContext);
				}else  intent =ActivityManager.GetActivity<InvoiceActivity>(this.ApplicationContext);

				StartActivity(intent);

			};

			txtbarcode.SetOnKeyListener (this);
			txtbarcode.InputType = 0;
			GetData ();
			populate (listData);
			listView = FindViewById<ListView> (Resource.Id.invitemList);
			listView.ItemClick += OnListItemClick;
			//listView.Adapter = new CusotmItemListAdapter(this, listData);
			SetViewDlg viewdlg = SetViewDelegate;
			listView.Adapter = new GenericListAdapter<InvoiceDtls> (this, listData, Resource.Layout.InvDtlItemViewCS, viewdlg);


		}
		public override void OnBackPressed() {
			// do nothing.
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


		private void DeleteInvWithEmptyInovItem()
		{
			try{
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list = db.Table<InvoiceDtls>().Where(x=>x.invno==invno).ToList<InvoiceDtls>();
				if (list.Count == 0) {
				    var list2 = db.Table<Invoice>().Where(x=>x.invno==invno).ToList<Invoice>();
					if (list2.Count > 0) {
							string trxtype = list2[0].trxtype=="CASH"?"CS":"INV";
							AdNumDate adNum= DataHelper.GetNumDate (pathToDatabase, list2[0].invdate,trxtype);
						if (invno.Length > 5) {
							string snum= invno.Substring (invno.Length - 4);					
							int num;
							if (int.TryParse (snum, out num)) {
								if (adNum.RunNo == num) {
									adNum.RunNo = num - 1;
									db.Delete (list2[0]);
									db.Delete (adNum);
								}
							}
 						}
					}
					//db.Table<Invoice> ().Delete (x => x.invno == invno);
				}
			}
			}catch{
			}
		}

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

//		private void SetViewDelegate(View view,object clsobj)
//		{
//			InvoiceDtls item = (InvoiceDtls)clsobj;
//			string sqty =item.qty==0?"": item.qty.ToString ();
//			string sprice =item.price==0?"": item.price.ToString ("n2");
//
//			view.FindViewById<TextView> (Resource.Id.invitemcode).Text = item.icode;
//			view.FindViewById<TextView> (Resource.Id.invitemdesc).Text = item.description;
//			view.FindViewById<TextView> (Resource.Id.invitemqty).Text = sqty;
//			view.FindViewById<TextView> (Resource.Id.invitemtaxgrp).Text = item.taxgrp;
//			if (item.icode == "TAX" || item.icode == "AMOUNT") {
//				view.FindViewById<TextView> (Resource.Id.invitemtax).Text = "";
//			}else view.FindViewById<TextView> (Resource.Id.invitemtax).Text = item.tax.ToString ("n2");
//			view.FindViewById<TextView> (Resource.Id.invitemprice).Text = sprice;
//			view.FindViewById<TextView> (Resource.Id.invitemamt).Text = item.netamount.ToString ("n2");
//
//
//		}

		protected override void OnResume()
		{
			base.OnResume();
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			COMPCODE = ((GlobalvarsApp)this.Application).COMPANY_CODE;
			rights = Utility.GetAccessRights (pathToDatabase);
			TRXTYPE= Intent.GetStringExtra ("trxtype") ?? "CASH";
			invno = Intent.GetStringExtra ("invoiceno") ?? "AUTO";
			CUSTCODE = Intent.GetStringExtra ("custcode") ?? "AUTO";
			isNotAllowEditAfterPrinted  = DataHelper.GetInvoicePrintStatus (pathToDatabase,invno,rights);
			Button butNew= FindViewById<Button> (Resource.Id.butnewItem); 
			if (isNotAllowEditAfterPrinted)
				butNew.Enabled = false;
			
			listData = new List<InvoiceDtls> ();
			GetData ();
			populate (listData);
			listView = FindViewById<ListView> (Resource.Id.invitemList);
			SetViewDlg viewdlg = SetViewDelegate;
			listView.Adapter = new GenericListAdapter<InvoiceDtls> (this, listData, Resource.Layout.InvDtlItemViewCS, viewdlg);
		}

		void OnListItemClick(object sender, AdapterView.ItemClickEventArgs e) {
			InvoiceDtls item = listData.ElementAt (e.Position);
			if (item.icode.IndexOf ("TAX") > -1)
				return;

			if (item.icode.IndexOf ("AMOUNT") > -1)
				return;
			
			PopupMenu menu = new PopupMenu (e.Parent.Context, e.View);
			menu.Inflate (Resource.Menu.popupItem);
			if (!rights.InvAllowAdd) {
				menu.Menu.RemoveItem (Resource.Id.popadd);
			}
			if (!rights.InvAllowEdit) {
				menu.Menu.RemoveItem (Resource.Id.popedit);
			}
			if (!rights.InvAllowDelete) {
				menu.Menu.RemoveItem (Resource.Id.popdelete);
			}
			//if allow edit and Invoice printed, remove edit
			//printed invoice not allow to edit

			if (isNotAllowEditAfterPrinted) {
				menu.Menu.RemoveItem (Resource.Id.popedit);
				menu.Menu.RemoveItem (Resource.Id.popdelete);
				menu.Menu.RemoveItem (Resource.Id.popadd);
			}


			menu.MenuItemClick += (s1, arg1) => {
				if (arg1.Item.ItemId==Resource.Id.popadd)
				{
					NewItem(item.invno);
				}else if (arg1.Item.ItemId==Resource.Id.popedit)
				{
					Edit(item);
				} else if (arg1.Item.ItemId==Resource.Id.popdelete)
				{
					Delete(item);
				}
			
			};
			menu.Show ();

		}

		void Delete(InvoiceDtls inv)
		{
			var builder = new AlertDialog.Builder(this);
			builder.SetMessage(Resources.GetString(Resource.String.msg_confirmdelete)+"?");
			builder.SetPositiveButton("YES", (s, e) => { DeleteItem(inv); });
			builder.SetNegativeButton("Cancel", (s, e) => { /* do something on Cancel click */ });
			builder.Create().Show();
		}
		void DeleteItem(InvoiceDtls inv)
		{
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				var list = db.Table<InvoiceDtls>().Where(x=>x.invno==invno&& x.ID==inv.ID).ToList<InvoiceDtls>();
				if (list.Count > 0) {
					db.Delete (list [0]);
					var arrlist= listData.Where(x=>x.invno==invno&& x.ID==inv.ID).ToList<InvoiceDtls>();
					if (arrlist.Count > 0) {
						listData.Remove (arrlist [0]);
						SetViewDlg viewdlg = SetViewDelegate;
						listView.Adapter = new GenericListAdapter<InvoiceDtls> (this, listData, Resource.Layout.InvDtlItemViewCS, viewdlg);
					}
				}
			}
		}
		void Edit(InvoiceDtls inv)
		{
			//var intent = new Intent(this, typeof(EntryActivity));
			var intent =ActivityManager.GetActivity<EntryActivityEx>(this.ApplicationContext);
			intent.PutExtra ("invoiceno",inv.invno );
			intent.PutExtra ("trxtype",TRXTYPE );
			intent.PutExtra ("itemuid",inv.ID.ToString() );
			intent.PutExtra ("editmode","EDIT" );
			intent.PutExtra ("customer",CUSTNAME );
			intent.PutExtra ("custcode",CUSTCODE );
			StartActivity(intent);
		}

		private void NewItem(string invNo)
		{
			//var intent = new Intent(this, typeof(EntryActivity));
			var intent =ActivityManager.GetActivity<EntryActivityEx>(this.ApplicationContext);
			StartActivity(intent);
			intent.PutExtra ("invoiceno",invNo );
			intent.PutExtra ("trxtype",TRXTYPE );
			intent.PutExtra ("itemuid","-1");
			intent.PutExtra ("editmode","NEW" );
			intent.PutExtra ("customer",CUSTNAME );
			intent.PutExtra ("custcode",CUSTCODE );
			StartActivity(intent);
		}

		void GetData()
		{
			trd = DataHelper.GetTrader (pathToDatabase, CUSTCODE);
			invoice = DataHelper.GetInvoice(pathToDatabase, invno);
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				items = db.Table<Item> ().ToList<Item> ();
			}
		}
		void populate(List<InvoiceDtls> list)
		{

			comp = DataHelper.GetCompany (pathToDatabase);
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				//var list1 = db.Table<Invoice>().Where(x=>x.invno==invno).ToList<Invoice>();
				var list2 = db.Table<InvoiceDtls>().Where(x=>x.invno==invno).ToList<InvoiceDtls>();
				//var list3 = db.Table<Trader>().Where(x=>x.CustCode==CUSTCODE).ToList<Trader>();

				double ttlamt = 0;
				double ttltax = 0;
				if (trd!=null) {
					CUSTNAME = trd.CustName;
				}
				foreach(var item in list2)
				{
					ttlamt = ttlamt + item.netamount;
					ttltax = ttltax + item.tax;
					list.Add(item);
				}
				if (invoice!=null) {
					invoice.amount = ttlamt;
					db.Update (invoice);
				}
//				InvoiceDtls inv1 = new InvoiceDtls ();
//				inv1.icode = "TAX";
//				inv1.netamount = ttltax;
//				InvoiceDtls inv2 = new InvoiceDtls ();
//				inv2.icode = "AMOUNT";
//				inv2.netamount = ttlamt;
//
//				list.Add (inv1);
//				list.Add (inv2);
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
				InvoiceDtls inv4 = new InvoiceDtls ();
				inv4.icode = "AMOUNT";
				inv4.netamount =  roundVal;
				inv4.description = "ROUNDING ADJUST";
				InvoiceDtls inv5 = new InvoiceDtls ();
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


		private void AddBarCodeItem(Item prd )
		{
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
			inv.invno = invno;
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
				var list =db.Table<InvoiceDtls> ().Where (x => x.invno == invno && x.icode == prd.ICode).ToList ();
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

			listData = new List<InvoiceDtls> ();
			populate (listData);
			listView = FindViewById<ListView> (Resource.Id.invitemList);
			SetViewDlg viewdlg = SetViewDelegate;
			listView.Adapter = new GenericListAdapter<InvoiceDtls> (this, listData, Resource.Layout.InvDtlItemViewCS, viewdlg);
			listView.SetSelection (listView.Count - 1);
		}

		public void UpdateInvoiceAmount(string invno,ref Invoice inv)
		{
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var itemlist = db.Table<InvoiceDtls> ().Where (x => x.invno == invno);	
				double ttlamt= itemlist.Sum (x => x.netamount);
				double ttltax= itemlist.Sum (x => x.tax);
				var invlist =db.Table<Invoice> ().Where (x => x.invno == invno).ToList<Invoice> ();
				if (invlist.Count > 0) {
					invlist [0].amount = ttlamt;
					invlist [0].taxamt = ttltax;
					inv = invlist [0];
					db.Update (invlist [0]);
				}
			}
		}
	}
}

