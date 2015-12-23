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
	[Activity (Label = "DELIVERY ORDER",Icon="@drawable/delivery")]			
	public class DelOrderActivity : Activity
	{
		ListView listView ;
		List<DelOrder> listData = new List<DelOrder> ();
		string pathToDatabase;
		AccessRights rights;
		AdPara apara=null;
		CompanyInfo compinfo;
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!((GlobalvarsApp)this.Application).ISLOGON) {
				Finish ();
			}
			SetTitle (Resource.String.title_do);
			// Create your application here
			SetContentView (Resource.Layout.ListView);
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			rights = Utility.GetAccessRights (pathToDatabase);

			populate (listData);
			apara =  DataHelper.GetAdPara (pathToDatabase);
			listView = FindViewById<ListView> (Resource.Id.feedList);
			Button butNew= FindViewById<Button> (Resource.Id.butnewInv); 
			butNew.Click += butCreateNewInv;
			Button butInvBack= FindViewById<Button> (Resource.Id.butInvBack); 
			butInvBack.Click += (object sender, EventArgs e) => {
				StartActivity(typeof(MainActivity));
			};

			listView.ItemClick += OnListItemClick;
			listView.ItemLongClick += OnListItemLongClick;
			//listView.Adapter = new CusotmListAdapter(this, listData);
			SetViewDlg viewdlg = SetViewDelegate;
			listView.Adapter = new GenericListAdapter<DelOrder> (this, listData, Resource.Layout.ListItemRow, viewdlg);

		}

		public override void OnBackPressed() {
			// do nothing.
		}

		private void SetViewDelegate(View view,object clsobj)
		{
			DelOrder item = (DelOrder)clsobj;
			view.FindViewById<TextView> (Resource.Id.invdate).Text = item.dodate.ToString ("dd-MM-yy");
			view.FindViewById<TextView> (Resource.Id.invno).Text = item.dono;
			view.FindViewById<TextView> (Resource.Id.trxtype).Text = "";
			view.FindViewById<TextView>(Resource.Id.invcust).Text = item.description;
			//view.FindViewById<TextView> (Resource.Id.Amount).Text = item.amount.ToString("n2");
			view.FindViewById<TextView> (Resource.Id.TaxAmount).Text = "";
			view.FindViewById<TextView> (Resource.Id.TtlAmount).Text = item.amount.ToString("n");;
			ImageView img = view.FindViewById<ImageView> (Resource.Id.printed);
			if (!item.isPrinted)
				img.Visibility = ViewStates.Invisible;
			if (!string.IsNullOrEmpty (item.remark)) {
				view.FindViewById<LinearLayout> (Resource.Id.linearLayoutRmark).Visibility = ViewStates.Visible;
				view.FindViewById<TextView> (Resource.Id.invremark).Text = item.remark.ToUpper ();
			}
		}

		protected override void OnResume()
		{
			base.OnResume();
			listData = new List<DelOrder> ();
			populate (listData);
			rights = Utility.GetAccessRights (pathToDatabase);
			apara =  DataHelper.GetAdPara (pathToDatabase);
			listView = FindViewById<ListView> (Resource.Id.feedList);
			SetViewDlg viewdlg = SetViewDelegate;
			listView.Adapter = new GenericListAdapter<DelOrder> (this, listData, Resource.Layout.ListItemRow, viewdlg);
		}

		void OnListItemClick(object sender, AdapterView.ItemClickEventArgs e) {
			DelOrder item = listData.ElementAt (e.Position);
			//var intent = new Intent(this, typeof(DOItemActivity));
			var intent =ActivityManager.GetActivity<DOItemActivity>(this.ApplicationContext);
			intent.PutExtra ("invoiceno",item.dono );
			intent.PutExtra ("custcode",item.custcode );
			StartActivity(intent);
		}

		void OnListItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e) {
			DelOrder item = listData.ElementAt (e.Position);
			PopupMenu menu = new PopupMenu (e.Parent.Context, e.View);
			menu.Inflate (Resource.Menu.popupInv);
			menu.Menu.RemoveItem (Resource.Id.poppay);

			menu.Menu.RemoveItem (Resource.Id.popInvdelete);
			if (!rights.DOAllowAdd) {
				menu.Menu.RemoveItem (Resource.Id.popInvadd);
			}

			if (!rights.DOAllowEdit) {
				menu.Menu.RemoveItem (Resource.Id.popInvedit);
			}

			if (!rights.DOAllowPrint) {
				menu.Menu.RemoveItem (Resource.Id.popInvprint);
				menu.Menu.RemoveItem (Resource.Id.popInvprint2);
			}

			if (DataHelper.GetDelOderPrintStatus (pathToDatabase, item.dono, rights)) {
				menu.Menu.RemoveItem (Resource.Id.popInvdelete);
				menu.Menu.RemoveItem (Resource.Id.popInvedit);
			}
			menu.MenuItemClick += (s1, arg1) => {
				if (arg1.Item.ItemId == Resource.Id.popInvadd) {
					CreateNewDelOrder ();
				} else if (arg1.Item.ItemId == Resource.Id.popInvprint) {
					PrintInv (item, 1);	
				} else if (arg1.Item.ItemId == Resource.Id.popInvprint2) {
					PrintInv (item, 2);	

				} else if (arg1.Item.ItemId == Resource.Id.popInvdelete) {
					Delete (item);
				} else if (arg1.Item.ItemId == Resource.Id.popInvedit) {
					Edit(item);
				}

			};
			menu.Show ();
		}

		void Edit(DelOrder doorder)
		{
			//var intent = new Intent (this, typeof(EditInvoice));
			var intent =ActivityManager.GetActivity<EditDelOrder>(this.ApplicationContext);
			intent.PutExtra ("dono", doorder.dono);
			StartActivity (intent);
		}

		void Delete(DelOrder inv)
		{
			var builder = new AlertDialog.Builder(this);
			builder.SetMessage(Resources.GetString(Resource.String.msg_confirmdelete)+"?");
			builder.SetPositiveButton("YES", (s, e) => { DeleteItem(inv); });
			builder.SetNegativeButton("Cancel", (s, e) => { /* do something on Cancel click */ });
			builder.Create().Show();
		}
		void DeleteItem(DelOrder doorder)
		{
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				var list = db.Table<DelOrder>().Where(x=>x.dono==doorder.dono).ToList<DelOrder>();
				if (list.Count > 0) {
					db.Delete (list [0]);
					var arrlist= listData.Where(x=>x.dono==doorder.dono).ToList<DelOrder>();
					if (arrlist.Count > 0) {
						listData.Remove (arrlist [0]);
						SetViewDlg viewdlg = SetViewDelegate;
						listView.Adapter = new GenericListAdapter<DelOrder> (this, listData, Resource.Layout.ListItemRow, viewdlg);
					}
				}
			}
		}

		void populate(List<DelOrder> list)
		{
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				var list2 = db.Table<DelOrder> ()
					.Where(x=>x.isUploaded==false)
					.OrderByDescending (x => x.dodate)
					.ThenByDescending (x => x.created)
					.ToList<DelOrder> ();
				
				foreach(var item in list2)
				{
					list.Add(item);
				}

			}
			compinfo = DataHelper.GetCompany (pathToDatabase);
		}


		private void butCreateNewInv(object sender, EventArgs e)
		{
			CreateNewDelOrder ();
		}

		private void CreateNewDelOrder()
		{
			//var intent = new Intent(this, typeof(CreateDelOrder));
			var intent =ActivityManager.GetActivity<CreateDelOrder>(this.ApplicationContext);
			StartActivity(intent);
		}


		void PrintInv(DelOrder doorder,int noofcopy)
		{
			//Toast.MakeText (this, "print....", ToastLength.Long).Show ();	
			DelOrderDtls[] list;
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)){
				var ls= db.Table<DelOrderDtls> ().Where (x => x.dono==doorder.dono).ToList<DelOrderDtls>();
				list = new DelOrderDtls[ls.Count];
				ls.CopyTo (list);
			}

			IPrintDocument prtSO = PrintDocManager.GetPrintDocument<PrintDelOrder>();
			prtSO.SetDocument (doorder);
			prtSO.SetDocumentDtls(list);
			prtSO.SetNoOfCopy (noofcopy);
			prtSO.SetCallingActivity (this);
			if (prtSO.StartPrint ()) {
				updatePrintedStatus (doorder);
				var found = listData.Where (x => x.dono == doorder.dono).ToList ();
				if (found.Count > 0) {
					found [0].isPrinted = true;
					SetViewDlg viewdlg = SetViewDelegate;
					listView.Adapter = new GenericListAdapter<DelOrder> (this, listData, Resource.Layout.ListItemRow, viewdlg);
				}
			} else {
				Toast.MakeText (this, prtSO.GetErrMsg(), ToastLength.Long).Show ();	
			}
		
		}

		void updatePrintedStatus(DelOrder doorder)
		{
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				var list = db.Table<DelOrder> ().Where (x => x.dono == doorder.dono).ToList<DelOrder> ();
				if (list.Count > 0) {
					//if only contains items then allow to update the printed status.
					//this to allow the invoice;s item can be added. if not can not be posted(upload)
					var list2 = db.Table<DelOrderDtls> ().Where (x => x.dono == doorder.dono).ToList<DelOrderDtls> ();
					if (list2.Count > 0) {
						list [0].isPrinted = true;
						db.Update (list [0]);
					}
				}
			}
		}


	}
}

