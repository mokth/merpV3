
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

namespace wincom.mobile.erp
{
	public delegate bool IsValidTraderFilter(Trader trd);
	public class TraderDialog : DialogFragment
	{
		ListView listView ;
		List<Trader> listData = new List<Trader> ();
		string pathToDatabase;
		GenericListAdapter<Trader> adapter; 
		EditText  txtSearch;
		Button butSearch;
		SetViewDlg viewdlg;
		string _selectedItem;

		static IsValidTraderFilter _filter;
		static int eventid ;
	    
		public static TraderDialog NewInstance()
		{
			var dialogFragment = new TraderDialog();
			_filter = null;
			eventid = EventID.CUSTCODE_SELECTED;
			return dialogFragment;
		}


		public static TraderDialog NewInstance(IsValidTraderFilter filter,int selectdEventid)
		{
			var dialogFragment = new TraderDialog();
			_filter = filter;
			eventid = selectdEventid;
			return dialogFragment;
		}

		public string SelectedItem
		{
			get { return _selectedItem;}
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
			populate (listData);
			viewdlg = SetViewDelegate;
			var view = inflater.Inflate(Resource.Layout.ListCustView, null);
			listView = view.FindViewById<ListView> (Resource.Id.CustList);
			if (listView != null) {
				adapter = new GenericListAdapter<Trader> (this.Activity, listData,Resource.Layout.ListCustDtlView, viewdlg);
				listView.Adapter = adapter;
				listView.ItemClick += ListView_Click;
				txtSearch= view.FindViewById<EditText > (Resource.Id.txtSearch);
				butSearch= view.FindViewById<Button> (Resource.Id.butCustBack); 
				butSearch.Text = "SEARCH";
				butSearch.SetCompoundDrawables (null, null, null, null);
				butSearch.Click+= ButSearch_Click;
				//txtSearch.TextChanged += TxtSearch_TextChanged;
				builder.SetView (view);
				builder.SetPositiveButton ("CANCEL", HandlePositiveButtonClick);
			}
			var dialog = builder.Create();
			//Now return the constructed dialog to the calling activity
			return dialog;
		}

		void ButSearch_Click (object sender, EventArgs e)
		{
			FindTraderByText ();
		}

		private void HandlePositiveButtonClick(object sender, DialogClickEventArgs e)
		{
			var dialog = (AlertDialog) sender;
			dialog.Dismiss();
		}
		private void  HandleNegativeButtonClick(object sender, DialogClickEventArgs e)
		{
			var dialog = (AlertDialog) sender;
			dialog.Dismiss();
		}
		void FindTraderByText ()
		{
			List<Trader> found = PerformSearch (txtSearch.Text);
			adapter = new GenericListAdapter<Trader> (this.Activity, found, Resource.Layout.ListCustDtlView, viewdlg);
			listView.Adapter = adapter;
		}

		void TxtSearch_TextChanged (object sender, Android.Text.TextChangedEventArgs e)
		{
			FindTraderByText ();
		}

		void ListView_Click (object sender, AdapterView.ItemClickEventArgs e)
		{
			Trader itm = adapter[e.Position];
			if (itm != null) {
				_selectedItem = itm.CustCode + " | " + itm.CustName.Trim();
				Hashtable param = new Hashtable ();
				param.Add ("SELECTED", _selectedItem);
				//int eventID = (_filter == null ? EventID.CUSTCODE_SELECTED : EventID.CUSTCODE2_SELECTED);
				EventParam p = new EventParam (eventid , param);
				EventManagerFacade.Instance.GetEventManager ().PerformEvent (this.Activity, p);
			}
			this.Dialog.Dismiss ();
		}

		private void SetViewDelegate(View view,object clsobj)
		{
			Trader item = (Trader)clsobj;
			view.FindViewById<TextView> (Resource.Id.custcode).Text = item.CustCode;
			view.FindViewById<TextView> (Resource.Id.custname).Text = item.CustName;
		}

		List<Trader> PerformSearch (string constraint)
		{
			List<Trader>  results = new List<Trader>();
			if (constraint != null) {
				var searchFor = constraint.ToString ().ToUpper();
				Console.WriteLine ("searchFor:" + searchFor);

				foreach(Trader itm in listData)
				{
					if (itm.CustCode.ToUpper().IndexOf (searchFor) >= 0) {
						results.Add (itm);
						continue;
					}
					if (itm.CustName.ToUpper().IndexOf (searchFor) >= 0) {
						results.Add (itm);
						continue;
					}

					if (itm.Addr1.ToUpper().IndexOf (searchFor) >= 0) {
						results.Add (itm);
						continue;
					}
					if (itm.Addr2.ToUpper().IndexOf (searchFor) >= 0) {
						results.Add (itm);
						continue;
					}
					if (itm.Addr3.ToUpper().IndexOf (searchFor) >= 0) {
						results.Add (itm);
						continue;
					}
					if (itm.Addr4.ToUpper().IndexOf (searchFor) >= 0) {
						results.Add (itm);
						continue;
					}
					if (itm.Tel.ToUpper().IndexOf (searchFor) >= 0) {
						results.Add (itm);
						continue;
					}
					if (itm.Fax.ToUpper().IndexOf (searchFor) >= 0) {
						results.Add (itm);
						continue;
					}

					if (itm.AgentCode != null) {
						if (itm.AgentCode.ToUpper ().IndexOf (searchFor) >= 0) {
							results.Add (itm);
							continue;
						}
					}
				}
			}
			return results;
		}

		void populate(List<Trader> list)
		{

			var documents = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
			pathToDatabase = Path.Combine(documents, "db_adonet.db");

			//SqliteConnection.CreateFile(pathToDatabase);
			using (var db = new SQLite.SQLiteConnection(pathToDatabase))
			{

				var list2 = db.Table<Trader>().ToList<Trader>();
				if (_filter == null) {
					foreach (var item in list2) {
						list.Add (item);
					}
				} else {
					foreach (var item in list2) {
					    	
						if (_filter.Invoke(item))
							list.Add (item);
					}
				}
			}
		}

	}
}

