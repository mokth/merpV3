using System;
using Android.App;

namespace wincom.mobile.erp
{
	

	public class PrintCompanyHeader:PrintHelperBase
	{
		public void PrintCompHeader (ref string test)
		{
			//string pathToDatabase = ((GlobalvarsApp)Application.Context).DATABASE_PATH;
			CompanyInfo comp= DataHelper.GetCompany (pathToDatabase);
			if (comp == null)
				return;
			string tel = string.IsNullOrEmpty (comp.Tel) ? " " : comp.Tel.Trim ();
			string fax = string.IsNullOrEmpty (comp.Fax) ? " " : comp.Fax.Trim ();
			string addr1 =string.IsNullOrEmpty (comp.Addr1) ? "" : comp.Addr1.Trim ();
			string addr2 =string.IsNullOrEmpty (comp.Addr2) ? "" : comp.Addr2.Trim ();
			string addr3 =string.IsNullOrEmpty (comp.Addr3) ? "" : comp.Addr3.Trim ();
			string addr4 =string.IsNullOrEmpty (comp.Addr4) ? "" : comp.Addr4.Trim ();
			string gst =string.IsNullOrEmpty (comp.GSTNo) ? "" : comp.GSTNo.Trim ();
			string compname = comp.CompanyName.Trim ();
			string[] names =compname.Split (new char[]{ '|' });
			if (names.Length > 1) {
				test += names [0] + "\n";

				if ((names [1].Trim ().Length + comp.RegNo.Trim ().Length + 2) > 42) {
					//test += names [1].Trim () + "\n";
					//test += "(" + comp.RegNo.Trim () + ")\n";
					PrintLongText (ref test, names [1].Trim () + "(" + comp.RegNo.Trim () + ")");
				} else {
					test += names [1].Trim () + "(" + comp.RegNo.Trim () + ")\n";
				}
			} else {
				if ((comp.CompanyName.Trim ().Length + comp.RegNo.Trim ().Length + 2) > 42) {
					test += comp.CompanyName.Trim () + "\n";
					test += "(" + comp.RegNo.Trim () + ")\n";
				} else {
					test += comp.CompanyName.Trim () + "(" + comp.RegNo.Trim () + ")\n";
				}
			}
			if (addr1!="")
				test += comp.Addr1.Trim () + "\n";	
			if (addr2!="")
				test += comp.Addr2.Trim () + "\n";	
			if (addr3!="")
				test += comp.Addr3.Trim () + "\n";	
			if (addr4!="")
				test += comp.Addr4.Trim () + "\n";	

			test += "TEL:" + tel+"  FAX:"+fax+"\n";
			test += "GST NO:" + gst+"\n";

		}
	}
}

