using System;
using SQLite;

namespace wincom.mobile.erp
{
	public class AdPara
	{
		[PrimaryKey, AutoIncrement]
		public int ID { get; set; }
		public string PrinterName{ get; set;}
		public string Prefix{ get; set;}
		public string PaperSize{ get; set;}
		public string Warehouse{ get; set;}
		public string ReceiptTitle{ get; set;}
		public int RunNo{ get; set;}
		//Version 2 added
		public int CNRunNo{ get; set;}
		public int SORunNo{ get; set;}
		public int DORunNo{ get; set;}
		public string CNPrefix{ get; set;}
		public string DOPrefix{ get; set;}
		public string SOPrefix{ get; set;}
		//Print Server printing added
		public string PrinterIP{ get; set;}
		public string PrinterType{ get; set;}
		public string FooterNote{ get; set;}
		public string FooterCNNote{ get; set;}
		public string FooterDONote{ get; set;}
		public string FooterSONote{ get; set;}
	}

	public class AdNumDate
	{
		[PrimaryKey, AutoIncrement]
		public int ID { get; set; }
		public int Year{ get; set;}
		public int Month{ get; set;}
		public int RunNo{ get; set;}
		//Version 2 added
		public string TrxType{ get; set;}

	}

	public class GeoLocation
	{
		[PrimaryKey, AutoIncrement]
		public int ID { get; set; }
		public string UserID{ get; set;}
		public string CompCode{ get; set;}
		public string BranchCode{ get; set;}
		public DateTime Date{ get; set;}
		public double lat{ get; set;}
		public double lng { get; set;}
		public double Altitude { get; set;}
		public double Heading { get; set;}
		public bool isUploaded  { get; set; }

	}

	public class GeoLocationModel
	{
		public string Desc{ get; set;}
		public string Date{ get; set;}
		public double lat{ get; set;}
		public double lng { get; set;}
		public double Altitude { get; set;}
		public double Heading { get; set;}


	}
}

