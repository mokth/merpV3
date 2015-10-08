using System;
using System.Collections.Generic;

namespace wincom.mobile.erp
{
	public class PrintUtil
	{
		public static List<string> GetLine(string line,int lineLen) 
		{
			string[] text = line.Split(new char[] { ' ','\n','\r' });
			List<string> lines = new List<string>();
			string str = "";
			foreach (string txt in text)
			{
				if ((str.Length + txt.Length + 1) < lineLen)
					str = str + txt + " ";
				else
				{
					lines.Add(str);
					str = txt+" ";
				}
			}
			lines.Add(str);

			return lines;
		}
		public static List<string> GetNote(string line,int lineLen) 
		{
			string[] text = line.Split(new char[] { '\n','\r' });
			List<string> lines = new List<string>();
			string str = "";
			foreach (string txt in text)
			{
				if ((str.Length + txt.Length + 1) < lineLen)
					str = str + txt + " ";
				else
				{
					lines.Add(str);
					str = txt+" ";
				}
			}
			lines.Add(str);

			return lines;
		}
	}
}

