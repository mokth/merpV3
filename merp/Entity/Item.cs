using System;
using SQLite;

namespace wincom.mobile.erp
{
	public class Item
	{
		[PrimaryKey, AutoIncrement]
		/// <summary>
		/// Gets or sets the I.
		/// </summary>
		/// <value>The I.</value>
		public int ID { get; set; }
		/// <summary>
		/// Gets or sets the I code.
		/// </summary>
		/// <value>The I code.</value>
		public string ICode{ get; set; }
		/// <summary>
		/// Gets or sets the I desc.
		/// </summary>
		/// <value>The I desc.</value>
		public string IDesc { get; set; }
		/// <summary>
		/// Gets or sets the price.
		/// </summary>
		/// <value>The price.</value>
		public double Price { get; set; }
		/// <summary>
		/// Gets or sets the tax.
		/// </summary>
		/// <value>The tax.</value>
		public double tax { get; set; }
		/// <summary>
		/// Gets or sets the taxgrp.
		/// </summary>
		/// <value>The taxgrp.</value>
		public string taxgrp { get; set; }
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="wincom.mobile.erp.Item"/> is isincludesive.
		/// </summary>
		/// <value><c>true</c> if isincludesive; otherwise, <c>false</c>.</value>
		public bool isincludesive { get; set; }

		/// <summary>
		/// Gets or sets the VIP price.
		/// </summary>
		/// <value>The VIP price.</value>
		public double VIPPrice { get; set; }
		/// <summary>
		/// Gets or sets the retail price.
		/// </summary>
		/// <value>The retail price.</value>
		public double RetailPrice { get; set; }
		/// <summary>
		/// Gets or sets the whole sale price.
		/// </summary>
		/// <value>The whole sale price.</value>
		public double WholeSalePrice { get; set; }

		public string Barcode { get; set; }

		public string StdUom { get; set; }
		public string Class { get; set; }
		public string ImageFilename { get; set; }
	}

	public class ItemStock
	{
		[PrimaryKey, AutoIncrement]
		public int ID { get; set; }
		public string ICode{ get; set; }
		public DateTime DateTrx{ get; set; }
		public string IDesc { get; set; }
		public string StdUom { get; set; }
		public string Wh { get; set; }
		public double QtyGR { get; set; }
		public double QtyAct { get; set; }
		public double QtySales { get; set; }
		public double QtyBrf { get; set; }
		public double QtyRtr { get; set; }
		public double QtyCrf { get; set; }
		public double QtyBal { get; set; }
	}

	public class ItemStkSummary
	{
		public string ICODE { get; set; }
		public string DESC { get; set; }
		public double BF {get;set;}    //Stock from Yesterday
		public double NSTK { get; set; }  //New Stock Received by Actual Qty
		public double SSTK { get; set; }  //Latest Online receive qty
		public double DSTK { get; set; }  //BF - NSTK
		public double DSL { get; set; }   //SSL -ASL
		public double SAMT { get; set; }  //SSL * Unit price
		public double SSL { get; set; }  //Sales qty base on invoice
		public double SRT { get; set; }  //Return qty base on invoice (EXCH) and CN
		public double SBL { get; set; }  //SSTK - SSL - SRT
		public double AAMT { get; set; }  //ASL * UnitPrice
		public double ASL { get; set; }  //BF +NSTK -ART - ABL
		public double ART { get; set; }  //Return Qty base on actual count
		public double ABL { get; set; } //Balance qty base on actual count
		public double DAMT { get; set; } //SAMT - AAMT

	}

	public class ItemPrices
	{
		[PrimaryKey, AutoIncrement]
		public int ID { get; set; }
		public string CustCode { get; set; }
		public string CustName { get; set; }
		public string ICode { get; set; }
		public string IDesc { get; set; }
		public string IClass { get; set; }
		public double Price { get; set; }
		public DateTime InvDate { get; set; }

	}

}
