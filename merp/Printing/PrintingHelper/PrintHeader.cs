using System;

namespace wincom.mobile.erp
{
	public class PrintReportHeader:PrintHelperBase
	{
		static void printCaption (ref string test)
		{
			test += "------------------------------------------\n";
			test += "DESCRIPTION                               \n";
			test += "NO       QTY  U/PRICE    TAX AMT    AMOUNT\n";
			test += "------------------------------------------\n";
		}

		static void printCaption_NTax (ref string test)
		{
			test += "------------------------------------------\n";
			test += "DESCRIPTION                               \n";
			test += "NO        QTY      U/PRICE         AMOUNT \n";
			test += "------------------------------------------\n";
				   //123456789012345678901234567890123456789012
			      // 123 123456789 12345678912 1234567890124 
		}

		public void PrintHeader_NTax (ref string test,Invoice inv)
		{
			string userid = USERID;
		//	string[] titles = apara.ReceiptTitle.Split (new char[]{ ',', '|'});
			string title1 = "INVOICE";
			string title2 = "";

			string date = DateTime.Now.ToString ("dd-MM-yyyy");
			string datetime = DateTime.Now.ToString ("dd-MM-yyyy hh:mm tt");
			if (compinfo.ShowTime) {
				test += datetime+title1.PadLeft(41-datetime.Length,' ')+"\n";
			} else {
				test += date+title1.PadLeft(41-date.Length,' ')+"\n";
			}
			//test += DateTime.Now.ToString ("dd-MM-yyyy")+"TAX INVOICE".PadLeft(31,' ')+"\n";
			string recno = "RECPT NO : " + inv.invno.Trim();
			//test += "RECPT NO : " + inv.invno+"\n";
			test += recno+title2.PadLeft(41-recno.Length,' ')+"\n";
			string issueline = "ISSUED BY: " + userid.ToUpper ();
			int templen = 41 - issueline.Length;
			string term = "("+((inv.trxtype.IndexOf("CASH")>-1)?"COD":"TERM")+")"; 
			issueline = issueline + term.PadLeft (templen, ' ')+"\n";
			test += issueline;// "ISSUED BY: " + userid.ToUpper()+"\n";
			printCaption_NTax (ref test);
		}

		public void PrintHeader (ref string test,Invoice inv)
		{
			PrintInvHeader (ref test,inv);
			printCaption (ref test);
		}

		void PrintInvHeader (ref string test,Invoice inv)
		{
			string userid = USERID;
			string[] titles = apara.ReceiptTitle.Split (new char[]{ ',', '|'});
			string title1 = "";
			string title2 = "";

			if (titles.Length ==1)
				title1 = titles [0].ToUpper ();
			if (titles.Length > 1) {
				title1 = titles [0].ToUpper ();
				title2 = titles [1].ToUpper ();
			}
			if (titles.Length == 0 || title1=="")
				title1 = "TAX INVOICE";

			string date = DateTime.Now.ToString ("dd-MM-yyyy");
			string datetime = DateTime.Now.ToString ("dd-MM-yyyy hh:mm tt");
			if (compinfo.ShowTime) {
				test += datetime+title1.PadLeft(41-datetime.Length,' ')+"\n";
			} else {
				test += date+title1.PadLeft(41-date.Length,' ')+"\n";
			}
			//test += DateTime.Now.ToString ("dd-MM-yyyy")+"TAX INVOICE".PadLeft(31,' ')+"\n";
			string recno = "RECPT NO : " + inv.invno.Trim();
			//test += "RECPT NO : " + inv.invno+"\n";
			test += recno+title2.PadLeft(41-recno.Length,' ')+"\n";
			string issueline = "ISSUED BY: " + userid.ToUpper ();
			int templen = 41 - issueline.Length;
			string term = "("+((inv.trxtype.IndexOf("CASH")>-1)?"COD":"TERM")+")"; 
			issueline = issueline + term.PadLeft (templen, ' ')+"\n";
			test += issueline;// "ISSUED BY: " + userid.ToUpper()+"\n";

		}

		public void PrintCNHeader (ref string test,CNNote cn)
		{
			PrintHeaderCN (ref test, cn);
			printCaption (ref test);
		}

		public void PrintCNHeader_NTax (ref string test,CNNote cn)
		{
			PrintHeaderCN (ref test, cn);
			printCaption_NTax (ref test);
		}

		public void PrintHeaderCN (ref string test,CNNote inv)
		{
			string userid = USERID;
			string[] titles = apara.ReceiptTitle.Split (new char[]{ ',', '|'});
			string title1 = "";
			string title2 = "";
			title1 = "CREDIT NOTE";

			string date = DateTime.Now.ToString ("dd-MM-yyyy");
			string datetime = DateTime.Now.ToString ("dd-MM-yyyy hh:mm tt");
			if (compinfo.ShowTime) {
				test += datetime+title1.PadLeft(41-datetime.Length,' ')+"\n";
			} else {
				test += date+title1.PadLeft(41-date.Length,' ')+"\n";
			}
			//test += DateTime.Now.ToString ("dd-MM-yyyy")+"TAX INVOICE".PadLeft(31,' ')+"\n";
			if (!string.IsNullOrEmpty (inv.invno)) {
				test += "INVOICE NO     : " + inv.invno.Trim ()+"\n";
			}

			string recno = "CREDIT NOTE NO : " + inv.cnno.Trim();

			//test += "RECPT NO : " + inv.invno+"\n";
			test += recno+title2.PadLeft(41-recno.Length,' ')+"\n";
			string issueline = "ISSUED BY: " + userid.ToUpper ();
			int templen = 41 - issueline.Length;
			string term = "("+((inv.trxtype.IndexOf("CASH")>-1)?"COD":"TERM")+")"; 
			issueline = issueline + term.PadLeft (templen, ' ')+"\n";
			test += issueline;// "ISSUED BY: " + userid.ToUpper()+"\n";
			if (!string.IsNullOrEmpty (inv.remark)) {
				string reason = "REASON: " + inv.remark;
				PrintLongText (ref test, reason);
			}

		}

		//众人皆醉我独醒，与code共舞
		public void PrintSOHeader (ref string test,SaleOrder so)
		{
			PrintHeaderSO (ref test, so);
			printCaption (ref test);
		}

		public void PrintSOHeader_NTax (ref string test,SaleOrder so)
		{
			PrintHeaderSO (ref test, so);
			printCaption_NTax (ref test);
		}

		public void PrintHeaderSO (ref string test,SaleOrder so)
		{
			string userid = USERID;
			string title1 = "SALES ORDER";
			string date = DateTime.Now.ToString ("dd-MM-yyyy");
			string datetime = DateTime.Now.ToString ("dd-MM-yyyy hh:mm tt");
			if (compinfo.ShowTime) {
				test += datetime+title1.PadLeft(41-datetime.Length,' ')+"\n";
			} else {
				test += date+title1.PadLeft(41-date.Length,' ')+"\n";
			}
			string recno = "SALES ORDER NO : " + so.sono.Trim();
			test += recno+"\n";
			test += "CUST PO NO : " + so.custpono+"\n";
			string issueline = "ISSUED BY: " + userid.ToUpper ();
			issueline = issueline +"\n";
			test += issueline;// "ISSUED BY: " + userid.ToUpper()+"\n";
			if (so.remark.Length > 1) {
				test += "REMARK:\n";
				test += so.remark+"\n";
			}

		}


		public void PrintDOHeader (ref string test,DelOrder doOrder)
		{
			string userid = USERID;
			string title1 = "DELIVERY ORDER";
			string date = DateTime.Now.ToString ("dd-MM-yyyy");
			string datetime = DateTime.Now.ToString ("dd-MM-yyyy hh:mm tt");
			if (compinfo.ShowTime) {
				test += datetime+title1.PadLeft(41-datetime.Length,' ')+"\n";
			} else {
				test += date+title1.PadLeft(41-date.Length,' ')+"\n";
			}
			string recno = "DELIVERY ORDER NO : " + doOrder.dono.Trim();
			test += recno+"\n";
			string issueline = "ISSUED BY: " + userid.ToUpper ();
			string term = "TERM     : " + doOrder.term.ToUpper();
			issueline = issueline +"\n";
			test += issueline;// "ISSUED BY: " + userid.ToUpper()+"\n";
			test += term +"\n";
			if (doOrder.remark.Length > 1) {
				test += "REMARK:\n";
				test += doOrder.remark+"\n";
			}
			test += "------------------------------------------\n";
			test += "NO  DESCRIPTION                       QTY \n";
			test += "------------------------------------------\n";
			//  1234
			//	  	 12345678901234567890123456789012312345
			//
		}

		public void PrintPaymentHeader (ref string test,SaleOrder so)
		{
			string userid = USERID;
			string title1 = "PAYMENT VOUCHER";
			string date = DateTime.Now.ToString ("dd-MM-yyyy");
			string datetime = DateTime.Now.ToString ("dd-MM-yyyy hh:mm tt");
			if (compinfo.ShowTime) {
				test += datetime+title1.PadLeft(41-datetime.Length,' ')+"\n";
			} else {
				test += date+title1.PadLeft(41-date.Length,' ')+"\n";
			}
			string recno = "REFERENCE NO : " + so.sono.Trim();
			test += recno+"\n";
			test += "CUST PO NO : " + so.custpono+"\n";
			string issueline = "ISSUED BY: " + userid.ToUpper ();
			issueline = issueline +"\n";
			test += issueline;// "ISSUED BY: " + userid.ToUpper()+"\n";
			if (so.remark.Length > 1) {
				test += "REMARK:\n";
				test += so.remark+"\n";
			}
			printCaption (ref test);

		}
	}
}

