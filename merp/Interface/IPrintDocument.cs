using System;
using System.Collections;
using Android.App;

namespace wincom.mobile.erp
{
	public interface IPrintDocument
	{
		void SetCallingActivity(Activity activity);
		bool StartPrint ();
		void SetDocument(object doc);
		void SetDocumentDtls(object docdtls);
		void SetNoOfCopy(int noofcopy);
		void SetExtraPara(Hashtable para);
		string GetErrMsg();
	}
}

