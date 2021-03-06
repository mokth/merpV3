﻿
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
	[Activity (Label = "MASTER",Icon="@drawable/item")]			
	public class MasterRefActivity : Activity
	{
		Button butCustProf;
		Button butMItem;
		Button butPrice;
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetTitle (Resource.String.mainmenu_master);
			SetContentView (Resource.Layout.MasterRef);

			butCustProf = FindViewById<Button> (Resource.Id.butCustProf);
			butCustProf.Click+=  butCustomerClick;

			butMItem = FindViewById<Button> (Resource.Id.butMaster);
			butMItem.Click += butMasterClick;

			butPrice = FindViewById<Button> (Resource.Id.butPrices);
			butPrice.Click+= ButPrice_Click;
			Button butback = FindViewById<Button> (Resource.Id.butMain);
			butback.Click += (object sender, EventArgs e) => {
				base.OnBackPressed ();
			};

			GetTotalNumber ();
		}

		void ButPrice_Click (object sender, EventArgs e)
		{
			var intent = new Intent(this, typeof(ItemPricesActivity));
			StartActivity(intent);
		}

		void GetTotalNumber()
		{   
			string pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			int ttlNum = 0;
			int numitm =DataHelper.GetTotalItems (pathToDatabase);
			int numcust =DataHelper.GetTotalCusts (pathToDatabase);
			int numprices =DataHelper.GetTotalItemPrice (pathToDatabase);


			butMItem.Text = Resources.GetString (Resource.String.submenu_item)+" ("+numitm.ToString()+")";
			butCustProf.Text = Resources.GetString (Resource.String.submenu_cust)+" ("+numcust.ToString()+")";
			butPrice.Text= Resources.GetString (Resource.String.submenu_itemprice)+" ("+numprices.ToString()+")";
		}
		private void butMasterClick(object sender,EventArgs e)
		{
			var intent = new Intent(this, typeof(MasterItemActivity));
			StartActivity(intent);
		}
		private void butCustomerClick(object sender,EventArgs e)
		{
			var intent = new Intent(this, typeof(CustomerActivity));
			StartActivity(intent);
		}
	}
}

