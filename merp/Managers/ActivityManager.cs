using System;
using Android.Content;
using Android.App;

namespace wincom.mobile.erp
{
	public class ActivityManager:Activity
	{
		
		public static Intent GetActivity<T>(Context context)
		{
			string COMPCODE = ((GlobalvarsApp)Application.Context).COMPANY_CODE;
			Intent intent = null;
			string classname = "wincom.mobile.erp" + "." + typeof(T).Name+"_"+COMPCODE ;
			try {
				Type cType = Type.GetType (classname, false, true);
				if (cType != null) {
					intent = new Intent (context, cType);
				} else {
					intent = new Intent (context, typeof(T));
				}
			} catch {
			
				if (intent == null)
					intent = new Intent (context, typeof(T));
			}
			return intent;
		}


	}
}

