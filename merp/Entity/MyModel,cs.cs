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
}

