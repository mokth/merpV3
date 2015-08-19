
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
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!((GlobalvarsApp)this.Application).ISLOGON) {
				Finish ();
			}
			SetTitle (Resource.String.mainmenu_trxlist);
			SetContentView (Resource.Layout.Translist);
			Button butInvlist = FindViewById<Button> (Resource.Id.butInvlist);
			butInvlist.Click+= ButInvlist_Click;

			Button butCNNoteList = FindViewById<Button> (Resource.Id.butCNlist);
			butCNNoteList.Click+= ButCNNoteList_Click;

			Button butSOList = FindViewById<Button> (Resource.Id.butSOlist);
			butSOList.Click+= ButSOList_Click;

			Button butsumm = FindViewById<Button> (Resource.Id.butInvsumm);
			butsumm.Click+= (object sender, EventArgs e) => {
				StartActivity(typeof(PrintSumm));
			};

			Button butback = FindViewById<Button> (Resource.Id.butMain);
			butback.Click+= (object sender, EventArgs e) => {
				StartActivity(typeof(MainActivity));
			};
		}

		void ButInvlist_Click (object sender, EventArgs e)
		{
			//var intent = new Intent(this, typeof(InvoiceAllActivity));
			var intent =ActivityManager.GetActivity<InvoiceAllActivity>(this.ApplicationContext);
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
	}
}

