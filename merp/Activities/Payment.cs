
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
using Android.Views.InputMethods;

namespace wincom.mobile.erp
{
	[Activity (Label = "PAYMENT")]			
	public class Payment : Activity
	{
		double amount =0;
		EditText txtAmt;
		EditText txtRound;
		EditText txtTotal;
		EditText txtCash;
		EditText txtChange;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			if (!((GlobalvarsApp)this.Application).ISLOGON) {
				Finish ();
			}
			SetTitle (Resource.String.title_payment);
			SetContentView (Resource.Layout.Payment);
			string samt = Intent.GetStringExtra ("amount") ?? "0";
			amount = Convert.ToDouble (samt);

			txtAmt = FindViewById<EditText> (Resource.Id.payamt);
			txtRound = FindViewById<EditText> (Resource.Id.payround);
			txtTotal = FindViewById<EditText> (Resource.Id.paytotal);
			txtCash = FindViewById<EditText> (Resource.Id.paycash);
			txtChange = FindViewById<EditText> (Resource.Id.paychange);
			txtAmt.Text = amount.ToString ("n2");
			double roundVal=0;
			double ttlAmt = Utility.AdjustToNear (amount, ref roundVal);
			txtRound.Text = roundVal.ToString ("n2");
			txtTotal.Text = ttlAmt.ToString ("n2");
			txtCash.EditorAction += HandleEditorAction;
			txtCash.AfterTextChanged += TxtCash_AfterTextChanged;
			Button butInvBack= FindViewById<Button> (Resource.Id.payok); 
			butInvBack.Click += (object sender, EventArgs e) => {
				base.OnBackPressed ();
			};
			txtCash.RequestFocus ();
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

