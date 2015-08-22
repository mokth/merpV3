using System;

namespace wincom.mobile.erp
{
	public class PrintCustomerHeader:PrintHelperBase
	{
		public void PrintCustomer (ref string test,string custcode,string title ="CUSTOMER")
		{
			Trader comp = DataHelper.GetTrader (pathToDatabase, custcode);
			test += "------------------------------------------\n";
			test += title+"\n";
			string tel = string.IsNullOrEmpty (comp.Tel) ? " " : comp.Tel.Trim ();
			string fax = string.IsNullOrEmpty (comp.Fax) ? " " : comp.Fax.Trim ();
			string addr1 =string.IsNullOrEmpty (comp.Addr1) ? "" : comp.Addr1.Trim ();
			string addr2 =string.IsNullOrEmpty (comp.Addr2) ? "" : comp.Addr2.Trim ();
			string addr3 =string.IsNullOrEmpty (comp.Addr3) ? "" : comp.Addr3.Trim ();
			string addr4 =string.IsNullOrEmpty (comp.Addr4) ? "" : comp.Addr4.Trim ();
			string gst =string.IsNullOrEmpty (comp.gst) ? "" : comp.gst.Trim ();

			PrintLongText (ref test, comp.CustName.Trim ());

			if (addr1!="")
				test += comp.Addr1.Trim () + "\n";	
			if (addr2!="")
				test += comp.Addr2.Trim () + "\n";	
			if (addr3!="")
				test += comp.Addr3.Trim () + "\n";	
			if (addr4!="")
				test += comp.Addr4.Trim () + "\n";	

			test += "TEL:" + tel+"  FAX:"+fax+"\n";
			test += "GST NO:" + gst+"\n";
			test += "------------------------------------------\n";
		}
	}
}

