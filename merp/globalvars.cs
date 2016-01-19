﻿using System;
using Android.App;
using Android.Runtime;
using System.IO;

namespace wincom.mobile.erp
{
	[Application(Label = "M-ERP V5")]
	public class GlobalvarsApp:Application
	{
		public string COMPANY_CODE;
		public string BRANCH_CODE;
		public string USERID_CODE;
		public string DATABASE_PATH;
		public string USERFUNCTION;
		public string VERSION;
		public bool ISLOGON;

		public GlobalvarsApp(IntPtr handle, JniHandleOwnership transfer)
			: base(handle, transfer)
		{
		}

		public override void OnCreate()
		{
			string path = Path.Combine (Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "erpdata");
			if (!Directory.Exists (path))
				Directory.CreateDirectory (path);
			//var documents = System.Environment.GetFolderPath (System.Environment.GetFolderPath());
			//DATABASE_PATH = Path.Combine (path, "db_adonet.db");
			var documents = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
			DATABASE_PATH = Path.Combine (documents, "db_adonet.db");
			base.OnCreate();
		}

	}
}

