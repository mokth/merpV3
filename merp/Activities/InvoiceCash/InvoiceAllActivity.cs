using System;
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
	[Activity (Label = "INVOICE LIST",Icon="@drawable/shop")]			
	public class InvoiceAllActivity : Activity,IEventListener
	{
		ListView listView ;
		List<Invoice> listData = new List<Invoice> ();
		string pathToDatabase;

		AdPara apara=null;
		CompanyInfo compinfo;
		DateTime sdate;
		DateTime edate;
		AccessRights rights;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!((GlobalvarsApp)this.Application).ISLOGON) {
				Finish ();
			}
			SetTitle (Resource.String.submenu_invlist);
			EventManagerFacade.Instance.GetEventManager().AddListener(this);
			// Create your application here
			SetContentView (Resource.Layout.ListView);
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			rights = Utility.GetAccessRights (pathToDatabase);

			Utility.GetDateRange (ref sdate,ref edate);
			populate (listData);
			apara =  DataHelper.GetAdPara (pathToDatabase);
			listView = FindViewById<ListView> (Resource.Id.feedList);
//			TableLayout tlay = FindViewById<TableLayout> (Resource.Id.tableLayout1);
//			tlay.Visibility = ViewStates.Invisible;
			Button butNew= FindViewById<Button> (Resource.Id.butnewInv); 
			butNew.Visibility = ViewStates.Invisible;
			Button butInvBack= FindViewById<Button> (Resource.Id.butInvBack); 
			butInvBack.Click+= (object sender, EventArgs e) => {
				StartActivity(typeof(TransListActivity));
			};

			listView.ItemClick += OnListItemClick;
			listView.ItemLongClick += OnListItemLongClick;
			//listView.Adapter = new CusotmListAdapter(this, listData);
			SetViewDlg viewdlg = SetViewDelegate;
			listView.Adapter = new GenericListAdapter<Invoice> (this, listData, Resource.Layout.ListItemRow, viewdlg);

		}

		private void SetViewDelegate(View view,object clsobj)
		{
			Invoice item = (Invoice)clsobj;
			view.FindViewById<TextView> (Resource.Id.invdate).Text = item.invdate.ToString ("dd-MM-yy");
			view.FindViewById<TextView> (Resource.Id.invno).Text = item.invno;
			view.FindViewById<TextView> (Resource.Id.trxtype).Text = item.trxtype;
			view.FindViewById<TextView>(Resource.Id.invcust).Text = item.description;
			//view.FindViewById<TextView> (Resource.Id.Amount).Text = item.amount.ToString("n2");
			view.FindViewById<TextView> (Resource.Id.TaxAmount).Text = item.taxamt.ToString("n2");
			double ttl = item.amount + item.taxamt;
			view.FindViewById<TextView> (Resource.Id.TtlAmount).Text =ttl.ToString("n2");
			ImageView img = view.FindViewById<ImageView> (Resource.Id.printed);
			if (!item.isPrinted)
				img.Visibility = ViewStates.Invisible;
		}

		protected override void OnResume()
		{
			base.OnResume();
			listData = new List<Invoice> ();
			populate (listData);
			apara =  DataHelper.GetAdPara (pathToDatabase);
			listView = FindViewById<ListView> (Resource.Id.feedList);
			SetViewDlg viewdlg = SetViewDelegate;
			listView.Adapter = new GenericListAdapter<Invoice> (this, listData, Resource.Layout.ListItemRow, viewdlg);
		}

		void OnListItemClick(object sender, AdapterView.ItemClickEventArgs e) {
			Invoice item = listData.ElementAt (e.Position);
			var intent = new Intent(this, typeof(InvItemHisActivity));
			intent.PutExtra ("invoiceno",item.invno );
			intent.PutExtra ("custcode",item.custcode );
			StartActivity(intent);
		}

		void OnListItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e) {
			Invoice item = listData.ElementAt (e.Position);
			PopupMenu menu = new PopupMenu (e.Parent.Context, e.View);
			menu.Inflate (Resource.Menu.popupHis);
			if (!rights.InvAllowPrint) {
				menu.Menu.RemoveItem (Resource.Id.popInvprint);
				menu.Menu.RemoveItem (Resource.Id.popInvprint2);
			}
			menu.MenuItemClick += (s1, arg1) => {
				
				if (arg1.Item.ItemId == Resource.Id.popInvprint) {
					PrintInv (item,1);	
				}else if (arg1.Item.ItemId == Resource.Id.popInvprint2) {
					PrintInv (item,2);	
				} 
				else if (arg1.Item.ItemId == Resource.Id.popInvfilter) {
					ShowDateRangeLookUp();
				} 
			};
			menu.Show ();
		}

		void ShowDateRangeLookUp()
		{
			var intent = new Intent (this, typeof(DateRange));
			intent.PutExtra ("eventid", "2020");
			StartActivity (intent);
		}

		void populate(List<Invoice> list)
		{
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				var list2 = db.Table<Invoice>()
					.Where(x=>x.isUploaded==true&&x.invdate>=sdate&&x.invdate<=edate)
					.OrderByDescending (x => x.invno)
					.ToList<Invoice>();
				foreach(var item in list2)
				{
					list.Add(item);
				}

			}
			compinfo = DataHelper.GetCompany (pathToDatabase);
		}


		void PrintInv(Invoice inv,int noofcopy)
		{
			//Toast.MakeText (this, "print....", ToastLength.Long).Show ();	
			InvoiceDtls[] list;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)){
				var ls= db.Table<InvoiceDtls> ().Where (x => x.invno==inv.invno).ToList<InvoiceDtls>();
				list = new InvoiceDtls[ls.Count];
				ls.CopyTo (list);
			}

			IPrintDocument prtInv = PrintDocManager.GetPrintDocument<PrintInvoice>();
			prtInv.SetDocument (inv);
			prtInv.SetDocumentDtls(list);
			prtInv.SetNoOfCopy (noofcopy);
			prtInv.SetCallingActivity (this);
			if (prtInv.StartPrint ()) {
				updatePrintedStatus (inv);
				var found = listData.Where (x => x.invno == inv.invno).ToList ();
				if (found.Count > 0) {
					found [0].isPrinted = true;
					SetViewDlg viewdlg = SetViewDelegate;
					listView.Adapter = new GenericListAdapter<Invoice> (this, listData, Resource.Layout.ListItemRow, viewdlg);
				}
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

		public event nsEventHandler eventHandler;

		public void FireEvent(object sender,EventParam eventArgs)
		{
			if (eventHandler != null)
				eventHandler (sender, eventArgs);
		}

		public void PerformEvent(object sender, EventParam e)
		{
			switch (e.EventID) {
			case 2020:
				sdate =Utility.ConvertToDate(e.Param ["DATE1"].ToString ());
				edate=Utility.ConvertToDate(e.Param ["DATE2"].ToString ());
				break;
			}
		}

	}
}

