//
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//
//using Android.App;
//using Android.Content;
//using Android.OS;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Android.Views.InputMethods;
//using System.Collections;
//
//namespace wincom.mobile.erp
//{
//	[Activity (Label = "PAYMENT")]			
//	public class PaymentActivity : Activity
//	{
//		EditText txtAmt;
//		EditText txtRound;
//		EditText txtTotal;
//		EditText txtCash;
//		EditText txtChange;
//		EditText txtCust;
//		EditText txtRemark;
//
//		Button butCancel;
//		Button butPrint;
//		Button butPaid;
//
//		string pathToDatabase;
//		string AMOUNT;
//		string CUSTNAME;
//		string INVOICENO;
//		static int eventid ;
//		double amount;
//
//		protected override void OnCreate (Bundle savedInstanceState)
//		{
//			base.OnCreate (savedInstanceState);
//
//			if (!((GlobalvarsApp)this.Application).ISLOGON) {
//				Finish ();
//			}
//			SetContentView (Resource.Layout.PaymentEx);
//			GetControls ();
//			AMOUNT = Intent.GetStringExtra ("amount") ?? "0";
//			CUSTNAME = Intent.GetStringExtra ("custname") ?? "";
//			INVOICENO = Intent.GetStringExtra ("invoiceno") ?? "";
//
//			amount = Convert2NumTool<double>.ConvertVal (AMOUNT);
//		}
//
//		void DisplayData()
//		{
//			txtCust.Text = CUSTNAME;
//			txtAmt.Text = amount.ToString ("n2");
//			double roundVal = 0;
//			double ttlAmt = Utility.AdjustToNear (amount, ref roundVal);
//			txtRound.Text = roundVal.ToString ("n2");
//			txtTotal.Text = ttlAmt.ToString ("n2");
//			txtCash.EditorAction += HandleEditorAction;
//			txtCash.AfterTextChanged += TxtCash_AfterTextChanged;
//			txtCash.Text = "0";
//		}
//
//		void GetControls ()
//		{
//			butCancel = FindViewById<Button> (Resource.Id.Cancel);
//			butPrint = FindViewById<Button> (Resource.Id.Print);
//			butPaid = FindViewById<Button> (Resource.Id.Paid);
//			txtAmt = FindViewById<EditText> (Resource.Id.payamt);
//			txtCash = FindViewById<EditText> (Resource.Id.paycash);
//			txtChange =  FindViewById<EditText> (Resource.Id.paychange);
//			txtCust = FindViewById<EditText> (Resource.Id.newinv_custname);
//			txtRemark = FindViewById<EditText> (Resource.Id.newinv_remark);
//			txtRound = FindViewById<EditText> (Resource.Id.payround);
//			txtTotal = FindViewById<EditText> (Resource.Id.paytotal);
//
//		}
//
//		void ButtonAction()
//		{
//			butCancel.Click+= ButCancel_Click;
//			butPaid.Click+= ButPaid_Click;
//			butPrint.Click+= ButPrint_Click;
//		}
//
//		void ButPrint_Click (object sender, EventArgs e)
//		{
//			Printreceipt ();
//		}
//
//		void ButPaid_Click (object sender, EventArgs e)
//		{
//			Hashtable param = new Hashtable ();
//			EventParam p = new EventParam (EventID.PAYMENT_PAID , param);
//			EventManagerFacade.Instance.GetEventManager ().PerformEvent (this, p);
//		}
//
//		void ButCancel_Click (object sender, EventArgs e)
//		{
//			
//		}
//
//		void
//
//		void Printreceipt()
//		{
//			InvoiceDtls[] list;
//			Invoice inv= null;
//			using (var db = new SQLite.SQLiteConnection (pathToDatabase)){
//				var mls= db.Table<Invoice> ().Where (x => x.invno==INVOICENO).ToList<Invoice>();
//				var ls= db.Table<InvoiceDtls> ().Where (x => x.invno==INVOICENO).ToList<InvoiceDtls>();
//				if (mls.Count > 0) {
//					inv = mls [0];
//				}
//				list = new InvoiceDtls[ls.Count];
//				ls.CopyTo (list);
//			}
//			if (inv == null)
//				return;
//			
//			//mmDevice = null;
//			//findBTPrinter ();
//			IPrintDocument prtInv = PrintDocManager.GetPrintDocument<PrintInvoice>();
//			prtInv.SetDocument (inv);
//			prtInv.SetDocumentDtls(list);
//			prtInv.SetNoOfCopy (1);
//			prtInv.SetCallingActivity (this);
//			if (prtInv.StartPrint ()) {
//				updatePrintedStatus (inv);
//
//			} else {
//				Toast.MakeText (this, prtInv.GetErrMsg(), ToastLength.Long).Show ();	
//			}
//		}
//
//		void updatePrintedStatus(Invoice inv)
//		{
//			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
//				var list = db.Table<Invoice> ().Where (x => x.invno == inv.invno).ToList<Invoice> ();
//				if (list.Count > 0) {
//					//if only contains items then allow to update the printed status.
//					//this to allow the invoice;s item can be added. if not can not be posted(upload)
//					var list2 = db.Table<InvoiceDtls> ().Where (x => x.invno == inv.invno).ToList<InvoiceDtls> ();
//					if (list2.Count > 0) {
//						list [0].isPrinted = true;
//						db.Update (list [0]);
//					}
//				}
//			}
//		}
//
//		void ShowKeyBoard(View view)
//		{
//			InputMethodManager imm = (InputMethodManager)Activity.GetSystemService(Context.InputMethodService);
//			imm.ShowSoftInputFromInputMethod(view.WindowToken, ShowFlags.Forced);
//			imm.ToggleSoftInput (ShowFlags.Forced, 0); 
//		}
//
//		void TxtCash_AfterTextChanged (object sender, Android.Text.AfterTextChangedEventArgs e)
//		{
//			CalChanges ();
//		}
//
//		private void HandleEditorAction(object sender, TextView.EditorActionEventArgs e)
//		{
//			e.Handled = false;
//			if ((e.ActionId == ImeAction.Done)||(e.ActionId == ImeAction.Next))
//			{
//				CalChanges ();
//				e.Handled = true;   
//				//				View view = sender as View;
//				//				InputMethodManager imm = (InputMethodManager)Activity.GetSystemService(Context.InputMethodService);
//				//				imm.HideSoftInputFromWindow(view.WindowToken, 0);
//			}
//		}
//
//		private void CalChanges()
//		{
//			try{
//				double cash = Convert.ToDouble (txtCash.Text);
//				double total = Convert.ToDouble (txtTotal.Text);
//				double change = cash - total;
//				//	txtCash.Text = cash.ToString("n2");
//				txtChange.Text = change.ToString ("n2");
//			}catch
//			{}
//
//		}
//	}
//}

