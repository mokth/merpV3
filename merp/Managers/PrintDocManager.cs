using System;
using Android.App;

namespace wincom.mobile.erp
{
	public class PrintDocManager
	{
		public static IPrintDocument GetPrintDocument<T>()
		{
			string USERDEFINE = ((GlobalvarsApp)Application.Context).USERFUNCTION;
			IPrintDocument  intent = null;
			string classname = "wincom.mobile.erp" + "." + typeof(T).Name + "_" +USERDEFINE;
			try {
				Type cType = Type.GetType (classname, false, true);
				if (cType != null) {
					intent = (IPrintDocument )Activator.CreateInstance (cType);
				} else {
					intent = (IPrintDocument )Activator.CreateInstance (typeof(T));
				} 
			} catch {

				if (intent == null)
					intent = (IPrintDocument )Activator.CreateInstance (typeof(T));
			}
			return intent;
		}
	}
}

