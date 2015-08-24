
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
using Android.Content.PM;

namespace wincom.mobile.erp
{
	[Activity (Label = "UPLOAD",Icon="@drawable/upload")]			
	//[Activity (Label = Resources.GetString(Resource.String.mainmenu_downupload))]			
	public class UtilityActivity : Activity
	{
		AccessRights rights;
		string pathToDatabase;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetTitle (Resource.String.mainmenu_settings);
			SetContentView (Resource.Layout.Settings);
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			rights = Utility.GetAccessRights (pathToDatabase);

			Button butSett = FindViewById<Button> (Resource.Id.butsetting);
			butSett.Click += butSetting;
			Button butAbt = FindViewById<Button> (Resource.Id.butAbout);
			butAbt.Click+= ButAbt_Click;
			Button buttestprint =FindViewById<Button> (Resource.Id.buttestprint);
			buttestprint.Click+= Buttestprint_Click;

			Button butback = FindViewById<Button> (Resource.Id.butMain);
			butback.Click+= (object sender, EventArgs e) => {
				base.OnBackPressed();
			};

		
		}

		void Buttestprint_Click (object sender, EventArgs e)
		{
			IPrintDocument prtInv = PrintDocManager.GetPrintDocument<PrintTest>();
			prtInv.SetNoOfCopy (1);
			prtInv.SetCallingActivity (this);
			if (!prtInv.StartPrint ()) {
				Toast.MakeText (this, prtInv.GetErrMsg(), ToastLength.Long).Show ();				
			}
		}

		void butSetting(object sender,EventArgs e)
		{
			//StartActivity (typeof(SettingActivity));
			var intent =ActivityManager.GetActivity<SettingActivity>(this.ApplicationContext);
			StartActivity(intent);
		}

		void ButAbt_Click (object sender, EventArgs e)
		{
			CompanyInfo comp= DataHelper.GetCompany (pathToDatabase);
			View messageView = LayoutInflater.Inflate(Resource.Layout.About, null, false);
			PackageInfo pInfo = PackageManager.GetPackageInfo (PackageName, 0);
			// When linking text, force to always use default color. This works
			// around a pressed color state bug.
			TextView textView = (TextView) messageView.FindViewById(Resource.Id.about_credits);
			TextView textDesc = (TextView) messageView.FindViewById(Resource.Id.about_descrip);
			TextView textVer = (TextView) messageView.FindViewById(Resource.Id.about_ver);
			//textDesc.Text = Html.FromHtml (Resources.GetString(Resource.String.app_descrip))..ToString();
			textView.Text = "For inquiry, please contact " + comp.SupportContat;
			textVer .Text = "Build Version : "+pInfo.VersionName;
			AlertDialog.Builder builder = new AlertDialog.Builder(this);
			builder.SetIcon(Resource.Drawable.Icon);
			builder.SetTitle(Resource.String.app_name);
			builder.SetView(messageView);
			builder.Create();
			builder.Show();
		}
	
	}
}

