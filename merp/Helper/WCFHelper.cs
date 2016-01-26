using System;
using System.ServiceModel;

namespace wincom.mobile.erp
{
	public class WCFHelper
	{
		public readonly EndpointAddress EndPoint;
		private Service1Client _client;

		public WCFHelper()
		{
			//Demo/testing -test version											
			EndPoint = new EndpointAddress("http://www.wincomcloud.com/WcfV5Demo/Service1.svc");
		
			//production -live version
			//EndPoint = new EndpointAddress ("http://www.wincomcloud.com/WfcV5Live/Service1.svc");

		}

		public static string GetDownloadDBUrl()
		{
			//production -live version
			return "http://www.wincomcloud.com/wfcv3Live/dbfiles/";

			//Demo/testing -test version
			//return "http://www.wincomcloud.com/wfcv3/dbfiles/";
		}

		public static string GeUploadDBUrl()
		{
			return "http://www.wincomcloud.com/UploadDb/";
		}

		public static string GeUploadApkUrl()
		{
			return "http://www.wincomcloud.com/wfcv3Live/apks/";
		}

		public static string GeUploadZipDBUrl()
		{
			return "http://www.wincomcloud.com/UploadDbEx/";
		}


		public static string GetDownloadTemplateUrl()
		{
			//production -live version
			return "http://www.wincomcloud.com/wfcv3Live/templates/";

			//Demo/testing -test version
			//return "http://www.wincomcloud.com/wfcv3/dbfiles/";
		}

		public  Service1Client GetServiceClient()
		{
			try {
				InitializeServiceClient ();
			} catch {
				_client = null;
			}
			return _client;
		}

		private void InitializeServiceClient()
		{
			BasicHttpBinding binding = CreateBasicHttp();

			_client = new Service1Client(binding, EndPoint);

		}

		private static BasicHttpBinding CreateBasicHttp()
		{
			BasicHttpBinding binding = new BasicHttpBinding
			{
				Name = "basicHttpBinding",
				MaxBufferSize = 2147483647,
				MaxReceivedMessageSize = 2147483647
				//MaxBufferPoolSize=2147483647
        	};
			TimeSpan timeout = new TimeSpan(0, 1, 0);
			binding.SendTimeout = timeout;
			binding.OpenTimeout = timeout;
			binding.ReceiveTimeout = timeout;
		    
			return binding;
		}
	}
}

