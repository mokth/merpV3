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
	[Activity (Label = "ITEM LATEST PRICES LIST",Icon="@drawable/item")]			
	public class ItemPricesActivity : Activity
	{
		ListView listView ;
		List<ItemPrices> listData = new List<ItemPrices> ();
		string pathToDatabase;
		GenericListAdapter<ItemPrices> adapter; 
		EditText  txtSearch;
		SetViewDlg viewdlg;
		Spinner spinItem;
		ArrayAdapter<String> dAdapterItem;
		List<string> classcodes = new List<string> ();
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!((GlobalvarsApp)this.Application).ISLOGON) {
				Finish ();
			}
			SetTitle (Resource.String.submenu_itemprice);
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			SetContentView (Resource.Layout.ItemCodeList);
			populate (listData);
			listView = FindViewById<ListView> (Resource.Id.ICodeList);
			txtSearch= FindViewById<EditText > (Resource.Id.txtSearch);

			Button butInvBack= FindViewById<Button> (Resource.Id.butICodeBack); 
			butInvBack.Click += (object sender, EventArgs e) => {
				base.OnBackPressed();
			};
			SetupClassSpinner ();
			viewdlg = SetViewDelegate;
			//PerformFilteringDlg filterDlg=PerformFiltering;
			//listView.Adapter = new CusotmMasterItemListAdapter(this, listData);

			adapter = new GenericListAdapter<ItemPrices> (this, listData, Resource.Layout.ItemPriceList, viewdlg);
		    listView.Adapter = adapter;
			listView.ItemClick+= ListView_Click;
			txtSearch.TextChanged+= TxtSearch_TextChanged;
		}

		void SetupClassSpinner ()
		{
			spinItem = FindViewById<Spinner> (Resource.Id.txtClass);
			dAdapterItem = new ArrayAdapter<String> (this, Resource.Layout.spinner_item, classcodes);
			dAdapterItem.SetDropDownViewResource (Resource.Layout.SimpleSpinnerDropDownItemEx);
			spinItem.Adapter = dAdapterItem;
			spinItem.ItemSelected+= SpinItem_ItemSelected;
		}

		void SpinItem_ItemSelected (object sender, AdapterView.ItemSelectedEventArgs e)
		{
			Spinner spinner = (Spinner)sender;

			string selectclass = spinner.GetItemAtPosition (e.Position).ToString();
			if (string.IsNullOrEmpty(selectclass))
				return;
			FindItemByText (selectclass);
		}

		void FindItemByText (string classcode)
		{
			List<ItemPrices> found = new List<ItemPrices> ();
			if (string.IsNullOrEmpty(classcode))
				found = PerformSearch (txtSearch.Text,false);
			else found = PerformSearch (classcode,true);

			//List<Item> found = PerformSearch (txtSearch.Text);
			adapter = new GenericListAdapter<ItemPrices> (this, found, Resource.Layout.ItemPriceList, viewdlg);
			listView.Adapter = adapter;
		}

		void TxtSearch_TextChanged (object sender, Android.Text.TextChangedEventArgs e)
		{
			FindItemByText ("");
		}

		void ListView_Click (object sender, AdapterView.ItemClickEventArgs e)
		{
			
		
		}

		private void SetViewDelegate(View view,object clsobj)
	    {
			ItemPrices item = (ItemPrices)clsobj;
			view.FindViewById<TextView> (Resource.Id.icode).Text = item.ICode;
			view.FindViewById<TextView> (Resource.Id.itemdesc).Text = item.IDesc;
			view.FindViewById<TextView> (Resource.Id.custcode).Text = item.CustCode;
			view.FindViewById<TextView> (Resource.Id.custname).Text = item.CustName;
		   view.FindViewById<TextView> (Resource.Id.invdate).Text = item.InvDate.ToString("dd-MM-yyyy");
			view.FindViewById<TextView> (Resource.Id.price).Text =  item.Price.ToString ("n3");
		}
		protected override void OnResume()
		{
			base.OnResume();
			if (txtSearch.Text != "") {
				FindItemByText ("");
				return;
			}
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			listData = new List<ItemPrices> ();
			populate (listData);
			listView = FindViewById<ListView> (Resource.Id.ICodeList);
			SetViewDlg viewdlg = SetViewDelegate;
			listView.Adapter = new GenericListAdapter<ItemPrices> (this, listData, Resource.Layout.ItemPriceList, viewdlg);
		}
		void populate(List<ItemPrices> list)
		{
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{

				var list2 = db.Table<ItemPrices>().ToList<ItemPrices>();
				foreach(var item in list2)
				{
					list.Add(item);
				}

				var grpclass = list2.GroupBy (x => x.IClass);
				foreach (var grp in grpclass) {
					classcodes.Add(grp.Key);
				}
			}
		}

		List<ItemPrices> PerformSearch (string constraint,bool isIClass)
		{
			List<ItemPrices>  results = new List<ItemPrices>();
			if (constraint != null) {
				var searchFor = constraint.ToString ().ToUpper();
				Console.WriteLine ("searchFor:" + searchFor);

				foreach(ItemPrices itm in listData)
				{
					if (itm.IClass.ToUpper().IndexOf (searchFor) >= 0) {
						results.Add (itm);
						continue;
					}
					if (isIClass)
						continue;
					
					if (itm.ICode.ToUpper().IndexOf (searchFor) >= 0) {
						results.Add (itm);
						continue;
					}

					if (itm.IDesc.ToUpper().IndexOf (searchFor) >= 0) {
						results.Add (itm);
						continue;
					}

					if (itm.CustCode.ToUpper().IndexOf (searchFor) >= 0) {
						results.Add (itm);
						continue;
					}

					if (itm.CustName.ToUpper().IndexOf (searchFor) >= 0) {
						results.Add (itm);
						continue;
					}

					if (itm.Price.ToString("n3").IndexOf (searchFor) >= 0) {
						results.Add (itm);
						continue;
					}

					if (itm.InvDate.ToString("dd-MM-yyyy").IndexOf (searchFor) >= 0) {
						results.Add (itm);
						continue;
					}


				}


			}
			return results;
		}

	}
}

