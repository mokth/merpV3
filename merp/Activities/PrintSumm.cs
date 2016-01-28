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
using System.Collections;
using Android.Views.InputMethods;

namespace wincom.mobile.erp
{
	[Activity (Label = "PRINT SUMMARY",Icon="@drawable/summary")]			
	public class PrintSumm : Activity
	{
		string pathToDatabase;
		AdPara apara;
		const int DATE_DIALOG_ID1 = 0;
		const int DATE_DIALOG_ID2 = 1;
		DateTime date;
		private InputMethodManager imm;
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!((GlobalvarsApp)this.Application).ISLOGON) {
				Finish ();
			}
			SetTitle (Resource.String.submenu_summary);
			SetContentView (Resource.Layout.PrintInvSumm);
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			apara =  DataHelper.GetAdPara (pathToDatabase);
			Button butPrint= FindViewById<Button> (Resource.Id.printsumm); 
			Button butInvBack= FindViewById<Button> (Resource.Id.printsumm_cancel); 
			EditText frd = FindViewById<EditText> (Resource.Id.trxdatefr);
			EditText tod = FindViewById<EditText> (Resource.Id.trxdateto);
			imm = (InputMethodManager)GetSystemService(Context.InputMethodService);
			frd.Text = DateTime.Today.ToString ("dd-MM-yyyy");
			frd.Click += delegate(object sender, EventArgs e) {
				imm.HideSoftInputFromWindow(frd.WindowToken, 0);
				ShowDialog (DATE_DIALOG_ID1);
			};
			tod.Text = DateTime.Today.ToString ("dd-MM-yyyy");
			date =  DateTime.Today;
			tod.Click += delegate(object sender, EventArgs e) {
				imm.HideSoftInputFromWindow(tod.WindowToken, 0);
				ShowDialog (DATE_DIALOG_ID2);
			};
			butInvBack.Click += (object sender, EventArgs e) => {
				StartActivity(typeof(TransListActivity));
			};
			butPrint.Click+= ButPrint_Click;


		}
		protected override Dialog OnCreateDialog (int id)
		{
			switch (id) {
			case DATE_DIALOG_ID1:
				return new DatePickerDialog (this, OnDateSet1, date.Year, date.Month - 1, date.Day); 
			case DATE_DIALOG_ID2:
				return new DatePickerDialog (this, OnDateSet2, date.Year, date.Month - 1, date.Day); 
			}
			return null;
		}
		void OnDateSet1 (object sender, DatePickerDialog.DateSetEventArgs e)
		{
			EditText frd = FindViewById<EditText> (Resource.Id.trxdatefr);
			//EditText tod = FindViewById<EditText> (Resource.Id.trxdateto);
			frd.Text = e.Date.ToString("dd-MM-yyyy");
		}
		void OnDateSet2 (object sender, DatePickerDialog.DateSetEventArgs e)
		{
			EditText tod = FindViewById<EditText> (Resource.Id.trxdateto);
			tod.Text = e.Date.ToString("dd-MM-yyyy");
		}

		void ButPrint_Click (object sender, EventArgs e)
		{
			EditText frd = FindViewById<EditText> (Resource.Id.trxdatefr);
			EditText tod = FindViewById<EditText> (Resource.Id.trxdateto);
			DateTime? sdate=null;
			DateTime? edate=null;
			if (frd.Text != "") {
				sdate = Utility.ConvertToDate (frd.Text);
			} else
				sdate = DateTime.Today;

//			if (tod.Text != "") {
//				edate =Utility.ConvertToDate (tod.Text);
//			}
//			if (edate == null)
//				edate = sdate;
			PrintInvSumm (sdate.Value, sdate.Value);
		}



		void PrintInvSumm(DateTime printdate1,DateTime printdate2)
		{
			IPrintDocument prtInv = PrintDocManager.GetPrintSummary<PrintSummary>();
			prtInv.SetNoOfCopy (1);
			Hashtable para = new Hashtable ();
			para.Add ("DateStart", printdate1);
			para.Add ("DateEnd", printdate2);
			prtInv.SetExtraPara (para);
			prtInv.SetCallingActivity (this);
			prtInv.StartPrint ();
			Toast.MakeText (this, prtInv.GetErrMsg(), ToastLength.Long).Show ();	

		}

	
	}
}

