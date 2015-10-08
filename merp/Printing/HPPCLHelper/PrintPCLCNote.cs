using System;
using Android.App;
using System.IO;
using System.Collections.Generic;

namespace wincom.mobile.erp
{
	
	public class PrintPCLCNote:PrintPCLDocumentBase,IPrintDocument
	{
		CNNote inv;
		CNNoteDtls[] list;
		int noOfCopy=1;
		Activity callingActivity;

		public void SetCallingActivity (Activity activity)
		{
			callingActivity = activity;
		}

		public bool StartPrint ()
		{
			return Print ();
		}

		public void SetDocument (object doc)
		{
			inv = (CNNote)doc;
		}

		public void SetDocumentDtls (object docdtls)
		{
			list = (CNNoteDtls[])docdtls;
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
		
			TCPHPPCLHelper device = new TCPHPPCLHelper();
			device.SetCallingActivity (callingActivity);
			device.SetIsPrintCompLogo (iSPrintCompLogo ());
			bool isReady = device.StartPrint (text, noOfCopy, ref errMsg);
			if (isReady) {
				isPrinted = PrintCNNote (device);
				device.Close ();
			}
			return isPrinted;
		}

		bool PrintCNNote(TCPHPPCLHelper device)
		{
			bool success = false;
			try {
				string dtltexts ="";
				List<string> dtlLines = prtDetail.GetPrintCNDetalis(inv, list);
				List<string> remlines = new List<string>();
				int lineRem;
				int lineRemark=0;
				if(!string.IsNullOrEmpty(inv.remark))
				{
					string remtext = "REMARK: "+inv.remark.ToUpper();
					remlines= PrintUtil.GetLine(remtext,110 );
					lineRemark= remlines.Count+1;
				}
				int page = Math.DivRem (dtlLines.Count+lineRemark, 20,out lineRem);
				bool squeezeTopage = false;
				if (lineRem < 4)
					squeezeTopage = true;
				else page +=1;

				int printpage = Math.DivRem (dtlLines.Count+lineRemark, 35, out lineRem);
				if (printpage ==0)
				{
					if (page>1)
					{
						int remlinecount= 35-dtlLines.Count+lineRemark;
						for(int i=0; i <remlinecount;i++)
						{
							dtlLines.Add("\r");
						}
					}
				}
				//1 2 3 4
				int page_no=1;
				string pageno="";
				int recno=35;
				int printedRecno=0;
				for (int i = 1; i < page; i++) {	
					pageno = string.Format("{0} OF {1}",i,(page==0)?1:page);
					prtcompHeader.PrintCNHeader (device.mmOutputStream, inv,pageno);
					for(int y=0; y<recno;y++)
					{
						prtDetail.PrintLine (device.mmOutputStream,dtlLines[y]);
						printedRecno+=1;
					}
					recno= recno+35;
					prtDetail.FormFeed(device.mmOutputStream);
					page_no=page_no+1;
				}

				int lastpageRecno=0;
				pageno = string.Format("{0} OF {1}",page_no,(page==0)?1:page);
				prtcompHeader.PrintCNHeader (device.mmOutputStream, inv,pageno);
				for(int y=printedRecno; y < dtlLines.Count ;y++)
				{
					prtDetail.PrintLine (device.mmOutputStream,dtlLines[y]);
					lastpageRecno+=1;
				}

				if ( remlines.Count>0)
				{
					prtDetail.PrintLine (device.mmOutputStream,"\r");
					foreach(string rline in remlines)
					{
						prtDetail.PrintLine (device.mmOutputStream, rline+"\r");
					}
				}
				string text = "";
				prtTaxSumm.PrintCNTaxSumm (ref text, list);
				string[] taxline = text.Split (new char[]{ '\r' });
				int tttline = lastpageRecno + taxline.Length;
				int remindline = 42 - (tttline + 1)-9; // 9 is the footer line
				SetLineFeed (device.mmOutputStream, remindline);
				prtDetail.PrintLine (device.mmOutputStream, text);
				prtFooter.PrintFooter (device.mmOutputStream, prtDetail.TotaTaxAmount, prtDetail.TotalNetAmount);
				success = true;
			} catch (Exception ex) {
				errMsg = ex.Message;
			}

			return success;
		}

		public void SetLineFeed(Stream mmOutputStream,int noOfline)
		{ 
			byte[]  charfont;
			charfont = new Byte[] { 10 }; 
			for (int i = 0; i < noOfline; i++) {
				mmOutputStream.Write (charfont, 0, charfont.Length);
			}

		}
	
	}
}

