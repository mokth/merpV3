
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
	[Activity (Label = "StockEntry")]			
	public class StockEntry : Activity
	{
		string pathToDatabase;
		EditText txtICode;
		TextView txtIDesc;
		EditText txtDate;
		EditText qtyGr;
		EditText qtyAct;
		EditText qtySales;
		EditText qtyRtn;
		EditText qtyCrf;
		EditText qtyBrf;
		Button btnSave;
		Button btnCancel;
		string ICODE;
		string TRXDATE;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			SetContentView (Resource.Layout.StockEntry);
			pathToDatabase = ((GlobalvarsApp)this.Application).DATABASE_PATH;
			ICODE= Intent.GetStringExtra ("icode") ?? "";
			TRXDATE = Intent.GetStringExtra ("trxdate") ?? "";
			// Create your application here
			FindControls();
			LoadData ();
		}

		void FindControls(){
			txtICode =FindViewById<EditText> (Resource.Id.txticode);
			txtIDesc=FindViewById<TextView> (Resource.Id.txtdesc);
			txtDate=FindViewById<EditText> (Resource.Id.txtdate);
			qtyGr=FindViewById<EditText> (Resource.Id.qtygr);
			qtyAct=FindViewById<EditText> (Resource.Id.qtyact);
			qtySales=FindViewById<EditText> (Resource.Id.qtysales);
			qtyRtn=FindViewById<EditText> (Resource.Id.qtyrtn);
			qtyCrf=FindViewById<EditText> (Resource.Id.qtycrf);
			qtyBrf=FindViewById<EditText> (Resource.Id.qtybrf);
			btnSave = FindViewById<Button> (Resource.Id.Save);
			btnCancel = FindViewById<Button> (Resource.Id.Cancel);
			btnSave.Click+= BtnSave_Click;
			btnCancel.Click+= BtnCancel_Click;
			txtICode.Focusable = false;
			txtDate.Focusable = false;
			qtyGr.Focusable = false;


		}

		void BtnCancel_Click (object sender, EventArgs e)
		{
			base.OnBackPressed();
		}

		void BtnSave_Click (object sender, EventArgs e)
		{
			SaveData ();
		}

		void LoadData()
		{
			ItemStock stk;
			DateTime date= Utility.ConvertToDate (TRXDATE);
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				var list =db.Table<ItemStock> ().Where (x => x.ICode == ICODE && x.DateTrx == date).ToList();
				if (list.Count > 0) {
					txtIDesc.Text = list [0].IDesc;
					txtICode.Text = list [0].ICode;
					txtDate.Text = TRXDATE;
					qtyAct.Text = list [0].QtyAct.ToString ();
					qtyGr.Text = list [0].QtyGR.ToString ();
					qtyRtn.Text = list [0].QtyRtr.ToString ();
					qtyCrf.Text = list [0].QtyCrf.ToString ();
					qtyBrf.Text = list [0].QtyBrf.ToString ();
					qtySales.Text = list [0].QtyBal.ToString ();
				}

//				var listinv =db.Table<Invoice> ().Where (x =>x.invdate == date).ToList();
//				var list2 = db.Table<InvoiceDtls> ().ToList<InvoiceDtls> ();
//
//				double ttlsales = 0;
//				foreach (Invoice inv in listinv) {
//					ttlsales = ttlsales + list2.Where (x => x.invno == inv.invno && x.icode == ICODE).Sum (x => x.qty);
//				}
//
//				qtySales.Text = ttlsales.ToString ();
			}
		}

		void SaveData()
		{
			DateTime date= Utility.ConvertToDate (TRXDATE);
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{
				var list =db.Table<ItemStock> ().Where (x => x.ICode == ICODE && x.DateTrx == date).ToList();
				if (list.Count > 0) {
					
					list [0].QtyAct = Convert2NumTool<double>.ConvertVal (qtyAct.Text);
					list [0].QtyRtr = Convert2NumTool<double>.ConvertVal (qtyRtn.Text);
					list [0].QtyCrf = Convert2NumTool<double>.ConvertVal (qtyCrf.Text);
					list [0].QtyBrf = Convert2NumTool<double>.ConvertVal (qtyBrf.Text);
					list [0].QtyBal = Convert2NumTool<double>.ConvertVal (qtySales.Text);
					db.Update (list [0]);
				} 
			}
			Toast.MakeText (this,"Successfully save...", ToastLength.Long).Show ();
			base.OnBackPressed();
		}
	}
}

