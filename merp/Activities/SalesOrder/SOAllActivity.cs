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
	[Activity (Label = "SALES ORDER LIST",Icon="@drawable/sales")]			
	public class SOAllActivity : Activity
	{
		ListView listView ;
		List<SaleOrder> listData = new List<SaleOrder> ();
		string pathToDatabase;
		AdPara apara=null;
		CompanyInfo compinfo;
		AccessRights rights;
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!((GlobalvarsApp)this.Application).ISLOGON) {
				Finish ();
			}
			// Create your application here
			SetTitle (Resource.String.title_so);
			SetContentView (Resource.Layout.ListView);
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			rights = Utility.GetAccessRights (pathToDatabase);
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
			listView.Adapter = new GenericListAdapter<SaleOrder> (this, listData, Resource.Layout.ListItemRow, viewdlg);

		}

		private void SetViewDelegate(View view,object clsobj)
		{
			SaleOrder item = (SaleOrder)clsobj;
			view.FindViewById<TextView> (Resource.Id.invdate).Text = item.sodate.ToString ("dd-MM-yy");
			view.FindViewById<TextView> (Resource.Id.invno).Text = item.sono;
			view.FindViewById<TextView> (Resource.Id.trxtype).Text = item.custpono;
			view.FindViewById<TextView>(Resource.Id.invcust).Text = item.description;
			//view.FindViewById<TextView> (Resource.Id.Amount).Text = item.amount.ToString("n2");
			view.FindViewById<TextView> (Resource.Id.TaxAmount).Text = item.taxamt.ToString("n2");
			double ttl = item.amount + item.taxamt;
			view.FindViewById<TextView> (Resource.Id.TtlAmount).Text =ttl.ToString("n2");
		}

		protected override void OnResume()
		{
			base.OnResume();
			listData = new List<SaleOrder> ();
			populate (listData);
			apara =  DataHelper.GetAdPara (pathToDatabase);
			rights = Utility.GetAccessRights (pathToDatabase);
			listView = FindViewById<ListView> (Resource.Id.feedList);
			SetViewDlg viewdlg = SetViewDelegate;
			listView.Adapter = new GenericListAdapter<SaleOrder> (this, listData, Resource.Layout.ListItemRow, viewdlg);
		}

		void OnListItemClick(object sender, AdapterView.ItemClickEventArgs e) {
			SaleOrder item = listData.ElementAt (e.Position);
			var intent = new Intent(this, typeof(SOItemHisActivity));
			intent.PutExtra ("invoiceno",item.sono );
			intent.PutExtra ("custcode",item.custcode );
			StartActivity(intent);
		}

		void OnListItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e) {
			SaleOrder item = listData.ElementAt (e.Position);
			PopupMenu menu = new PopupMenu (e.Parent.Context, e.View);
			menu.Inflate (Resource.Menu.popupHis);
			if (!rights.InvAllowPrint) {
				menu.Menu.RemoveItem (Resource.Id.popInvprint);
				menu.Menu.RemoveItem (Resource.Id.popInvprint2);
			}
			menu.MenuItemClick += (s1, arg1) => {
				
				if (arg1.Item.ItemId == Resource.Id.popInvprint) {
					PrintSO (item,1);	
				}else if (arg1.Item.ItemId == Resource.Id.popInvprint2) {
					PrintSO (item,2);	
				}  
			};
			menu.Show ();
		}

		void populate(List<SaleOrder> list)
		{
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				var list2 = db.Table<SaleOrder>().Where(x=>x.isUploaded==true).ToList<SaleOrder>();
				foreach(var item in list2)
				{
					list.Add(item);
				}

			}
			compinfo = DataHelper.GetCompany (pathToDatabase);
		}


		void PrintSO(SaleOrder so,int noofcopy)
		{
			//Toast.MakeText (this, "print....", ToastLength.Long).Show ();	
			SaleOrderDtls[] list;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)){
				var ls= db.Table<SaleOrderDtls> ().Where (x => x.sono==so.sono).ToList<SaleOrderDtls>();
				list = new SaleOrderDtls[ls.Count];
				ls.CopyTo (list);
			}
			IPrintDocument prtSO = PrintDocManager.GetPrintDocument<PrintSalesOrder>();
			prtSO.SetDocument (so);
			prtSO.SetDocumentDtls(list);
			prtSO.SetNoOfCopy (noofcopy);
			prtSO.SetCallingActivity (this);
			if (prtSO.StartPrint ()) {
				updatePrintedStatus (so);
				var found = listData.Where (x => x.sono == so.sono).ToList ();
				if (found.Count > 0) {
					found [0].isPrinted = true;
					SetViewDlg viewdlg = SetViewDelegate;
					listView.Adapter = new GenericListAdapter<SaleOrder> (this, listData, Resource.Layout.ListItemRow, viewdlg);
				}
			} else {
				Toast.MakeText (this, prtSO.GetErrMsg(), ToastLength.Long).Show ();	
			}
		}

		void updatePrintedStatus(SaleOrder so)
		{
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list = db.Table<SaleOrder> ().Where (x => x.sono == so.sono).ToList<SaleOrder> ();
				if (list.Count > 0) {
					//if only contains items then allow to update the printed status.
					//this to allow the invoice;s item can be added. if not can not be posted(upload)
					var list2 = db.Table<SaleOrderDtls> ().Where (x => x.sono == so.sono).ToList<SaleOrderDtls> ();
					if (list2.Count > 0) {
						list [0].isPrinted = true;
						db.Update (list [0]);
					}
				}
			}
		}

	}
}

