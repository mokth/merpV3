﻿using System;

namespace wincom.mobile.erp
{
	public class PrintTotalAmount:PrintHelperBase
	{
		public void PrintTotal (ref string test,double ttlAmt,double ttlTax)
		{
			double ttlRounAmt = ttlAmt+ttlTax;

			test += "------------------------------------------\n";
			test += "               TOTAL EXCL GST "+Math.Round(ttlAmt,2).ToString("n2").PadLeft (12, ' ')+"\n";
			test += "               TOTAL TAX      "+Math.Round(ttlTax,2).ToString("n2").PadLeft (12, ' ')+"\n";
			test += "               TOTAL INCL GST "+Math.Round(ttlAmt+ttlTax,2).ToString("n2").PadLeft (12, ' ')+"\n";
			test += "------------------------------------------\n";
		}

		public void PrintTotalAjust (ref string test,double ttlAmt,double ttlTax)
		{
			double roundVal=0;
			double ttlRounAmt = Utility.AdjustToNear (ttlAmt+ttlTax, ref roundVal);

			test += "------------------------------------------\n";
			test += "               TOTAL EXCL GST "+Math.Round(ttlAmt,2).ToString("n2").PadLeft (12, ' ')+"\n";
			test += "               TOTAL TAX      "+Math.Round(ttlTax,2).ToString("n2").PadLeft (12, ' ')+"\n";
			test += "               TOTAL INCL GST "+Math.Round(ttlAmt+ttlTax,2).ToString("n2").PadLeft (12, ' ')+"\n";
			test += "               ROUNDING ADJ   "+Math.Round(roundVal,2).ToString("n2").PadLeft (12, ' ')+"\n";
			test += "               TOTAL AMOUNT   "+Math.Round(ttlRounAmt ,2).ToString("n2").PadLeft (12, ' ')+"\n";
			test += "------------------------------------------\n";
		}

		public void PrintTotal_NTax (ref string test,double ttlAmt,double ttlTax)
		{
			test += "------------------------------------------\n";
			test += "          TOTAL AMOUNT     "+Math.Round(ttlAmt,2).ToString("n2").PadLeft (14, ' ')+"\n";
			test += "------------------------------------------\n";
		}

		public void PrintTotal (ref string test,double cnttlAmt,double cnttlTax,double InvttlAmt,double invttlTax)
		{
			double roundVal = 0;
			//double ttlCollect = (InvttlAmt + invttlTax) - (cnttlAmt + cnttlTax);
			double ttlInv = InvttlAmt+invttlTax;
			double ttlCN = cnttlAmt+cnttlTax;
			double ttlCollect = ttlInv - ttlCN;
			test += "------------------------------------------\n";
			test += "  TOTAL INVOICE AMOUNT : "+Math.Round(ttlInv,2).ToString("n2").PadLeft (12, ' ')+"\n";
			test += "  TOTAL C/NOTE AMOUNT  : "+Math.Round(ttlCN,2).ToString("n2").PadLeft (12, ' ')+"\n";
			test += "  TOTAL COLLECT AMOUNT : "+Math.Round(ttlCollect,2).ToString("n2").PadLeft (12, ' ')+"\n";
			test += "------------------------------------------\n";
		}

		public void PrintTotalAjust (ref string test,double cnttlAmt,double cnttlTax,double InvttlAmt,double invttlTax)
		{
			double roundVal = 0;
			//double ttlCollect = (InvttlAmt + invttlTax) - (cnttlAmt + cnttlTax);
			double ttlInv = Utility.AdjustToNear (InvttlAmt+invttlTax, ref roundVal);
			double ttlCN = Utility.AdjustToNear (cnttlAmt+cnttlTax, ref roundVal);
			double ttlCollect = ttlInv - ttlCN;
			test += "------------------------------------------\n";
			test += "  TOTAL INVOICE AMOUNT : "+Math.Round(ttlInv,2).ToString("n2").PadLeft (12, ' ')+"\n";
			test += "  TOTAL C/NOTE AMOUNT  : "+Math.Round(ttlCN,2).ToString("n2").PadLeft (12, ' ')+"\n";
			test += "  TOTAL COLLECT AMOUNT : "+Math.Round(ttlCollect,2).ToString("n2").PadLeft (12, ' ')+"\n";
			test += "------------------------------------------\n";
		}

		public void PrintDOTotal (ref string test,double ttlQty)
		{

			test += "------------------------------------------\n";
			test += "                   TOTAL QTY  "+ttlQty.ToString("n").PadLeft (12, ' ')+"\n";
			test += "------------------------------------------\n";
		}
	}
}

