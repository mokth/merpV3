using System;
using System.Globalization;
using System.Reflection;
using Android.Bluetooth;
using Android.App;
using Android.Content;

namespace wincom.mobile.erp
{
	public class Utility:Activity
	{/// <summary>
	/// Converts to date.
	/// </summary>
	/// <returns>The to date.</returns>
	/// <param name="sdate">Sdate.</param>
		public static DateTime ConvertToDate(string sdate)
		{
			DateTime date = DateTime.Today;  
			string[] para = sdate.Split(new char[]{'-'});
			if (para.Length > 2) {
				int yy = Convert.ToInt32 (para [2]);
				int mm = Convert.ToInt32 (para [1]);
				int dd = Convert.ToInt32 (para [0]);

				date = new DateTime (yy, mm, dd);
			}

			return date;
		}
		/// <summary>
		/// Gets the date range for 3 months (start from last 3 month).
		/// </summary>
		/// <param name="sdate">Sdate.</param>
		/// <param name="edate">Edate.</param>
		public static void GetDateRange (ref DateTime sdate,ref DateTime edate)
		{
			DateTime today = DateTime.Today;
			sdate = new DateTime (today.Year, today.Month , 1);
			sdate = sdate.AddMonths (-3);
			edate = today.AddMonths (1).AddDays (-1);
		}

		public static bool IsValidDateString (string datestr)
		{   
			bool valid = false;
			DateTime sdate;
			CultureInfo culture;
			DateTimeStyles styles;
			culture = CultureInfo.CreateSpecificCulture("en-GB"); 
			styles = DateTimeStyles.None;

			if (DateTime.TryParse (datestr, culture, styles, out sdate)) {
				valid = true;
			}

			return valid;
		}
		public static AccessRights GetAccessRights()
		{
			string pathToDatabase = ((GlobalvarsApp)Application.Context).DATABASE_PATH;
			return  GetAccessRights (pathToDatabase);
		}

		/// <summary>
		/// Gets the access rights.
		/// </summary>
		/// <returns>The access rights.</returns>
		/// <param name="pathToDatabase">Path to database.</param>
		public static AccessRights GetAccessRights(string pathToDatabase)
		{
			CompanyInfo info = new CompanyInfo();
			using (var db = new SQLite.SQLiteConnection (pathToDatabase)) {
				info = db.Table<CompanyInfo> ().FirstOrDefault ();
			}

			string rights = info.WCFUrl;
			AccessRights right = new AccessRights();
			Type rtype = right.GetType();

			AccessRightField rightdesc = new AccessRightField();
			Type ftype = rightdesc.GetType();
			string[] keys = rights.Split(new char[] { ',' });
			foreach (string key in keys)
			{
				FieldInfo finfo = ftype.GetField(key);
				if (finfo != null)
				{
					string fname=  finfo.GetValue(rightdesc).ToString();
					PropertyInfo pinfo = rtype.GetProperty(fname);
					if (pinfo != null)
					{
						pinfo.SetValue(right, true);
					}
				}
			}

			return right;

		}

		public static double AdjustToNear(double amount,ref double roundVal)
		{
			double amt = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
			double intval = Math.Truncate(amt);
			string decPlaces = amt.ToString("n2");
			string[] digits = decPlaces.Split(new char[] { '.' });
			int num0 = Convert.ToInt32(digits[1][0].ToString());
			int num1 = Convert.ToInt32(digits[1][1].ToString());
			string dec = "0";
			string rdec = "0";
			double diff = 0;
			bool isUp = false;
			if (num1 > 2 && num1 <= 5)
			{
				dec = string.Format("0.{0}{1}", num0, 5);
				rdec = string.Format("0.0{0}", 5-num1);
				isUp = true;
			}
			if (num1 <= 2)
			{
				dec = string.Format("0.{0}{1}", num0, 0);
				rdec = string.Format("0.0{0}",  num1);
				isUp = false;
			}
			if (num1 > 5 && num1 <= 7)
			{
				dec = string.Format("0.{0}{1}", num0, 5);
				rdec = string.Format("0.0{0}", num1-5);
				isUp = false;
			}
			if (num1 > 7)
			{
				dec = string.Format("0.{0}{1}", num0 + 1, 0);
				rdec = string.Format("0.0{0}", 10-num1);
				isUp = true;
			}


			roundVal = Convert.ToDouble(rdec) * ((!isUp)?-1:1);
			double decval = Convert.ToDouble(dec);
			intval = intval + decval;
			return intval;
		}

		public  BluetoothDevice FindBTPrinter(string printername,ref string msg){
			BluetoothAdapter mBluetoothAdapter =null;
			BluetoothDevice mmDevice=null;
			try{
				mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;

				if (mBluetoothAdapter ==null)
				{
					msg = Resources.GetString(Resource.String.msg_bluetootherror);	
					return  mmDevice;
				}
				string txt ="";
				if (!mBluetoothAdapter.Enable()) {
					Intent enableBluetooth = new Intent(
						BluetoothAdapter.ActionRequestEnable);
					StartActivityForResult(enableBluetooth, 0);
				}

				var pair= mBluetoothAdapter.BondedDevices;
				if (pair.Count > 0) {
					foreach (BluetoothDevice dev in pair) {
						Console.WriteLine (dev.Name);
						txt = txt+","+dev.Name;
						if (dev.Name.ToUpper()==printername)
						{
							mmDevice = dev;
							break;
						}
					}
				}
				msg = Resources.GetString(Resource.String.msg_bluetoothfound) +mmDevice.Name;
				//Toast.MakeText(this, "found device " +mmDevice.Name, ToastLength.Long).Show ();	

			}catch(Exception ex) {

				//Toast.MakeText (this, "Error in Bluetooth device. Try again.", ToastLength.Long).Show ();	
				msg =Resources.GetString(Resource.String.msg_bluetootherror);
			}

			return mmDevice;
		}

		public static double GetUnitPrice (Trader trd,Item prd)
		{
			double unitprice = 0;
			if (trd == null) {
				unitprice = prd.Price;
				return unitprice;
			}

			if (trd.CustType.ToUpper () == "OTHER") {
				unitprice = prd.Price;
			} else if (trd.CustType.ToUpper () == "RETAIL") {
				unitprice = prd.RetailPrice;
			} else if (trd.CustType.ToUpper () == "VIP") {
				unitprice = prd.VIPPrice;
			} else if (trd.CustType.ToUpper () == "WHOLESALE") {
				unitprice = prd.WholeSalePrice;
			} else {
				unitprice = prd.Price;
			}

			//if only SellingPirce is set, take selling price.
			if (unitprice==0)
				unitprice = prd.Price;

			return unitprice;
		}
	}
}

