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

namespace wincom.mobile.erp
{
	[Activity (Label = "SALES ORDER ITEM(S)",Icon="@drawable/sales")]			
	public class SOItemHisActivity : Activity
	{
		ListView listView ;
		List<SaleOrderDtls> listData = new List<SaleOrderDtls> ();
		string pathToDatabase;
		string invno ="";
		string CUSTCODE ="";
		string CUSTNAME ="";
		CompanyInfo comp;
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!((GlobalvarsApp)this.Application).ISLOGON) {
				Finish ();
			}
			SetTitle (Resource.String.title_soitems);
			SetContentView (Resource.Layout.InvDtlView);
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			invno = Intent.GetStringExtra ("invoiceno") ?? "AUTO";
			CUSTCODE = Intent.GetStringExtra ("custcode") ?? "AUTO";

//			TableLayout tlay = FindViewById<TableLayout> (Resource.Id.tableLayout1);
//			tlay.Visibility = ViewStates.Invisible;
			Button butNew= FindViewById<Button> (Resource.Id.butnewItem); 
			butNew.Visibility = ViewStates.Invisible;
			Button butInvBack= FindViewById<Button> (Resource.Id.butInvItmBack); 
			butInvBack.Click+= (object sender, EventArgs e) => {
				base.OnBackPressed();
			};
				
			populate (listData);
			listView = FindViewById<ListView> (Resource.Id.invitemList);
			SetViewDlg viewdlg = SetViewDelegate;
			listView.Adapter = new GenericListAdapter<SaleOrderDtls> (this, listData, Resource.Layout.InvDtlItemViewCS, viewdlg);
		}
	
//		private void SetViewDelegate(View view,object clsobj)
//		{
//			SaleOrderDtls item = (SaleOrderDtls)clsobj;
//			string sqty =item.qty==0?"": item.qty.ToString ();
//			string sprice =item.price==0?"": item.price.ToString ("n2");
//
//			view.FindViewById<TextView> (Resource.Id.invitemcode).Text = item.icode;
//			view.FindViewById<TextView> (Resource.Id.invitemdesc).Text = item.description;
//			view.FindViewById<TextView> (Resource.Id.invitemqty).Text = sqty;
//			view.FindViewById<TextView> (Resource.Id.invitemtaxgrp).Text = item.taxgrp;
//			view.FindViewById<TextView> (Resource.Id.invitemtax).Text = item.tax.ToString ("n2");
//			view.FindViewById<TextView> (Resource.Id.invitemprice).Text = sprice;
//			view.FindViewById<TextView> (Resource.Id.invitemamt).Text = item.netamount.ToString ("n2");
//
//
//		}

		private void SetViewDelegate(View view,object clsobj)
		{
			SaleOrderDtls item = (SaleOrderDtls)clsobj;
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
			invno = Intent.GetStringExtra ("invoiceno") ?? "AUTO";
			CUSTCODE = Intent.GetStringExtra ("custcode") ?? "AUTO";
			listData = new List<SaleOrderDtls> ();
			populate (listData);
			listView = FindViewById<ListView> (Resource.Id.invitemList);
			SetViewDlg viewdlg = SetViewDelegate;
			listView.Adapter = new GenericListAdapter<SaleOrderDtls> (this, listData, Resource.Layout.InvDtlItemViewCS, viewdlg);
		}
			
		void populate(List<SaleOrderDtls> list)
		{
			comp = DataHelper.GetCompany (pathToDatabase);
			//SqliteConnection.CreateFile(pathToDatabase);
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				var list1 = db.Table<SaleOrder>().Where(x=>x.sono==invno).ToList<SaleOrder>();
				var list2 = db.Table<SaleOrderDtls>().Where(x=>x.sono==invno).ToList<SaleOrderDtls>();
				var list3 = db.Table<Trader>().Where(x=>x.CustCode==CUSTCODE).ToList<Trader>();

				double ttlamt = 0;
				double ttltax = 0;
				if (list3.Count > 0) {
					CUSTNAME = list3 [0].CustName;
				}
				foreach(var item in list2)
				{
					ttlamt = ttlamt + item.netamount;
					ttltax = ttltax + item.tax;
					list.Add(item);
				}
				if (list1.Count > 0) {
					list1 [0].amount = ttlamt;
					db.Update (list1 [0]);
				}
				SaleOrderDtls inv1 = new SaleOrderDtls ();
				inv1.icode = "TAX";
				inv1.netamount = ttlamt;
				inv1.description = "TOTAL EXCL GST";
				SaleOrderDtls inv2 = new SaleOrderDtls ();
				inv2.icode = "AMOUNT";
				inv2.netamount = ttltax;
				inv2.description = "TOTAL 6% GST" ;
				SaleOrderDtls inv3 = new SaleOrderDtls ();
				inv3.icode = "TAX";
				inv3.netamount = ttlamt + ttltax;
				inv3.description = "TOTAL INCL GST" ;

				list.Add (inv1);
				list.Add (inv2);
				list.Add (inv3);
			}
		}
	}
}

