using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System.IO;
using Android.Graphics.Drawables;
using System.Collections;
using Android.Views.InputMethods;

namespace wincom.mobile.erp
{
	
	public class CashDialog : DialogFragment
	{
		EditText txtAmt;
		EditText txtRound;
		EditText txtTotal;
		EditText txtCash;
		EditText txtChange;
		EditText txtCust;
		EditText txtRemark;
		EditText txtInvno;

		string pathToDatabase;

		static int eventid ;
	    
		public static CashDialog NewInstance()
		{
	    	var dialogFragment = new CashDialog();
			return dialogFragment;
		}
		private double _amount;
		public double Amount
		{
			get { return _amount;}
			set {  _amount =value;}
		}

		private string _custname;
		public string CustName
		{
			get { return _custname;}
			set {  _custname =value;}
		}

		private string _invno;
		public string InvNo
		{
			get { return _invno;}
			set {  _invno =value;}
		}

		private string _remark;
		public string Remark
		{
			get { return _remark;}
			set {  _remark =value;}
		}

		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Create your fragment here
		}
		public override Dialog OnCreateDialog(Bundle savedInstanceState)
		{ 
			base.OnCreate(savedInstanceState);
			// Begin building a new dialog.
			var builder = new AlertDialog.Builder(Activity);
			//Get the layout inflater
			var inflater = Activity.LayoutInflater;

			var view = inflater.Inflate(Resource.Layout.Payment, null);
			txtAmt = view.FindViewById<EditText> (Resource.Id.payamt);
			if (txtAmt != null) {
				txtRound = view.FindViewById<EditText> (Resource.Id.payround);
				txtTotal = view.FindViewById<EditText> (Resource.Id.paytotal);
				txtCash = view.FindViewById<EditText> (Resource.Id.paycash);
				txtChange = view.FindViewById<EditText> (Resource.Id.paychange);
				Button butInvBack = view.FindViewById<Button> (Resource.Id.payok); 
				txtCust = view.FindViewById<EditText> (Resource.Id.newinv_custname);
				txtRemark = view.FindViewById<EditText> (Resource.Id.newinv_remark);
				txtInvno = view.FindViewById<EditText> (Resource.Id.newinvno);

				txtRemark.Text = Remark;
				txtInvno.Text = InvNo;
				txtCust.Text = CustName;
				txtAmt.Text = _amount.ToString ("n2");
				double roundVal = 0;
				double ttlAmt = Utility.AdjustToNear (_amount, ref roundVal);
				txtRound.Text = roundVal.ToString ("n2");
				txtTotal.Text = ttlAmt.ToString ("n2");
				txtCash.EditorAction += HandleEditorAction;
				txtCash.AfterTextChanged += TxtCash_AfterTextChanged;
				txtCash.Text = "0";
				butInvBack.Visibility = ViewStates.Gone;

				builder.SetView (view);
				builder.SetPositiveButton ("PAID/PRINT", HandlePositiveButtonClick);
				builder.SetNegativeButton ("PAID",HandlePaidOnlyButtonClick);
				builder.SetNeutralButton ("CANCEL",  HandleNegativeButtonClick );

				txtCash.RequestFocus ();
				ShowKeyBoard (view);
			}
			var dialog = builder.Create();
			//Now return the constructed dialog to the calling activity
			return dialog;
		}

		void ShowKeyBoard(View view)
		{
			InputMethodManager imm = (InputMethodManager)Activity.GetSystemService(Context.InputMethodService);
			imm.ShowSoftInputFromInputMethod(view.WindowToken, ShowFlags.Forced);
			imm.ToggleSoftInput (ShowFlags.Forced, 0); 
		}

		private void HandlePositiveButtonClick(object sender, DialogClickEventArgs e)
		{
			var dialog = (AlertDialog) sender;
			Hashtable param = new Hashtable ();
			param.Add ("print","yes");
			param.Add ("remark", txtRemark.Text.ToUpper ());
			EventParam p = new EventParam (EventID.PAYMENT_PAID , param);
			EventManagerFacade.Instance.GetEventManager ().PerformEvent (this.Activity, p);

			dialog.Dismiss();
		}
		private void  HandlePaidOnlyButtonClick(object sender, DialogClickEventArgs e)
		{
			var dialog = (AlertDialog)sender;
			Hashtable param = new Hashtable ();
			param.Add ("print","no");
			param.Add ("remark", txtRemark.Text.ToUpper ());
			EventParam p = new EventParam (EventID.PAYMENT_PRINT , param);
			EventManagerFacade.Instance.GetEventManager ().PerformEvent (this.Activity, p);
			dialog.Dismiss ();
		}

		private void  HandleNegativeButtonClick(object sender, DialogClickEventArgs e)
		{
			var dialog = (AlertDialog)sender;
			dialog.Dismiss ();
		}

		void TxtCash_AfterTextChanged (object sender, Android.Text.AfterTextChangedEventArgs e)
		{
			CalChanges ();
		}

		private void HandleEditorAction(object sender, TextView.EditorActionEventArgs e)
		{
			e.Handled = false;
			if ((e.ActionId == ImeAction.Done)||(e.ActionId == ImeAction.Next))
			{
				CalChanges ();
				e.Handled = true;   
//				View view = sender as View;
//				InputMethodManager imm = (InputMethodManager)Activity.GetSystemService(Context.InputMethodService);
//				imm.HideSoftInputFromWindow(view.WindowToken, 0);
			}
		}

		private void CalChanges()
		{
			try{
				double cash = Convert.ToDouble (txtCash.Text);
				double total = Convert.ToDouble (txtTotal.Text);
				double change = cash - total;
				//	txtCash.Text = cash.ToString("n2");
				txtChange.Text = change.ToString ("n2");
			}catch
			{}

		}
	}
}

