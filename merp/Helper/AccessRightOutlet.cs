
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
	public class AccessRightDesc
	{
		//general
		public string PT = "Show Print Time";
		public string CH = "Clear Posted Transactions";
		public string TA = "Customer By Agent";
		//invouice
		public string IA = "Invoice Allow Add";
		public string IE = "Invoice Allow Edit";
		public string ID = "Invoice Allow Delete";
		public string IP = "Invoice Allow Print";
		public string IUP = "Invoice Allow Edit Unit Price";
		public string IEP = "Invoice No Edit After Print";
		public string ITX = "Invoice Edit TrxType";

		//CN
		public string CA = "C/NOTE Allow Add";
		public string CE = "C/NOTE Allow Edit";
		public string CD = "C/NOTE Allow Delete";
		public string CP = "C/NOTE Allow Print";
		public string CEP = "C/NOTE No Edit After Print";

		//DO
		public string DA = "C/NOTE Allow Add";
		public string DE = "C/NOTE Allow Edit";
		public string DD = "C/NOTE Allow Delete";
		public string DP = "C/NOTE Allow Print";
		public string DEP = "C/NOTE No Edit After Print";

		//SO
		public string SA = "Sales Order Allow Add";
		public string SE = "Sales Order Allow Edit";
		public string SD = "Sales Order Allow Delete";
		public string SP = "Sales Order Allow Print";
		public string SEP = "Sales Order No Edit After Print";
		public string SUP = "Sales Order Allow Edit Unit Price";
	}

	public class AccessRightField
	{
		//genaral
		public string PT = "IsShowPrintTime";
		public string CH = "IsAllowClrPostedTrx";
		public string TA = "IsCustByAgent";
		//invouice
		public string IA = "InvAllowAdd";
		public string IE = "InvAllowEdit";
		public string ID = "InvAllowDelete";
		public string IP = "InvAllowPrint";
		public string IUP = "InvEditUPrice";
		public string IEP = "InvNotEditAftPrint";
		public string ITX = "InvEditTrxType";

		//invouice
		public string CA = "CNAllowAdd";
		public string CE = "CNAllowEdit";
		public string CD = "CNAllowDelete";
		public string CP = "CNAllowPrint";
		public string CEP = "CNNotEditAftPrint";

		public string DA = "DOAllowAdd";
		public string DE = "DOAllowEdit";
		public string DD = "DOAllowDelete";
		public string DP = "DOAllowPrint";
		public string DEP = "DONotEditAftPrint";

		public string SA = "SOAllowAdd";
		public string SE = "SOAllowEdit";
		public string SD = "SOAllowDelete";
		public string SP = "SOAllowPrint";
		public string SEP = "SONotEditAftPrint";
		public string SUP = "SOEditUPrice";
	}

	public class AccessRights
	{
		//genaral
		public bool IsShowPrintTime { get; set; }
		public bool IsAllowClrPostedTrx { get; set; }
		public bool IsCustByAgent { get; set; }
		//invouice
		public bool InvAllowAdd { get; set; }
		public bool InvAllowEdit { get; set; }
		public bool InvAllowDelete { get; set; }
		public bool InvAllowPrint { get; set; }
		public bool InvEditUPrice { get; set; }
		public bool InvNotEditAftPrint { get; set; }
		public bool InvEditTrxType { get; set; }
		//CN
		public bool CNAllowAdd { get; set; }
		public bool CNAllowEdit { get; set; }
		public bool CNAllowDelete { get; set; }
		public bool CNAllowPrint { get; set; }
		public bool CNNotEditAftPrint { get; set; }
		//DO
		public bool DOAllowAdd { get; set; }
		public bool DOAllowEdit { get; set; }
		public bool DOAllowDelete { get; set; }
		public bool DOAllowPrint { get; set; }
		public bool DONotEditAftPrint { get; set; }
		//SO
		public bool SOAllowAdd { get; set; }
		public bool SOAllowEdit { get; set; }
		public bool SOAllowDelete { get; set; }
		public bool SOAllowPrint { get; set; }
		public bool SONotEditAftPrint { get; set; }
		public bool SOEditUPrice { get; set; }


	}


}

