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
	}


}
