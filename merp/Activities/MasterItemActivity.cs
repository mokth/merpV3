﻿using System;
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
	[Activity (Label = "MASTER ITEM CODE",Icon="@drawable/item")]			
	public class MasterItemActivity : Activity
	{
		ListView listView ;
		List<Item> listData = new List<Item> ();
		string pathToDatabase;
		GenericListAdapter<Item> adapter; 
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
			SetTitle (Resource.String.submenu_item);
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

			adapter = new GenericListAdapter<Item> (this, listData, Resource.Layout.ItemCodeDtlList, viewdlg);
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
			List<Item> found = new List<Item> ();
			if (string.IsNullOrEmpty(classcode))
				found = PerformSearch (txtSearch.Text);
			else found = PerformSearch (classcode);

			//List<Item> found = PerformSearch (txtSearch.Text);
			adapter = new GenericListAdapter<Item> (this, found, Resource.Layout.ItemCodeDtlList, viewdlg);
			listView.Adapter = adapter;
		}

		void TxtSearch_TextChanged (object sender, Android.Text.TextChangedEventArgs e)
		{
			FindItemByText ("");
		}

		void ListView_Click (object sender, AdapterView.ItemClickEventArgs e)
		{
			
			Item itm = adapter[e.Position];// listData.ElementAt (e.Position);
			var intent = new Intent (this, typeof(ItemActivity));
			intent.PutExtra ("icode", itm.ICode);
			StartActivity (intent);
		}

		private void SetViewDelegate(View view,object clsobj)
	    {
			Item item = (Item)clsobj;
			view.FindViewById<TextView> (Resource.Id.icodecode).Text = item.ICode;
			view.FindViewById<TextView> (Resource.Id.icodedesc).Text = item.IDesc;
			view.FindViewById<TextView> (Resource.Id.icodeprice).Text = item.Price.ToString ("n3");
			view.FindViewById<TextView> (Resource.Id.icodetax).Text = item.taxgrp;
			view.FindViewById<TextView> (Resource.Id.icodetaxper).Text = item.tax.ToString ("n2");
			view.FindViewById<TextView> (Resource.Id.icodeinclusive).Text = (item.isincludesive)?"INC":"EXC";
		}
		protected override void OnResume()
		{
			base.OnResume();
			if (txtSearch.Text != "") {
				FindItemByText ("");
				return;
			}
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			listData = new List<Item> ();
			populate (listData);
			listView = FindViewById<ListView> (Resource.Id.ICodeList);
			SetViewDlg viewdlg = SetViewDelegate;
			listView.Adapter = new GenericListAdapter<Item> (this, listData, Resource.Layout.ItemCodeDtlList, viewdlg);
		}
		void populate(List<Item> list)
		{
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{

				var list2 = db.Table<Item>().ToList<Item>();
				foreach(var item in list2)
				{
					list.Add(item);
				}
				var grpclass = list2.GroupBy (x => x.Class);
				foreach (var grp in grpclass) {
					classcodes.Add(grp.Key);
				}
			}
		}

		List<Item> PerformSearch (string constraint)
		{
			List<Item>  results = new List<Item>();
			if (constraint != null) {
				var searchFor = constraint.ToString ().ToUpper();
				Console.WriteLine ("searchFor:" + searchFor);

				foreach(Item itm in listData)
				{
					if (itm.ICode.ToUpper().IndexOf (searchFor) >= 0) {
						results.Add (itm);
						continue;
					}

					if (itm.IDesc.ToUpper().IndexOf (searchFor) >= 0) {
						results.Add (itm);
						continue;
					}

					if (itm.Class.ToUpper().IndexOf (searchFor) >= 0) {
						results.Add (itm);
						continue;
					}
				}


			}
			return results;
		}

//		List<object> PerformFiltering (string constraint)
//		{
//			List<object>  results = new List<object>();
//			if (constraint != null) {
//				var searchFor = constraint.ToString ().ToUpper();
//				Console.WriteLine ("searchFor:" + searchFor);
//				var matchList = new List<Item>();
//
//				foreach(Item itm in listData)
//				{
//					if (itm.ICode.ToUpper().IndexOf (searchFor) >= 0) {
//						results.Add (itm);
//						continue;
//					}
//
//					if (itm.IDesc.ToUpper().IndexOf (searchFor) >= 0) {
//						results.Add (itm);
//						continue;
//					}
//				}
//
////				//a.MatchItems = matchList.ToArray ();
////				//Console.WriteLine ("resultCount:" + matchList.Count);
////
////				Java.Lang.Object[] matchObjects;
////				matchObjects = new Java.Lang.Object[matchList.Count];
////				for (int i = 0; i < matchList.Count; i++) {
////					matchObjects[i] = new JavaObject<Item>(matchList[i]);
////				}
////
////				results.Values = matchObjects;
////				results.Count = matchList.Count;
//			}
//			return results;
//		}

	
	}
}

