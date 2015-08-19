using System;
using Android.App;
using System.IO;
using System.Text;

namespace wincom.mobile.erp
{
	public abstract class PrintHelperBase
	{
		internal  string USERID;
		internal AdPara apara;
		internal CompanyInfo compinfo;
		internal string msg;
		internal string pathToDatabase;

		public PrintHelperBase()
		{
			pathToDatabase = ((GlobalvarsApp)Application.Context).DATABASE_PATH;
			USERID = ((GlobalvarsApp)Application.Context).USERID_CODE;
			apara =  DataHelper.GetAdPara (pathToDatabase);
			compinfo =DataHelper.GetCompany(pathToDatabase);
		}

		internal void PrintLongText(ref string test,string text)
		{
			if (text.Length > 42) {

				string temp = text.Substring(0,42);
				int pos = temp.LastIndexOf(" ");
				string line1 = text.Substring(0, pos);
				string line2 = text.Substring(pos);
				test = test + line1.Trim() + "\n";
				test = test + line2.Trim() + "\n";
			} else {

				test = test + text + "\n";
			}
		}

		internal void PrintLine(string text,Stream mmOutputStream)
		{
			byte[] cc = Encoding.ASCII.GetBytes (text);
			mmOutputStream.Write (cc, 0, cc.Length);

		}
	}
}

