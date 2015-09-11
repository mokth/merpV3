using System;
using System.Collections;
using Android.App;

namespace wincom.mobile.erp
{
	public interface IPrintToDevice
	{
		bool StartPrint (string text, int noofcopy, ref string errmsg);
		void SetCallingActivity(Activity activity);
		void  SetIsPrintCompLogo (bool  iSPrintCompLogo);
	}


}

