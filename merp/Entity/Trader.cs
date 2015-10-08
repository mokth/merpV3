
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite;


namespace wincom.mobile.erp
{
	public class Trader
	{
		[PrimaryKey]
		/// <summary>
		/// Gets or sets the cust code.
		/// </summary>
		/// <value>The cust code.</value>
		public string CustCode{ get; set;}
		/// <summary>
		/// Gets or sets the name of the cust.
		/// </summary>
		/// <value>The name of the cust.</value>
		public string CustName{ get; set;}
		/// <summary>
		/// Gets or sets the addr1.
		/// </summary>
		/// <value>The addr1.</value>
		public string Addr1{ get; set;}
		/// <summary>
		/// Gets or sets the addr2.
		/// </summary>
		/// <value>The addr2.</value>
		public string Addr2{ get; set;}
		/// <summary>
		/// Gets or sets the addr3.
		/// </summary>
		/// <value>The addr3.</value>
		public string Addr3{ get; set;}
		/// <summary>
		/// Gets or sets the addr4.
		/// </summary>
		/// <value>The addr4.</value>
		public string Addr4{ get; set;}
		/// <summary>
		/// Gets or sets the tel.
		/// </summary>
		/// <value>The tel.</value>
		public string Tel{ get; set;}
		/// <summary>
		/// Gets or sets the fax.
		/// </summary>
		/// <value>The fax.</value>
		public string Fax{ get; set;}
		/// <summary>
		/// Gets or sets the gst.
		/// </summary>
		/// <value>The gst.</value>
		public string gst{ get; set;}

		/// <summary>
		/// Gets or sets the pay code.
		/// </summary>
		/// <value>The pay code.</value>
		public string PayCode{ get; set;}
		/// <summary>
		/// Gets or sets the type of the cust.
		/// </summary>
		/// <value>The type of the cust.</value>
		public string CustType{ get; set;}

		public string AgentCode{ get; set;}
	}
}

