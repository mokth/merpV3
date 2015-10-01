using System;
using SQLite;

namespace wincom.mobile.erp
{
	public class Invoice
	{
		[PrimaryKey] 
		public string invno  { get; set; }
		public string trxtype  { get; set; }
		public DateTime invdate  { get; set; }
		public DateTime created  { get; set; }
		public double amount { get; set; }
		public double taxamt { get; set; }
		public string custcode { get; set; }
		public string description { get; set; }
		public DateTime uploaded  { get; set; }
		public bool isUploaded  { get; set; }
		public bool isPrinted  { get; set; }
		public string remark { get; set; }
	}

	public class InvoiceDtls
	{
		[PrimaryKey, AutoIncrement]
		public int ID { get; set; }
		public string invno  { get; set; }
		public double amount { get; set; }
		public double netamount { get; set; }
		public string icode { get; set; }
		public string description { get; set; }
		public double qty { get; set; }
		public double price { get; set; }
		public string taxgrp { get; set; }
		public double tax { get; set; }
		public bool isincludesive { get; set; }
	}

	public class OutLetBillTemp
	{
		public string CompanyCode { get; set; }
		public string BranchCode { get; set; }
		public string UserID { get; set; }
		public string InvNo { get; set; }
		public System.DateTime InvDate { get; set; }
		public System.DateTime Created { get; set; }
		public string CustCode { get; set; }
		public string ICode { get; set; }
		public double UPrice { get; set; }
		public double Qty { get; set; }
		public double Amount { get; set; }
		public double NetAmount { get; set; }
		public double TaxAmt { get; set; }
		public string TaxGrp { get; set; }
		public bool IsInclusive { get; set; }
		public string TrxType { get; set; }
		public string CNInvNo { get; set; }
		public string Remark { get; set; }
		public string OtheDesc { get; set; }
		public string Module { get; set; }
		public bool IsUploaded { get; set; }
	}
}

