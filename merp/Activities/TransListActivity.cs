
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

namespace wincom.mobile.erp
{
	[Activity (Label = "TRANSACTION",Icon="@drawable/shoplist")]			
	public class TransListActivity : Activity
	{
		string pathToDatabase;
		AccessRights rights;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!((GlobalvarsApp)this.Application).ISLOGON) {
				Finish ();
			}
			SetTitle (Resource.String.mainmenu_trxlist);
			SetContentView (Resource.Layout.Translist);
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			rights = Utility.GetAccessRights (pathToDatabase);

			Button butCashlist = FindViewById<Button> (Resource.Id.butCashlist);
			butCashlist.Click+= ButCashlist_Click ;

			Button butInvlist = FindViewById<Button> (Resource.Id.butInvlist);
			butInvlist.Click+= ButInvlist_Click;

			Button butCNNoteList = FindViewById<Button> (Resource.Id.butCNlist);
			butCNNoteList.Click+= ButCNNoteList_Click;

			Button butSOList = FindViewById<Button> (Resource.Id.butSOlist);
			butSOList.Click+= ButSOList_Click;

			Button butDOList = FindViewById<Button> (Resource.Id.butDOlist);
			butDOList.Click+= ButDOList_Click;

			Button butsumm = FindViewById<Button> (Resource.Id.butInvsumm);
			butsumm.Click+= (object sender, EventArgs e) => {
				StartActivity(typeof(PrintSumm));
			};

			Button butMap = FindViewById<Button> (Resource.Id.butMap);
			butMap.Click+= (object sender, EventArgs e) => {
				StartActivity(typeof(ShowMapActivity));
			};

			Button butback = FindViewById<Button> (Resource.Id.butMain);
			butback.Click+= (object sender, EventArgs e) => {
				StartActivity(typeof(MainActivity));
			};

			if (!rights.IsSOModule) {
				butSOList.Visibility = ViewStates.Gone;
			}
			if (!rights.IsCNModule) {
				butCNNoteList.Visibility = ViewStates.Gone;
			}
			if (!rights.IsDOModule) {
				butDOList.Visibility = ViewStates.Gone;
			}
		}

		void ButInvlist_Click (object sender, EventArgs e)
		{
			//var intent = new Intent(this, typeof(InvoiceAllActivity));
			var intent =ActivityManager.GetActivity<InvoiceAllActivity>(this.ApplicationContext);
			intent.PutExtra ("trxtype", "INVOICE");
			StartActivity(intent);
		}

		void ButCashlist_Click (object sender, EventArgs e)
		{
			//var intent = new Intent(this, typeof(InvoiceAllActivity));
			var intent =ActivityManager.GetActivity<InvoiceAllActivity>(this.ApplicationContext);
			intent.PutExtra ("trxtype", "CASH");
			StartActivity(intent);
		}

		void ButCNNoteList_Click (object sender, EventArgs e)
		{
			//var intent = new Intent(this, typeof(CNAllActivity));
			var intent =ActivityManager.GetActivity<CNAllActivity>(this.ApplicationContext);
			StartActivity(intent);
		}

		void ButSOList_Click (object sender, EventArgs e)
		{
			//var intent = new Intent(this, typeof(CNAllActivity));
			var intent =ActivityManager.GetActivity<SOAllActivity>(this.ApplicationContext);
			StartActivity(intent);
		}

		void ButDOList_Click (object sender, EventArgs e)
		{
			//var intent = new Intent(this, typeof(CNAllActivity));
			var intent =ActivityManager.GetActivity<DOAllActivity>(this.ApplicationContext);
			StartActivity(intent);
		}
	}
}

