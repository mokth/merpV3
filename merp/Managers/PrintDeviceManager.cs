using System;
using Android.App;

namespace wincom.mobile.erp
{
	public class PrintDeviceManager
	{
		public static IPrintToDevice GetPrintingDevice<T>()
		{
			string USERDEFINE = ((GlobalvarsApp)Application.Context).USERFUNCTION;
			string pathToDatabase = ((GlobalvarsApp)Application.Context).DATABASE_PATH;
			IPrintToDevice  intent = null;
			AdPara para= DataHelper.GetAdPara (pathToDatabase);

			if (para.PrinterType == "Network POS Printer") {
				intent = GetPrintingDeviceEx<TCPDeviceHelper> ();
			} else {
				intent = GetPrintingDeviceEx<T> ();
			}
			return intent;
		}

		private static IPrintToDevice GetPrintingDeviceEx<T>()
		{
			string USERDEFINE = ((GlobalvarsApp)Application.Context).USERFUNCTION;
			IPrintToDevice  intent = null;
			string classname = "wincom.mobile.erp" + "." + typeof(T).Name + "_" + USERDEFINE;
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
	
//	public class PrintDeviceManager
//	{
//		public static IPrintToDevice GetPrintingDevice<T>()
//		{
//			string USERDEFINE = ((GlobalvarsApp)Application.Context).USERFUNCTION;
//			IPrintToDevice  intent = null;
//			string classname = "wincom.mobile.erp" + "." + typeof(T).Name + "_" + USERDEFINE;
//			try {
//				Type cType = Type.GetType (classname, false, true);
//				if (cType != null) {
//					intent = (IPrintToDevice )Activator.CreateInstance (cType);
//				} else {
//					intent = (IPrintToDevice )Activator.CreateInstance (typeof(T));
//				} 
//			} catch {
//
//				if (intent == null)
//					intent = (IPrintToDevice )Activator.CreateInstance (typeof(T));
//			}
//			return intent;
//		}
//	}
}

