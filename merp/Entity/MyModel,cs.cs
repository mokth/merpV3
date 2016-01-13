using System;
using System.Collections.Generic;

namespace wincom.mobile.erp
{
	public class TaxInfo
	{
		public string Tax { get; set; }
		public double TaxPer { get; set; }
		public double Amount { get; set; }
		public double TaxAmt { get; set; }
	}

	public class MyModel
	{
		public CompanyInfo Company { get; set; }
		public Invoice Invoicehdr { get; set; }
		public List<InvoiceDtls> InvDtls { get; set; }
		public Trader Customer { get; set; }
		public String UserID { get; set; }
		public DateTime PrintDate { get; set; }
		public string CustomerAddress { get; set; }
		public List<TaxInfo> TaxSumm { get; set; }
	}

	public class ModelSumm
	{
		public CompanyInfo Company { get; set; }
		public String UserID { get; set; }
		public DateTime PrintDate { get; set; }
		public List<ModelSummDate> DailyTrx { get; set; }
		public double TotalInv { get; set; }
		public double TotalCash { get; set; }
		public double TotalCNInv { get; set; }
		public double TotalCNCash { get; set; }
		public double TotalCNCollect { get; set; }
		public List<ModelGrpPrice> GrpPrices { get; set; }
	}

	public class ModelGrpPrice
	{
		public String ICode { get; set; }
		public String IDesc { get; set; }
		public double Price { get; set; }
		public double Qty { get; set; }
	}

	public class ModelSummDate
	{
		public DateTime Date { get; set; }
		public List<Invoice> CashList { get; set; }
		public List<Invoice> InvList { get; set; }

	}
}

