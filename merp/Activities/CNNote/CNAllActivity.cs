﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Bluetooth;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.IO;
using SQLite;
using System.Threading;
using Java.Util;

namespace wincom.mobile.erp
{
	[Activity (Label = "CREDIT NOTE LIST",Icon="@drawable/bill")]			
	public class CNAllActivity : Activity,IEventListener
	{
		ListView listView ;
		List<CNNote> listData = new List<CNNote> ();
		string pathToDatabase;
		AdPara apara=null;
		CompanyInfo compinfo;
		DateTime sdate;
		DateTime edate;
		AccessRights rights;
		EditText txtSearch;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!((GlobalvarsApp)this.Application).ISLOGON) {
				Finish ();
			}
			SetTitle (Resource.String.submenu_cnlist);
			EventManagerFacade.Instance.GetEventManager().AddListener(this);
			// Create your application here
			//SetContentView (Resource.Layout.ListView);
			SetContentView (Resource.Layout.ListCustView);
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			rights = Utility.GetAccessRights (pathToDatabase);

			Utility.GetDateRange (ref sdate,ref edate);
			populate (listData);
			apara =  DataHelper.GetAdPara (pathToDatabase);
			listView = FindViewById<ListView> (Resource.Id.CustList);
			txtSearch= FindViewById<EditText > (Resource.Id.txtSearch);
//			TableLayout tlay = FindViewById<TableLayout> (Resource.Id.tableLayout1);
//			tlay.Visibility = ViewStates.Invisible;
			//Button butNew= FindViewById<Button> (Resource.Id.butCustBack); 
			//butNew.Visibility = ViewStates.Invisible;
			Button butInvBack= FindViewById<Button> (Resource.Id.butCustBack); 
			butInvBack.Click+= (object sender, EventArgs e) => {
				StartActivity(typeof(TransListActivity));
			};

			listView.ItemClick += OnListItemClick;
			listView.ItemLongClick += OnListItemLongClick;
			//listView.Adapter = new CusotmListAdapter(this, listData);
			SetViewDlg viewdlg = SetViewDelegate;
			listView.Adapter = new GenericListAdapter<CNNote> (this, listData, Resource.Layout.ListItemRowCN, viewdlg);
			txtSearch.TextChanged+= TxtSearch_TextChanged;
		}

		private void SetViewDelegate(View view,object clsobj)
		{
			CNNote item = (CNNote)clsobj;
			string trxtype = "";
			if (item.trxtype=="CASH") 
				trxtype = "CS";
			else if (item.trxtype=="INVOICE") 
				trxtype = "INV";
			else trxtype = "CN";
			view.FindViewById<TextView> (Resource.Id.invdate).Text = item.invdate.ToString ("dd-MM-yy");
			view.FindViewById<TextView> (Resource.Id.invno).Text = item.cnno;
			//view.FindViewById<TextView> (Resource.Id.trxtype).Text = trxtype;//item.trxtype;
			view.FindViewById<TextView>(Resource.Id.invcust).Text = item.description;
			//view.FindViewById<TextView> (Resource.Id.Amount).Text = item.amount.ToString("n2");
			view.FindViewById<TextView> (Resource.Id.TaxAmount).Text = item.taxamt.ToString("n2");
			double ttl = item.amount + item.taxamt;
			view.FindViewById<TextView> (Resource.Id.TtlAmount).Text =ttl.ToString("n2");
			ImageView img = view.FindViewById<ImageView> (Resource.Id.printed);
			if (!item.isPrinted)
				img.SetImageDrawable (null);  //.Visibility = ViewStates.Invisible;

			if (!string.IsNullOrEmpty (item.remark)) {
				view.FindViewById<LinearLayout> (Resource.Id.linearLayoutRmark).Visibility = ViewStates.Visible;
				view.FindViewById<TextView> (Resource.Id.invremark).Text = item.remark.ToUpper ();
			}
		}

		protected override void OnResume()
		{
			base.OnResume();
			if (txtSearch.Text != "") {
				FindItemByText ();
				return;
			}
			listData = new List<CNNote> ();
			populate (listData);
			apara =  DataHelper.GetAdPara (pathToDatabase);
			listView = FindViewById<ListView> (Resource.Id.CustList);
			SetViewDlg viewdlg = SetViewDelegate;
			listView.Adapter = new GenericListAdapter<CNNote> (this, listData, Resource.Layout.ListItemRowCN, viewdlg);
		}

		void OnListItemClick(object sender, AdapterView.ItemClickEventArgs e) {
			CNNote item = listData.ElementAt (e.Position);
			//var intent = new Intent(this, typeof(CNItemHisActivity));
			var intent =ActivityManager.GetActivity<CNItemHisActivity>(this.ApplicationContext);
			intent.PutExtra ("invoiceno",item.cnno );
			intent.PutExtra ("custcode",item.custcode );
			StartActivity(intent);
		}

		void OnListItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e) {
			CNNote item = listData.ElementAt (e.Position);
			PopupMenu menu = new PopupMenu (e.Parent.Context, e.View);
			menu.Inflate (Resource.Menu.popupHis);
			if (!rights.CNAllowPrint) {
				menu.Menu.RemoveItem (Resource.Id.popInvprint);
				menu.Menu.RemoveItem (Resource.Id.popInvprint2);
			}
			menu.MenuItemClick += (s1, arg1) => {
				
				if (arg1.Item.ItemId == Resource.Id.popInvprint) {
					PrintInv (item,1);	
				}else if (arg1.Item.ItemId == Resource.Id.popInvprint2) {
					PrintInv (item,2);	
				}else if (arg1.Item.ItemId== Resource.Id.popInvfilter) {
					ShowDateRangeLookUp();
				}  
			};
			menu.Show ();
		}

		void ShowDateRangeLookUp()
		{
			var intent = new Intent (this, typeof(DateRange));
			intent.PutExtra ("eventid", "2021");
			StartActivity (intent);
		}

		void populate(List<CNNote> list)
		{
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				var list2 = db.Table<CNNote>()
					.Where(x=>x.isUploaded==true&&x.invdate>=sdate&&x.invdate<=edate)
					.OrderByDescending (x => x.invdate)
					.ThenByDescending (x => x.created)
					.ToList<CNNote>();
				foreach(var item in list2)
				{
					list.Add(item);
				}

			}
			compinfo = DataHelper.GetCompany (pathToDatabase);
		}


		void PrintInv(CNNote inv,int noofcopy)
		{
			//Toast.MakeText (this, "print....", ToastLength.Long).Show ();	
			CNNoteDtls[] list;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)){
				var ls= db.Table<CNNoteDtls> ().Where (x => x.cnno==inv.cnno).ToList<CNNoteDtls>();
				list = new CNNoteDtls[ls.Count];
				ls.CopyTo (list);
			}
			IPrintDocument prtInv = PrintDocManager.GetPrintCNDocument<PrintCreditNote>();
			prtInv.SetDocument (inv);
			prtInv.SetDocumentDtls(list);
			prtInv.SetNoOfCopy (noofcopy);
			prtInv.SetCallingActivity (this);
			if (prtInv.StartPrint ()) {
				updatePrintedStatus (inv);
				var found =listData.Where (x => x.cnno == inv.cnno).ToList ();
				if (found.Count > 0) {
					found [0].isPrinted = true;
					SetViewDlg viewdlg = SetViewDelegate;
					listView.Adapter = new GenericListAdapter<CNNote> (this, listData, Resource.Layout.ListItemRowCN, viewdlg);
				}
			}
		}

		void updatePrintedStatus(CNNote inv)
		{
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list = db.Table<CNNote> ().Where (x => x.cnno == inv.cnno).ToList<CNNote> ();
				if (list.Count > 0) {
					var list2 = db.Table<CNNoteDtls> ().Where (x => x.cnno == inv.cnno).ToList<CNNoteDtls> ();
					if (list2.Count > 0) {
						list [0].isPrinted = true;
						db.Update (list [0]);
					}
				}
			}
		}
		void TxtSearch_TextChanged (object sender, Android.Text.TextChangedEventArgs e)
		{
			FindItemByText ();
		}

		void FindItemByText ()
		{
			SetViewDlg viewdlg = SetViewDelegate;
			List<CNNote> found = PerformSearch (txtSearch.Text);
			listView.Adapter = new GenericListAdapter<CNNote> (this, found, Resource.Layout.ListItemRowCN, viewdlg);
		}

		List<CNNote> PerformSearch (string constraint)
		{
			List<CNNote>  results = new List<CNNote>();
			if (constraint != null) {
				var searchFor = constraint.ToString ().ToUpper();

				foreach(CNNote itm in listData)
				{
					if (itm.cnno.ToUpper ().IndexOf (searchFor) >= 0) {
						results.Add (itm);
						continue;
					}else if (itm.invno.ToUpper().IndexOf (searchFor) >= 0) {
						results.Add (itm);
						continue;
					}else if (itm.custcode.ToUpper().IndexOf (searchFor) >= 0) {
						results.Add (itm);
						continue;
					}else if (itm.description.ToUpper().IndexOf (searchFor) >= 0) {
						results.Add (itm);
						continue;
					}else if (itm.remark.ToUpper().IndexOf (searchFor) >= 0) {
						results.Add (itm);
						continue;
					}
					else if (itm.invdate.ToString("dd-MM-yyyy").IndexOf (searchFor) >= 0) {
						results.Add (itm);
						continue;
					}

				}
			}
			return results;
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
			case 2021:
				sdate =Utility.ConvertToDate(e.Param ["DATE1"].ToString ());
				edate=Utility.ConvertToDate(e.Param ["DATE2"].ToString ());
				break;
			}
		}

	}
}

