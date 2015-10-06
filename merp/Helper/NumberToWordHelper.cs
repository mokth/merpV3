using System;

namespace wincom.mobile.erp
{
	public class NumberToWordHelper
	{
		public static string NumberToWords(decimal number)
		{
			if (number == 0)
				return "ZERO";

			if (number < 0)
				return "MINUS " + NumberToWords(Math.Abs(number));

			string words = String.Empty;

			long intPortion = (long)number;
			decimal fraction = (number - intPortion);
			int decimalPrecision = GetDecimalPrecision(number);

			fraction = CalculateFraction(decimalPrecision, fraction);

			long decPortion = (long)fraction;

			words = IntToWords(intPortion);
			if (decPortion > 0)
			{
				words += " AND ";
				words += IntToWords (decPortion) + " CENT ONLY";
			} else words += " ONLY";

			return words.Trim();
		}

		public static string IntToWords(long number)
		{
			if (number == 0)
				return "ZERO";

			if (number < 0)
				return "MINUS " + IntToWords(Math.Abs(number));

			string words = "";

			if ((number / 1000000000000000) > 0)
			{
				words += IntToWords(number / 1000000000000000) + " QUADRILLION ";
				number %= 1000000000000000;
			}

			if ((number / 1000000000000) > 0)
			{
				words += IntToWords(number / 1000000000000) + " TRILLION ";
				number %= 1000000000000;
			}

			if ((number / 1000000000) > 0)
			{
				words += IntToWords(number / 1000000000) + " BILLION ";
				number %= 1000000000;
			}

			if ((number / 1000000) > 0)
			{
				words += IntToWords(number / 1000000) + " MILLION ";
				number %= 1000000;
			}

			if ((number / 1000) > 0)
			{
				words += IntToWords(number / 1000) + " THOUSAND ";
				number %= 1000;
			}

			if ((number / 100) > 0)
			{
				words += IntToWords(number / 100) + " HUNDRED ";
				number %= 100;
			}

			if (number > 0)
			{
				if (words != String.Empty)
					words += "AND ";

				var unitsMap = new[] { "ZERO", "ONE", "TWO", "THREE", "FOUR", "FIVE", "SIX", "SEVEN", "EIGHT", "NINE", "TEN", "ELEVEN", "TWELVE", "THIRTEEN", "FOURTEEN", "FIFTEEN", "SIXTEEN", "SEVENTEEN", "EIGHTEEN", "NINETEEN" };
				var tensMap = new[] { "ZERO", "TEN", "TWENTY", "THIRTY", "FORTY", "FIFTY", "SIXTY", "SEVENTY", "EIGHTY", "NINETY" };

				if (number < 20)
					words += unitsMap[number];
				else
				{
					words += tensMap[number / 10];
					if ((number % 10) > 0)
						words += "-" + unitsMap[number % 10];
				}
			}

			return words.Trim();
		}

		private static int GetDecimalPrecision(decimal number)
		{
			return (Decimal.GetBits(number)[3] >> 16) & 0x000000FF;
		}

		private static decimal CalculateFraction(int decimalPrecision, decimal fraction)
		{
			switch (decimalPrecision)
			{
			case 1:
				return fraction * 10;
			case 2:
				return fraction * 100;
			case 3:
				return fraction * 1000;
			case 4:
				return fraction * 10000;
			case 5:
				return fraction * 100000;
			case 6:
				return fraction * 1000000;
			case 7:
				return fraction * 10000000;
			case 8:
				return fraction * 100000000;
			case 9:
				return fraction * 1000000000;
			case 10:
				return fraction * 10000000000;
			case 11:
				return fraction * 100000000000;
			case 12:
				return fraction * 1000000000000;
			case 13:
				return fraction * 10000000000000;
			default:
				return fraction * 10000000000000;
			}
		}
	}
}

