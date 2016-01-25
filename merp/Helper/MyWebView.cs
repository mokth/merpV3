using System;
using Android.Webkit;

namespace wincom.mobile.erp
{
	public class myWebView:WebViewClient
	{
		public override bool ShouldOverrideUrlLoading (WebView view, string url)
		{
			view.LoadUrl(url);
			return false;
		}
	}
}

