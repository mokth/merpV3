using System;

namespace wincom.mobile.erp
{
	public class CustomTool
	{
		public static double Round(double val, int decplace)
		{
			return Math.Round(val, decplace);
		}

		public static string RoundFormatPadLeft(double val, int decplace)
		{
			return Math.Round(val, decplace).ToString("n2");
		}

		public static string PadLeft2Decplace(string val, int len)
		{
			double nval = Convert.ToDouble(val);
			return nval.ToString("n2").PadLeft(len, ' ');
		}

		public static string PadLeftOnly(string val, int len)
		{
			return val.PadLeft(len, ' ');
		}

		public static string FormatNumStr(string val)
		{
			double nval = Convert.ToDouble(val);
			return nval.ToString("n2");
		}

		public static string PadRight(string val, int len)
		{
			return val.PadRight(len,' ');
		}
	}
}

