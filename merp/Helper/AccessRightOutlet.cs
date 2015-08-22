
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
		public string SO = "Sales Order";
		public string DO = "Delivery Order";
		public string CN = "Credit Note";
		public string INV = "Invoice";
		//invouice
		public string INA = "Invoice Allow Add";
		public string INE = "Invoice Allow Edit";
		public string IND = "Invoice Allow Delete";
		public string INP = "Invoice Allow Print";
		public string INUP = "Invoice Allow Edit Unit Price";
		public string INEP = "Invoice No Edit After Print";
		public string INTX = "Invoice Edit TrxType";
		public string INUU = "Upload Invoice";

		//CN
		public string CNA = "C/NOTE Allow Add";
		public string CNE = "C/NOTE Allow Edit";
		public string CND = "C/NOTE Allow Delete";
		public string CNP = "C/NOTE Allow Print";
		public string CNEP = "C/NOTE No Edit After Print";
		public string CNUP = "Upload C/NOTE";

		//DO
		public string DOA = "Delivery Order Allow Add";
		public string DOE = "Delivery Order Allow Edit";
		public string DOD = "Delivery Order Allow Delete";
		public string DOP = "Delivery Order Allow Print";
		public string DOEP = "Delivery Order No Edit After Print";
		public string DOUP = "Upload Delivery Order";

		//SO
		public string SOA = "Sales Order Allow Add";
		public string SOE = "Sales Order Allow Edit";
		public string SOD = "Sales Order Allow Delete";
		public string SOP = "Sales Order Allow Print";
		public string SOEP = "Sales Order No Edit After Print";
		public string SOUP = "Sales Order Allow Edit Unit Price";
		public string SOUU = "Upload Sales Order";
	}

	public class AccessRightField
	{
		//genaral
		public string GNPT = "IsShowPrintTime";
		public string GNCH = "IsAllowClrPostedTrx";
		public string GNTA = "IsCustByAgent";
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
		public string INUU = "InvUpload";

		//invouice
		public string CNA = "CNAllowAdd";
		public string CNE = "CNAllowEdit";
		public string CND = "CNAllowDelete";
		public string CNP = "CNAllowPrint";
		public string CNEP = "CNNotEditAftPrint";
		public string CNUP = "CNUpload";

		public string DOA = "DOAllowAdd";
		public string DOE = "DOAllowEdit";
		public string DOD = "DOAllowDelete";
		public string DOP = "DOAllowPrint";
		public string DOEP = "DONotEditAftPrint";
		public string DOUP = "DOUpload";

		public string SOA = "SOAllowAdd";
		public string SOE = "SOAllowEdit";
		public string SOD = "SOAllowDelete";
		public string SOP = "SOAllowPrint";
		public string SOEP = "SONotEditAftPrint";
		public string SOUP = "SOEditUPrice";
		public string SOUU = "SOUpload";
	}

	public class AccessRights
	{
		//genaral
		public bool IsShowPrintTime { get; set; }
		public bool IsAllowClrPostedTrx { get; set; }
		public bool IsCustByAgent { get; set; }
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
		public bool InvUpload { get; set; }
		//CN
		public bool CNAllowAdd { get; set; }
		public bool CNAllowEdit { get; set; }
		public bool CNAllowDelete { get; set; }
		public bool CNAllowPrint { get; set; }
		public bool CNNotEditAftPrint { get; set; }
		public bool CNUpload { get; set; }
		//DO
		public bool DOAllowAdd { get; set; }
		public bool DOAllowEdit { get; set; }
		public bool DOAllowDelete { get; set; }
		public bool DOAllowPrint { get; set; }
		public bool DONotEditAftPrint { get; set; }
		public bool DOUpload { get; set; }
		//SO
		public bool SOAllowAdd { get; set; }
		public bool SOAllowEdit { get; set; }
		public bool SOAllowDelete { get; set; }
		public bool SOAllowPrint { get; set; }
		public bool SONotEditAftPrint { get; set; }
		public bool SOEditUPrice { get; set; }
		public bool SOUpload { get; set; }

	}
}

