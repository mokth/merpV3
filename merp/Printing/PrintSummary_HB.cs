using System;
using System.Data;
using System.Xml.Linq;
using System.Linq;
using System.Linq.Expressions;
using Android.App;
using SQLite;

namespace wincom.mobile.erp
{
	public class PrintSummary_HB:PrintDocumentBase,IPrintDocument
	{
		Invoice inv;
		InvoiceDtls[] list;
		int noOfCopy=1;
		DateTime printDate1;
		DateTime printDate2;
		Activity callingActivity;

		public void SetCallingActivity (Activity activity)
		{
			callingActivity = activity;
		}

		public bool StartPrint ()
		{
			if (extrapara == null) {
			
				errMsg = "Date range not define yet...";
				return false;
			}
			printDate1 = (DateTime)extrapara ["DateStart"];
			printDate2 = (DateTime)extrapara ["DateEnd"];
			return Print ();
		}

		public void SetDocument (object doc)
		{
			inv = (Invoice)doc;
		}

		public void SetDocumentDtls (object docdtls)
		{
			list = (InvoiceDtls[])docdtls;
		}
		public void SetNoOfCopy (int noofcopy)
		{
			noOfCopy = noofcopy;
		}


		public void SetExtraPara (System.Collections.Hashtable para)
		{
			extrapara = para;
		}

		public string GetErrMsg()
		{
			return errMsg;
		}

		private bool Print()
		{
			text = "";
			errMsg = "";
			bool isPrinted = false;
			text = GetInvoiceSumm (printDate1, printDate2);
			IPrintToDevice device = PrintDeviceManager.GetPrintingDevice<BlueToothDeviceHelper> ();
			device.SetCallingActivity (callingActivity);
			isPrinted = device.StartPrint (text, noOfCopy, ref errMsg);

			return isPrinted;
		}

		private string GetInvoiceSumm (DateTime printDate1, DateTime printDate2)
		{
			string text = "";
			int count = 0;
			double ttlAmtl=0;
			double ttlCN=0;
			double ttl = 0;
			double rate = 0;
			double roundVal = 0;
			double GranInv = 0;
			double GranCN = 0;
			double GranCSInv = 0;
			double GranCSCN = 0;
			string cnno = "";
			bool isSamedate = printDate1==printDate2;

			string pathToDatabase = ((GlobalvarsApp)Application.Context).DATABASE_PATH;
			SQLiteConnection db = new SQLiteConnection(pathToDatabase);
			PrintSummHeader (printDate1, printDate2, ref text);
			var list = db.Table<Invoice>().ToList();
			var cnlist = db.Table<CNNote>().ToList();
			var grps = from p in list
					where p.invdate >= printDate1 && p.invdate <= printDate2
				group p by p.invdate into g
				select new { key = g.Key, result = g.ToList() };

		
			foreach (var grp in grps)
			{
				count = 1;
				GranInv = 0;
				GranCN = 0;
				GranCSInv = 0;
				GranCSCN = 0;
				//"------------------------------------------\";
				//12. 123456789012345678901234567890 12345567  

				//CN without Inv
				var cnNoInvs = cnlist.Where (x => x.invdate == grp.key && x.invno == "");
				if (!isSamedate) {
					text = text + grp.key.ToString ("dd-MM-yyyy") + "\n";
					text = text + "------------------------------------------\n";
				}
				foreach (var itm in grp.result)
				{
					ttlAmtl=itm.amount+itm.taxamt;
					ttlCN=0;
					ttl = 0;
					cnno = "";
					//var cninv = cnlist.Where(x => x.invno == itm.invno && x.invdate == itm.invdate).FirstOrDefault();
					var cninv = cnlist.Where(x => x.invno == itm.invno ).FirstOrDefault();
					if (cninv!=null)
					{
						cnno = cninv.cnno;
						ttlCN=cninv.amount + cninv.taxamt;
					}

					if (itm.trxtype == "CASH")
					{
						ttlAmtl = Utility.AdjustToNear(ttlAmtl, ref roundVal);
						ttlCN = Utility.AdjustToNear(ttlCN, ref roundVal);
						GranCSInv = GranCSInv + ttlAmtl;
						GranCSCN = GranCSCN + ttlCN;
					}
					else
					{
						GranInv = GranInv + ttlAmtl;
						GranCN = GranCN+ttlCN;
					}
					if (itm.description.Length > 30)
					{
						//12. 123456789012345678901234567890 12345567  
						text = text + count.ToString().PadRight(2, ' ') + ". " + itm.description.Substring(0, 30) + " " + itm.created.ToString("hh:mmtt") + "\n";
						text = text + "".PadRight(4, ' ') + itm.description.Substring(30).Trim()+ "\n";
					}
					else
					{
						text = text + count.ToString().PadRight(2, ' ') + ". " + itm.description.PadRight(30,' ') + " " + itm.created.ToString("hh:mmtt") + "\n";
					}

					text = text + "INV AMT -"+itm.invno.PadRight(21,' ')+ttlAmtl.ToString("n2").PadLeft(12,' ')+"\n";
					text = text + "CN AMT  -" + cnno.PadRight(21, ' ') + ttlCN.ToString("n2").PadLeft(12, ' ') + "\n";

					ttl = ttlAmtl - ttlCN;
					if (ttlCN > 0)
						rate = Math.Round((ttlCN / ttlAmtl)*100, 2);

					text = text + "TOTAL COLLECT AMOUNT  -".PadRight(30,' ') + ttl.ToString("n2").PadLeft(12, ' ') + "\n";
					text = text + "RETURN RATE %         -".PadRight(30, ' ') + rate.ToString("n2").PadLeft(12, ' ') + "\n\n";

					count += 1;
				}


				foreach (var itm in cnNoInvs)
				{
					ttlAmtl=0;
					ttlCN=itm.amount+itm.taxamt;
					ttl = 0;
					cnno = "";

					if (itm.trxtype == "CASH")
					{
						ttlCN = Utility.AdjustToNear(ttlCN, ref roundVal);
						GranCSCN = GranCSCN + ttlCN;
					}
					else
					{
						GranCN = GranCN+ttlCN;
					}
					if (itm.description.Length > 30)
					{
						//12. 123456789012345678901234567890 12345567  
						text = text + count.ToString().PadRight(2, ' ') + ". " + itm.description.Substring(0, 30) + " " + itm.created.ToString("hh:mmtt") + "\n";
						text = text + "".PadRight(4, ' ') + itm.description.Substring(30).Trim()+ "\n";
					}
					else
					{
						text = text + count.ToString().PadRight(2, ' ') + ". " + itm.description.PadRight(30,' ') + " " + itm.created.ToString("hh:mmtt") + "\n";
					}

					text = text + "INV AMT -"+itm.invno.PadRight(21,' ')+ttlAmtl.ToString("n2").PadLeft(12,' ')+"\n";
					text = text + "CN AMT  -" + itm.cnno.PadRight(21, ' ') + ttlCN.ToString("n2").PadLeft(12, ' ') + "\n";

					ttl = ttlAmtl - ttlCN;
					rate = 0;

					text = text + "TOTAL COLLECT AMOUNT  -".PadRight(30,' ') + ttl.ToString("n2").PadLeft(12, ' ') + "\n";
					text = text + "RETURN RATE %         -".PadRight(30, ' ') + rate.ToString("n2").PadLeft(12, ' ') + "\n\n";

					count += 1;
				}

				text = text + "\n";
				text = text + "SALES SUMMARY\n\n";
				text = text + "TOTAL INVOICE ".PadRight(30, ' ') + GranInv.ToString("n2").PadLeft(12, ' ') + "\n";
				text = text + "TOTAL CN INVOICE".PadRight(30, ' ') + GranCN.ToString("n2").PadLeft(12, ' ') + "\n";
				text = text + "TOTAL INVOICE COLLECT ".PadRight(30, ' ') + (GranInv - GranCN).ToString("n2").PadLeft(12, ' ') + "\n\n";

				text = text + "TOTAL CASH ".PadRight(30, ' ') + GranCSInv.ToString("n2").PadLeft(12, ' ') + "\n";
				text = text + "TOTAL CN CASH".PadRight(30, ' ') + GranCSCN.ToString("n2").PadLeft(12, ' ') + "\n";
				text = text + "TOTAL CASH COLLECT ".PadRight(30, ' ') + (GranCSInv - GranCSCN).ToString("n2").PadLeft(12, ' ') + "\n\n";
			
			}
		
		    text += "------------------------------------------\n";
			text += "CASH COLLECTION   :\n\n\n";
			text += "(-)DIESEL EXP     :\n\n\n";
			text += "(-)OTHER EXP      :\n\n\n";
			text += "(=)NET COLLECTION :\n\n\n";
			text += "(-)PAYMENT        :\n\n\n";
			text += "(=)SHORT          :\n\n\n";
			text += "PREPARED BY:\n\n\n\n\n";
			text += "VERIFY BY  :\n\n\n\n\n";
			text += "------------------------------------------\n\n\n\n";

			db.Close ();

			return text;
		}

		internal virtual void PrintSummHeader (DateTime printdate1, DateTime printdate2, ref string text)
		{
			string USERID = ((GlobalvarsApp)Application.Context).USERID_CODE;
			var compinfo =DataHelper.GetCompany (((GlobalvarsApp)Application.Context).DATABASE_PATH);
			text += "------------------------------------------\n";
			//text += compinfo.CompanyName.ToUpper () + "\n";
			string[] names =compinfo.CompanyName.ToUpper ().Split (new char[] {
				'|'
			});
			if (names.Length > 1) {
				text += names [0] + "\n";
				if ((names [1].Trim ().Length + compinfo.RegNo.Trim ().Length + 2) > 42) {
					//test += names [1].Trim () + "\n";
					//test += "(" + comp.RegNo.Trim () + ")\n";
					PrintLongText (ref text, names [1].Trim () + "(" + compinfo.RegNo.Trim () + ")");
				}
				else {
					text += names [1].Trim () + "(" + compinfo.RegNo.Trim () + ")\n";
				}
			}
			else {
				if ((compinfo.CompanyName.Trim ().Length + compinfo.RegNo.Trim ().Length + 2) > 42) {
					text += compinfo.CompanyName.Trim () + "\n";
					text += "(" + compinfo.RegNo.Trim () + ")\n";
				}
				else {
					text += compinfo.CompanyName.Trim () + "(" + compinfo.RegNo.Trim () + ")\n";
				}
			}

			text += "USER ID  : " + USERID + "\n";
			text += "PRINT ON : " + DateTime.Now.ToString ("dd-MM-yyyy hh:mm tt") + "\n";
			text += "END DAY REPORT ON " + printdate1.ToString ("yy-MM-yyyy")+"-"+ printdate2.ToString ("yy-MM-yyyy") + "\n";
		    
			text += "------------------------------------------\n";
			text += "NO  INVOICE NO         \n";
			text += "------------------------------------------\n";

		}

	}
}
 


