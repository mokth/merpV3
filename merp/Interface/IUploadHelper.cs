using System;
using Android.App;

namespace wincom.mobile.erp
{
	public delegate void OnUploadDoneDlg(Activity activity, int count,string msg);

	public interface IUploadHelper
	{
		void SetUploadHandel(OnUploadDoneDlg Uploadhandle);
		void SetCallingActivity(Activity CallingActivity);
		void startUpload();
	}
}

