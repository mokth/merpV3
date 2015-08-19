using System;
using Android.App;

namespace wincom.mobile.erp
{
	public class PrintDeviceManager
	{
		public static IPrintToDevice GetPrintingDevice<T>()
		{
			string COMPCODE = ((GlobalvarsApp)Application.Context).COMPANY_CODE;
			IPrintToDevice  intent = null;
			string classname = "wincom.mobile.erp" + "." + typeof(T).Name + "_" + COMPCODE;
			try {
				Type cType = Type.GetType (classname, false, true);
				if (cType != null) {
					intent = (IPrintToDevice )Activator.CreateInstance (cType);
				} else {
					intent = (IPrintToDevice )Activator.CreateInstance (typeof(T));
				} 
			} catch {

				if (intent == null)
					intent = (IPrintToDevice )Activator.CreateInstance (typeof(T));
			}
			return intent;
		}
	}
}

