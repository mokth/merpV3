using System;
using Android.App;

namespace wincom.mobile.erp
{
	public class PrintDocManager
	{
		public static IPrintDocument GetPrintDocument<T>()
		{
			//string USERDEFINE = ((GlobalvarsApp)Application.Context).USERFUNCTION;
			string pathToDatabase = ((GlobalvarsApp)Application.Context).DATABASE_PATH;
			IPrintDocument  intent = null;
			string classname = "";

			AdPara para= DataHelper.GetAdPara (pathToDatabase);

			if (para.PaperSize == "8.5Inch") {
				if (para.PrinterType == "Network Laserjet Printer")
					intent = GetPrintDocumentEX<PrintPCLInvoice> ();
				else intent = GetPrintDocumentEX<PrintDMInvoice> ();
			} else {
				intent = GetPrintDocumentEX<T> ();
			}

			return intent;
		}

		public static IPrintDocument GetPrintCNDocument<T>()
		{
			//string USERDEFINE = ((GlobalvarsApp)Application.Context).USERFUNCTION;
			string pathToDatabase = ((GlobalvarsApp)Application.Context).DATABASE_PATH;
			IPrintDocument  intent = null;
			string classname = "";

			AdPara para= DataHelper.GetAdPara (pathToDatabase);

			if (para.PaperSize == "8.5Inch") {
				if (para.PrinterType == "Network Laserjet Printer")
					intent = GetPrintDocumentEX<PrintPCLCNote> ();
				else intent = GetPrintDocumentEX<PrintDMCNote> ();
			} else {
				intent = GetPrintDocumentEX<T> ();
			}

			return intent;
		}

		public static IPrintDocument GetPrintSummary<T>()
		{
			//string USERDEFINE = ((GlobalvarsApp)Application.Context).USERFUNCTION;
			string pathToDatabase = ((GlobalvarsApp)Application.Context).DATABASE_PATH;
			IPrintDocument  intent = null;
			string classname = "";

			AdPara para= DataHelper.GetAdPara (pathToDatabase);

			if (para.PaperSize == "8.5Inch") {
				if (para.PrinterType == "Network Laserjet Printer")
					intent = GetPrintDocumentEX<PrintPCLSumm> ();
				else intent = GetPrintDocumentEX<PrintESCSumm> ();
			} else {
				intent = GetPrintDocumentEX<T> ();
			}

			return intent;
		}

		private static IPrintDocument GetPrintDocumentEX<T>()
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
//	public class PrintDocManager
//	{
//		public static IPrintDocument GetPrintDocument<T>()
//		{
//			string USERDEFINE = ((GlobalvarsApp)Application.Context).USERFUNCTION;
//			IPrintDocument  intent = null;
//			string classname = "wincom.mobile.erp" + "." + typeof(T).Name + "_" +USERDEFINE;
//			try {
//				Type cType = Type.GetType (classname, false, true);
//				if (cType != null) {
//					intent = (IPrintDocument )Activator.CreateInstance (cType);
//				} else {
//					intent = (IPrintDocument )Activator.CreateInstance (typeof(T));
//				} 
//			} catch {
//
//				if (intent == null)
//					intent = (IPrintDocument )Activator.CreateInstance (typeof(T));
//			}
//			return intent;
//		}
//	}
}

