
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace wincom.mobile.erp
{	
	//full access
	//GNPT,GNCH,GNTA,INA,INE,IND,INP,INUP,INEP,INTX,INUU,CNA,CNE,CND,CNP,CNEP,CNUP,DOA,DOE,DOD,DOP,DOEP,DOUP,SOA,SOE,SOD,SOP,SOEP,SOUP,SOUU,
	public class AccessRightDesc
	{
		//general
		public string GNPT = "Show Print Time";
		public string GNCH = "Clear Posted Transactions";
		public string GNTA = "Customer By Agent";
		public string GNLO = "Print Company Logo";
		public string GNLC = "Mobile App Shutdown Control";
		public string GNUC = "Force Upload Bill (Ver 5)";
		public string GNVC = "Mobile App Version Control (Ver 5)";
		public string GNVG = "GPS Tracking (Beta Version)";

		public string SO = "Sales Order";
		public string DO = "Delivery Order";
		public string CN = "Credit Note";
		public string INV = "Invoice";
		public string CS = "CASH BILL";
		//invouice
		public string INA = "Allow Add New Trx";
		public string INE = "Allow Edit Items";
		public string IND = "Allow Delete Items";
		public string INP = "Allow Print Invoice";
		public string INUP = "Allow Edit Unit Price";
		public string INEP = "Not Edit After Print";
		public string INTX = "Allow Edit Trx Type";
		public string INTD = "Allow Edit Trx Date";
		public string INTM = "Allow Multi Issue Type";
		public string INUU = "Allow Upload Invoice";

		//invouice
		public string CSA = "Allow Add New Trx";
		public string CSE = "Allow Edit Items";
		public string CSD = "Allow Delete Items";
		public string CSP = "Allow Print Invoice";
		public string CSUP = "Allow Edit Unit Price";
		public string CSEP = "Not Edit After Print";
		public string CSTX = "Allow Edit Trx Type";
		public string CSTD = "Allow Edit Trx Date";
		public string CSTM = "Allow Multi Issue Type";
		public string CSUU = "Allow Upload Invoice";

		//CN
		public string CNA = "Allow Add New Trx";
		public string CNE = "Allow Edit Items";
		public string CND = "Allow Delete Items";
		public string CNP = "Allow Print C/Note";
		public string CNEP = "Not Edit After Print";
		public string CNUP = "Allow Edit Unit Price";
		public string CNTD = "Allow Edit Trx Date";
		public string CNUU = "Allow Upload C/NOTE";

		//DO
		public string DOA = "Allow Add New Trx";
		public string DOE = "Allow Edit Items";
		public string DOD = "Allow Delete Items";
		public string DOP = "Allow Print Delivery Order";
		public string DOEP = "Not Edit After Print";
		public string DOTD = "Allow Edit Trx Date";
		public string DOUP = "Allow Upload Delivery Order";

		//SO
		public string SOA = "Allow Add New Trx";
		public string SOE = "Allow Edit Items";
		public string SOD = "Allow Delete Items";
		public string SOP = "Allow Print Sales Order";
		public string SOEP = "Not Edit After Print";
		public string SOUP = "Allow Edit Unit Price";
		public string SOTD = "Allow Edit Trx Date";
		public string SOUU = "Allow Upload Sales Order";
	}

	public class AccessRightField
	{
		//genaral
		public string GNPT = "IsShowPrintTime";
		public string GNCH = "IsAllowClrPostedTrx";
		public string GNTA = "IsCustByAgent";
		public string GNLO = "IsPrintCompLogo";
		public string GNLC = "IsLoginControl";
		public string GNUC = "IsUploadControl";
		public string GNVC = "IsVersionControl";
		public string GNVG = "IsGPSTracking";

		public string DO = "IsDOModule";
		public string SO = "IsSOModule";
		public string CN = "IsCNModule";
		public string INV = "IsInvModule";
		//invouice
		public string INA = "InvAllowAdd";
		public string INE = "InvAllowEdit";
		public string IND = "InvAllowDelete";
		public string INP = "InvAllowPrint";
		public string INUP = "InvEditUPrice";
		public string INEP = "InvNotEditAftPrint";
		public string INTX = "InvEditTrxType";
		public string INTD = "InvEditTrxDate";
		public string INTM = "InvMultiType";
		public string INUU = "InvUpload";

		//invouice
		public string CSA = "CSAllowAdd";
		public string CSE = "CSAllowEdit";
		public string CSD = "CSAllowDelete";
		public string CSP = "CSAllowPrint";
		public string CSUP = "CSEditUPrice";
		public string CSEP = "CSNotEditAftPrint";
		public string CSTX = "CSEditTrxType";
		public string CSTD = "CSEditTrxDate";
		public string CSTM = "CSMultiType";
		public string CSUU = "CSUpload";

		//invouice
		public string CNA = "CNAllowAdd";
		public string CNE = "CNAllowEdit";
		public string CND = "CNAllowDelete";
		public string CNP = "CNAllowPrint";
		public string CNEP = "CNNotEditAftPrint";
		public string CNUP = "CNEditUPrice";
		public string CNTD = "CNEditTrxDate";
		public string CNUU = "CNUpload";

		public string DOA = "DOAllowAdd";
		public string DOE = "DOAllowEdit";
		public string DOD = "DOAllowDelete";
		public string DOP = "DOAllowPrint";
		public string DOEP = "DONotEditAftPrint";
		public string DOUP = "DOUpload";
		public string DOTD = "DOEditTrxDate";

		public string SOA = "SOAllowAdd";
		public string SOE = "SOAllowEdit";
		public string SOD = "SOAllowDelete";
		public string SOP = "SOAllowPrint";
		public string SOEP = "SONotEditAftPrint";
		public string SOUP = "SOEditUPrice";
		public string SOUU = "SOUpload";
		public string SOTD = "SOEditTrxDate";
	}

	public class AccessRights
	{
		//genaral
		public bool IsShowPrintTime { get; set; }
		public bool IsAllowClrPostedTrx { get; set; }
		public bool IsCustByAgent { get; set; }
		public bool IsPrintCompLogo { get; set; }
		public bool IsLoginControl { get; set; }
		public bool IsUploadControl { get; set; }
		public bool IsVersionControl { get; set; }
		public bool IsGPSTracking { get; set; }

		public bool IsSOModule { get; set; }
		public bool IsDOModule { get; set; }
		public bool IsCNModule { get; set; }
		public bool IsInvModule { get; set; }
		//invouice
		public bool InvAllowAdd { get; set; }
		public bool InvAllowEdit { get; set; }
		public bool InvAllowDelete { get; set; }
		public bool InvAllowPrint { get; set; }
		public bool InvEditUPrice { get; set; }
		public bool InvNotEditAftPrint { get; set; }
		public bool InvEditTrxType { get; set; }
		public bool InvEditTrxDate { get; set; }
		public bool InvMultiType { get; set; }
		public bool InvUpload { get; set; }

		public bool CSAllowAdd { get; set; }
		public bool CSAllowEdit { get; set; }
		public bool CSAllowDelete { get; set; }
		public bool CSAllowPrint { get; set; }
		public bool CSEditUPrice { get; set; }
		public bool CSNotEditAftPrint { get; set; }
		public bool CSEditTrxType { get; set; }
		public bool CSEditTrxDate { get; set; }
		public bool CSMultiType { get; set; }
		public bool CSUpload { get; set; }

		//CN
		public bool CNAllowAdd { get; set; }
		public bool CNAllowEdit { get; set; }
		public bool CNAllowDelete { get; set; }
		public bool CNAllowPrint { get; set; }
		public bool CNNotEditAftPrint { get; set; }
		public bool CNUpload { get; set; }
		public bool CNEditUPrice { get; set; }
		public bool CNEditTrxDate { get; set; }
		//DO
		public bool DOAllowAdd { get; set; }
		public bool DOAllowEdit { get; set; }
		public bool DOAllowDelete { get; set; }
		public bool DOAllowPrint { get; set; }
		public bool DONotEditAftPrint { get; set; }
		public bool DOUpload { get; set; }
		public bool DOEditTrxDate { get; set; }
		//SO
		public bool SOAllowAdd { get; set; }
		public bool SOAllowEdit { get; set; }
		public bool SOAllowDelete { get; set; }
		public bool SOAllowPrint { get; set; }
		public bool SONotEditAftPrint { get; set; }
		public bool SOEditUPrice { get; set; }
		public bool SOUpload { get; set; }
		public bool SOEditTrxDate { get; set; }

	}
}

