using System;
using System.Runtime.Remoting.Contexts;
using Android.App;

namespace wincom.mobile.erp
{
	public class UploadManager
	{
		public static IUploadHelper GetUploadHepler<T>()
		{
			string USERDEFINE = ((GlobalvarsApp)Application.Context).USERFUNCTION;
			IUploadHelper intent = null;
			string classname = "wincom.mobile.erp" + "." +  typeof(T).Name+"_"+USERDEFINE;
			try {
				Type cType = Type.GetType (classname, false, true);
				if (cType != null) {
					intent = (IUploadHelper)Activator.CreateInstance (cType);
				} else {
					intent = (IUploadHelper)Activator.CreateInstance (typeof(T));
				} 
			} catch {

				if (intent == null)
					intent = (IUploadHelper)Activator.CreateInstance (typeof(T));
			}
			return intent;
		}

	}
}

