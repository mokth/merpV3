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
	[Activity (Label = "STOCK ITEM",Icon="@drawable/item")]			
	public class ItemStockActivity : Activity
	{
		ListView listView ;
		List<ItemStock> listData = new List<ItemStock> ();
		string pathToDatabase;
		GenericListAdapter<ItemStock> adapter; 
		EditText  txtSearch;
		SetViewDlg viewdlg;
		DateTime dateSearch;
		private InputMethodManager imm;
		const int DATE_DIALOG_ID1 = 0;
		DateTime date;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!((GlobalvarsApp)this.Application).ISLOGON) {
				Finish ();
			}
			SetTitle (Resource.String.submenu_item);
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			SetContentView (Resource.Layout.ItemStockList);
			imm = (InputMethodManager)GetSystemService(Context.InputMethodService);

			dateSearch = DateTime.Today;
			date = DateTime.Today;
			populate (listData);
			listView = FindViewById<ListView> (Resource.Id.ICodeList);
			txtSearch= FindViewById<EditText > (Resource.Id.txtSearch);
			Button butInvBack= FindViewById<Button> (Resource.Id.butICodeBack); 
			Button butDate= FindViewById<Button> (Resource.Id.butDate); 
			butDate.Click += delegate(object sender, EventArgs e) {
				//imm.HideSoftInputFromWindow(frd.WindowToken, 0);
				ShowDialog (DATE_DIALOG_ID1);
			};
			butInvBack.Click += (object sender, EventArgs e) => {
				base.OnBackPressed();
			};
			viewdlg = SetViewDelegate;
			adapter = new GenericListAdapter<ItemStock> (this, listData, Resource.Layout.ItemStockDtls, viewdlg);
		    listView.Adapter = adapter;
			listView.ItemClick+= ListView_Click;
			txtSearch.TextChanged+= TxtSearch_TextChanged;
		}

		protected override Dialog OnCreateDialog (int id)
		{
			switch (id) {
			case DATE_DIALOG_ID1:
				return new DatePickerDialog (this, OnDateSet1, date.Year, date.Month - 1, date.Day); 		
			}
			return null;
		}
		void OnDateSet1 (object sender, DatePickerDialog.DateSetEventArgs e)
		{
			dateSearch= e.Date;
			RefreshView();
		}

		void FindItemByText ()
		{
			List<ItemStock> found = PerformSearch (txtSearch.Text);
			adapter = new GenericListAdapter<ItemStock> (this, found, Resource.Layout.ItemStockDtls, viewdlg);
			listView.Adapter = adapter;
		}

		void TxtSearch_TextChanged (object sender, Android.Text.TextChangedEventArgs e)
		{
			FindItemByText ();
		}

		void ListView_Click (object sender, AdapterView.ItemClickEventArgs e)
		{
			
			ItemStock itm = adapter[e.Position];// listData.ElementAt (e.Position);
			var intent = new Intent (this, typeof(StockEntry));
			intent.PutExtra ("icode", itm.ICode);
			intent.PutExtra ("trxdate", itm.DateTrx.ToString("dd-MM-yyyy"));
			StartActivity (intent);
		}

		private void SetViewDelegate(View view,object clsobj)
	    {
			ItemStock item = (ItemStock)clsobj;
			view.FindViewById<TextView> (Resource.Id.date).Text = item.DateTrx.ToString("dd-MM-yyyy");
			view.FindViewById<TextView> (Resource.Id.icodedesc).Text = item.IDesc;
		    view.FindViewById<TextView> (Resource.Id.icode).Text =item.ICode;
			view.FindViewById<TextView> (Resource.Id.qtyrec).Text = item.QtyGR.ToString ();
			view.FindViewById<TextView> (Resource.Id.qtyactual).Text =item.QtyAct.ToString();
			view.FindViewById<TextView> (Resource.Id.qtysales).Text = item.QtySales.ToString ();
			view.FindViewById<TextView> (Resource.Id.qtybrf).Text = item.QtyBrf.ToString();
			view.FindViewById<TextView> (Resource.Id.qtyrtn).Text = item.QtyRtr.ToString();
			view.FindViewById<TextView> (Resource.Id.qtycrf).Text = item.QtyCrf.ToString();
		}

		protected override void OnResume()
		{
			base.OnResume();
			if (txtSearch.Text != "") {
				FindItemByText ();
				return;
			}
			RefreshView ();
		}

		void RefreshView(){
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			listData = new List<ItemStock> ();
			populate (listData);
			listView = FindViewById<ListView> (Resource.Id.ICodeList);
			SetViewDlg viewdlg = SetViewDelegate;
			listView.Adapter = new GenericListAdapter<ItemStock> (this, listData, Resource.Layout.ItemStockDtls,viewdlg);
		}

		void populate(List<ItemStock> list)
		{
			//SqliteConnection.CreateFile(pathToDatabase);
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				var list2 = db.Table<ItemStock>().Where(x=>x.DateTrx==dateSearch).ToList<ItemStock>();
				foreach(var item in list2)
				{
					list.Add(item);
				}
			}
		}

		List<ItemStock> PerformSearch (string constraint)
		{
			List<ItemStock>  results = new List<ItemStock>();
			if (constraint != null) {
				var searchFor = constraint.ToString ().ToUpper();
				Console.WriteLine ("searchFor:" + searchFor);

				foreach(ItemStock itm in listData)
				{
					if (itm.ICode.ToUpper().IndexOf (searchFor) >= 0) {
						results.Add (itm);
						continue;
					}

					if (itm.IDesc.ToUpper().IndexOf (searchFor) >= 0) {
						results.Add (itm);
						continue;
					}
				}
			}
			return results;
		}

	
	}
}

