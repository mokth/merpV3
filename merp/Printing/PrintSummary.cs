using System;
using System.Data;
using System.Xml.Linq;
using System.Linq;
using System.Linq.Expressions;
using Android.App;

namespace wincom.mobile.erp
{
	public class PrintSummary:PrintDocumentBase,IPrintDocument
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

		internal virtual string GetInvoiceSumm(DateTime printdate1,DateTime printdate2 )
		{
			string text = "";
			bool isSamedate = printdate1==printdate2;
			PrintSummHeader (printdate1, printdate2, ref text, isSamedate);
			var invs = DataHelper.GetInvoices (printdate1, printdate2);

			var grp= from inv in invs 
				group inv by inv.invdate 
				into g select new {key=g.Key,results=g};

			double ttlcash = 0;
			double ttlInv = 0;
			double ttltax = 0;
			double ttlamt = 0;
			double subttltax = 0;
			double subttlamt = 0;
			bool multiType = false;
			string line = "";
			int cont = 0;
			foreach (var g in grp) { //group by date
				var list = g.results.OrderBy (x => x.invno);
				var typgrp = from ty in list
					group ty by ty.trxtype	into tg
					select new {key = tg.Key,results = tg};

				if (!isSamedate) {
					text = text + g.key.ToString ("dd-MM-yyyy") + "\n";
					text = text +"---------- \n";
				}
				multiType = (typgrp.Count() > 1);
				foreach (var tygrp in typgrp) {  //group by trxtype
					text = text +"[ "+ tygrp.key.ToUpper() + " ]\n";
					var list2 = tygrp.results.OrderBy (x => x.invno);
					subttltax = 0;
					subttlamt = 0;
					cont = 0;
					foreach (Invoice inv in list2) {
						cont += 1;
						ttltax += inv.taxamt;
						ttlamt += inv.amount;
						subttltax += inv.taxamt;
						subttlamt += inv.amount;
						if (tygrp.key.ToUpper () == "CASH") {
							ttlcash = ttlcash + inv.amount + inv.taxamt;
						}else ttlInv = ttlInv + inv.amount + inv.taxamt;
						line = (cont.ToString () + ".").PadRight (4, ' ') +
							inv.invno.PadRight (13, ' ') +
							inv.trxtype.PadRight (8, ' ') +
							inv.taxamt.ToString ("n2").PadLeft (9, ' ') +
							inv.amount.ToString ("n2").PadLeft (8, ' ') + "\n";
						text = text + line;
					}
					//if (multiType) {
					if (typgrp.Count()>1)
						text = text + PrintSubTotal (subttltax, subttlamt);
					//	}
				}
			}

			double ttlCNTax = 0;
			double ttlCNAmt = 0;
			double ttlCNCODTax = 0;
			double ttlCNCODAmt = 0;
			double ttlCN = 0;
			double ttlCNCOD = 0;
			var cns = DataHelper.GetCNNote(printdate1, printdate2);
			foreach (CNNote cn in cns) {
				if (cn.trxtype == "CASH") {
					ttlCNCODTax = ttlCNCODTax + cn.taxamt;
					ttlCNCODAmt = ttlCNCODAmt + cn.amount;
				} else {
					ttlCNTax = ttlCNTax + cn.taxamt;
					ttlCNAmt = ttlCNAmt + cn.amount;
				}
			}
			ttlCN = ttlCNTax + ttlCNAmt;
			ttlCNCOD = ttlCNCODTax + ttlCNCODAmt;

			double ttl = ttlamt + ttltax;
			double cashCollect = ttlcash - ttlCNCOD;
			PrintSummFooter (ref text, ttlcash, ttlInv, ttltax, ttlamt, ttlCN, ttlCNCOD, ttl, cashCollect);

			return text;
		}

		internal virtual void PrintSummFooter (ref string text, double ttlcash, double ttlInv, double ttltax, double ttlamt, double ttlCN, double ttlCNCOD, double ttl, double cashCollect)
		{
			text += "------------------------------------------\n";
			text += "TOTAL TAX     :" + ttltax.ToString ("n2").PadLeft (13, ' ') + "\n";
			text += "TOTAL AMOUNT  :" + ttlamt.ToString ("n2").PadLeft (13, ' ') + "\n";
			text += "      TOTAL   :" + ttl.ToString ("n2").PadLeft (13, ' ') + "\n";
			text += "------------------------------------------\n";
			text += "SUMMARY\n";
			text += "TOTAL CASH    :" + ttlcash.ToString ("n2").PadLeft (13, ' ') + "\n";
			text += "TOTAL INVOICE :" + ttlInv.ToString ("n2").PadLeft (13, ' ') + "\n\n";
			text += "TOTAL CN CASH :" + ttlCNCOD.ToString ("n2").PadLeft (13, ' ') + "\n";
			text += "TOTAL CN INV  :" + ttlCN.ToString ("n2").PadLeft (13, ' ') + "\n";
			text += "TOTAL CASH COLLECT :" + cashCollect.ToString ("n2").PadLeft (13, ' ') + "\n";
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
		}

		internal virtual void PrintSummHeader (DateTime printdate1, DateTime printdate2, ref string text, bool isSamedate)
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
			if (isSamedate)
				text += "DAILTY SUMMARY ON " + printdate1.ToString ("yy-MM-yyyy") + "\n";
			else {
				text += "DAILTY SUMMARY ON " + printdate1.ToString ("yy-MM-yyyy") + " - " + printdate2.ToString ("yy-MM-yyyy") + "\n";
			}
			text += "------------------------------------------\n";
			text += "NO  INVOICE NO   TYPE     TAX AMT   AMOUNT\n";
			text += "------------------------------------------\n";
		}

		internal virtual string PrintSubTotal(double ttltax,double ttlamt)
		{
			double ttl = ttlamt + ttltax;
			string text ="";
			text += "------------------------------------------\n";
			text += " SUB TOTAL TAX    :" + ttltax.ToString ("n2").PadLeft (13, ' ')+"\n";
			text += " SUB TOTAL AMOUNT :" + ttlamt.ToString ("n2").PadLeft (13, ' ')+"\n";
			text += "        SUB TOTAL :" + ttl.ToString ("n2").PadLeft (13, ' ')+"\n";
			//text += "------------------------------------------\n";
			return text;				
		}



	}
}

