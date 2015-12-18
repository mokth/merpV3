
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
	public class CashItemActivity : Activity
	{
		ListView listView ;
		List<InvoiceDtls> listData = new List<InvoiceDtls> ();
		Invoice invoice ;
		Trader  trd;
		string pathToDatabase;
		string invno ="";
		string CUSTCODE ="";
		string CUSTNAME ="";

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
		
			invno = Intent.GetStringExtra ("invoiceno") ?? "AUTO";
			CUSTCODE = Intent.GetStringExtra ("custcode") ?? "AUTO";
		

			SetTitle (Resource.String.title_cashitems);
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			COMPCODE = ((GlobalvarsApp)this.Application).COMPANY_CODE;
			rights = Utility.GetAccessRights (pathToDatabase);



			isNotAllowEditAfterPrinted  = DataHelper.GetInvoicePrintStatus (pathToDatabase,invno,rights);
			txtbarcode = FindViewById<EditText> (Resource.Id.txtbarcode);
			txtbarcode.Visibility = ViewStates.Gone;
			Button butNew= FindViewById<Button> (Resource.Id.butnewItem); 
			butNew.Visibility = ViewStates.Gone;


			Button butInvBack= FindViewById<Button> (Resource.Id.butInvItmBack); 
			butInvBack.Click += (object sender, EventArgs e) => {
				Intent intent = ActivityManager.GetActivity<CashBillListing> (this.ApplicationContext);
				StartActivity (intent);
			};

			GetData ();
			populate (listData);
			listView = FindViewById<ListView> (Resource.Id.invitemList);
			SetViewDlg viewdlg = SetViewDelegate;
			listView.Adapter = new GenericListAdapter<InvoiceDtls> (this, listData, Resource.Layout.InvDtlItemViewCS, viewdlg);

		}
		public override void OnBackPressed() {
			// do nothing.
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

		protected override void OnResume()
		{
			base.OnResume();
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			COMPCODE = ((GlobalvarsApp)this.Application).COMPANY_CODE;
			rights = Utility.GetAccessRights (pathToDatabase);

			invno = Intent.GetStringExtra ("invoiceno") ?? "AUTO";
			CUSTCODE = Intent.GetStringExtra ("custcode") ?? "AUTO";
			isNotAllowEditAfterPrinted  = DataHelper.GetInvoicePrintStatus (pathToDatabase,invno,rights);

			listData = new List<InvoiceDtls> ();
			GetData ();
			populate (listData);
			listView = FindViewById<ListView> (Resource.Id.invitemList);
			SetViewDlg viewdlg = SetViewDelegate;
			listView.Adapter = new GenericListAdapter<InvoiceDtls> (this, listData, Resource.Layout.InvDtlItemViewCS, viewdlg);
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
				list.Add (inv4);
				list.Add (inv5);
			
			}
		}

	}
}

