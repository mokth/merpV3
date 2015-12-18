
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
	[Activity (Label = "CASH BILL",Icon="@drawable/shop")]			
	public class CashBillListing : Activity
	{
		ListView listView ;
		List<Invoice> listData = new List<Invoice> ();
		string pathToDatabase;
		string COMPCODE;

		AdPara apara=null;
		CompanyInfo compinfo;
		AccessRights rights;


		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!((GlobalvarsApp)this.Application).ISLOGON) {
				Finish ();
			}

			SetTitle (Resource.String.title_cash);

			// Create your application here
			SetContentView (Resource.Layout.ListView);
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			COMPCODE = ((GlobalvarsApp)this.Application).COMPANY_CODE;
			rights = Utility.GetAccessRights (pathToDatabase);
			populate (listData);
			apara =  DataHelper.GetAdPara (pathToDatabase);
			listView = FindViewById<ListView> (Resource.Id.feedList);
			Button butNew= FindViewById<Button> (Resource.Id.butnewInv); 
			butNew.Visibility = ViewStates.Gone;
			//butNew.Click += butCreateNewInv;
			Button butInvBack= FindViewById<Button> (Resource.Id.butInvBack); 
			butInvBack.Click += (object sender, EventArgs e) => {
				StartActivity(typeof(POSSalesActivity));
			};

			//if (!rights.InvAllowAdd)
			//	butNew.Enabled = false;

			listView.ItemClick += OnListItemLongClick;//OnListItemClick;
			listView.ItemLongClick += ListView_ItemLongClick;
			//listView.Adapter = new CusotmListAdapter(this, listData);
			SetViewDlg viewdlg = SetViewDelegate;
			listView.Adapter = new GenericListAdapter<Invoice> (this, listData, Resource.Layout.ListItemRow, viewdlg);

		}

		public override void OnBackPressed() {
			// do nothing.
		}

		void ListView_ItemLongClick (object sender, AdapterView.ItemLongClickEventArgs e)
		{
			Invoice item = listData.ElementAt (e.Position);
			var intent =ActivityManager.GetActivity<CashItemActivity>(this.ApplicationContext);
			intent.PutExtra ("invoiceno",item.invno );
			intent.PutExtra ("custcode",item.custcode );
			StartActivity(intent);
		}

		private void SetViewDelegate(View view,object clsobj)
		{
			Invoice item = (Invoice)clsobj;
			string trxtype = "";
			if (item.trxtype=="CASH") 
				trxtype = "CS";
			else if (item.trxtype=="INVOICE") 
				trxtype = "INV";
			else trxtype = "CN";
			view.FindViewById<TextView> (Resource.Id.invdate).Text = item.invdate.ToString ("dd-MM-yy");
			view.FindViewById<TextView> (Resource.Id.invno).Text = item.invno;
			view.FindViewById<TextView> (Resource.Id.trxtype).Text = trxtype;// item.trxtype;
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
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			COMPCODE = ((GlobalvarsApp)this.Application).COMPANY_CODE;
			rights = Utility.GetAccessRights (pathToDatabase);
			populate (listData);
			apara =  DataHelper.GetAdPara (pathToDatabase);
			listView = FindViewById<ListView> (Resource.Id.feedList);
			SetViewDlg viewdlg = SetViewDelegate;
			listView.Adapter = new GenericListAdapter<Invoice> (this, listData, Resource.Layout.ListItemRow, viewdlg);
		
		}

//		void OnListItemClick(object sender, AdapterView.ItemClickEventArgs e) {
//			Invoice item = listData.ElementAt (e.Position);
//			//var intent = new Intent(this, typeof(InvItemActivity));
//			var intent =ActivityManager.GetActivity<InvItemActivity>(this.ApplicationContext);
//			intent.PutExtra ("invoiceno",item.invno );
//			intent.PutExtra ("custcode",item.custcode );
//			StartActivity(intent);
//		}

		void OnListItemLongClick(object sender, AdapterView.ItemClickEventArgs  e) {
			Invoice item = listData.ElementAt (e.Position);
			PopupMenu menu = new PopupMenu (e.Parent.Context, e.View);
			menu.Inflate (Resource.Menu.popupInv);
			var mitem =menu.Menu.FindItem(Resource.Id.poppay);
			mitem.SetVisible (true);
			menu.Menu.RemoveItem (Resource.Id.popInvdelete);
			menu.Menu.RemoveItem (Resource.Id.popInvadd);
			menu.Menu.RemoveItem (Resource.Id.poppay);
			bool  isNotAllowEditAfterPrinted  = DataHelper.GetInvoicePrintStatus (pathToDatabase,item.invno,rights);

			if (isNotAllowEditAfterPrinted) {
				menu.Menu.RemoveItem (Resource.Id.popInvedit);
			}

			if (!rights.InvAllowPrint) {
				menu.Menu.RemoveItem (Resource.Id.popInvprint);
				menu.Menu.RemoveItem (Resource.Id.popInvprint2);
			}

			if (DataHelper.GetInvoicePrintStatus(pathToDatabase, item.invno,rights)) {
				menu.Menu.RemoveItem (Resource.Id.popInvdelete);
				menu.Menu.RemoveItem (Resource.Id.popInvedit);
			}
			menu.MenuItemClick += (s1, arg1) => {
				if (arg1.Item.ItemId==Resource.Id.popInvadd)
				{
					CreateNewInvoice();
				}else if (arg1.Item.ItemId==Resource.Id.popInvprint)
				{
					PrintInv(item,1);	
				}else if (arg1.Item.ItemId==Resource.Id.popInvprint2)
				{
					PrintInv(item,2);	

				} else if (arg1.Item.ItemId==Resource.Id.popInvdelete)
				{
					Delete(item);
				}else if (arg1.Item.ItemId==Resource.Id.popInvedit)
				{
					Edit(item);
				}
				else if (arg1.Item.ItemId==Resource.Id.poppay)
				{
					payment(item);
				} 

			};
			menu.Show ();
		}

		void payment(Invoice inv)
		{
//			var intent = new Intent (this, typeof(Payment));
//			double ttl = inv.amount + inv.taxamt;
//			intent.PutExtra ("amount", ttl.ToString());
//			StartActivity (intent);
			var dialog = CashDialog.NewInstance();
			dialog.Amount =inv.amount + inv.taxamt;
			dialog.Show(FragmentManager, "dialogPaid");
		}

		void Edit(Invoice inv)
		{
			//var intent = new Intent (this, typeof(EditInvoice));
			var intent =ActivityManager.GetActivity<POSSalesActivity >(this.ApplicationContext);
			intent.PutExtra ("trxtype", inv.trxtype);
			intent.PutExtra ("invoiceno", inv.invno);
			StartActivity (intent);
		}
		void Delete(Invoice inv)
		{
			var builder = new AlertDialog.Builder(this);
			builder.SetMessage(Resources.GetString(Resource.String.msg_confirmdelete)+"?");
			builder.SetPositiveButton("YES", (s, e) => { DeleteItem(inv); });
			builder.SetNegativeButton("Cancel", (s, e) => { /* do something on Cancel click */ });
			builder.Create().Show();
		}
		void DeleteItem(Invoice inv)
		{
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				var list = db.Table<Invoice>().Where(x=>x.invno==inv.invno).ToList<Invoice>();
				var list2 = db.Table<InvoiceDtls>().Where(x=>x.invno==inv.invno).ToList<InvoiceDtls>();
				if (list.Count > 0) {
					db.Delete (list [0]);
					db.Delete (list2);
					var arrlist= listData.Where(x=>x.invno==inv.invno).ToList<Invoice>();
					if (arrlist.Count > 0) {
						listData.Remove (arrlist [0]);
						SetViewDlg viewdlg = SetViewDelegate;
						listView.Adapter = new GenericListAdapter<Invoice> (this, listData, Resource.Layout.ListItemRow, viewdlg);
					}
				}
			}
		}

		void populate(List<Invoice> list)
		{
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				var list2 = db.Table<Invoice> ()
					.Where (x => x.isUploaded == false && x.trxtype=="CASH")
					.OrderByDescending(x=>x.invdate)
					.ThenByDescending (x => x.created)
					.ToList<Invoice> ();
				foreach(var item in list2)
				{
					list.Add(item);
				}

			}
			compinfo = DataHelper.GetCompany (pathToDatabase);
		}


		private void butCreateNewInv(object sender, EventArgs e)
		{
			CreateNewInvoice ();
		}

		private void CreateNewInvoice()
		{
			var intent =ActivityManager.GetActivity<POSSalesActivity >(this.ApplicationContext);
			intent.PutExtra ("action", "create");
			StartActivity (intent);
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
			//mmDevice = null;
			//findBTPrinter ();
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

	}
}

