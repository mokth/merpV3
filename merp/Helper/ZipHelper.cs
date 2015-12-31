using System;
using System.IO;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Zip;

namespace wincom.mobile.erp
{
	public class ZipHelper
	{
		public static void Decompress(string fileToDecompress, string newFileName)
		{
			using (FileStream originalFileStream = File.OpenRead(fileToDecompress))
			{

				using (FileStream decompressedFileStream = File.Create(newFileName))
				{
					using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
					{
						decompressionStream.CopyTo(decompressedFileStream);
					}
				}
			}
		}

		public static void DecompressFiles(string fileToDecompress, string PathtoExtract)
		{
			using (ZipInputStream s = new ZipInputStream(File.OpenRead(fileToDecompress)))
			{
				ZipEntry theEntry;
				while ((theEntry = s.GetNextEntry()) != null)
				{
					string directoryName = Path.GetDirectoryName(theEntry.Name);
					string fileName = Path.GetFileName(theEntry.Name);

					string fullfilename = Path.Combine(PathtoExtract, fileName);

					if (fileName != String.Empty)
					{
						using (FileStream streamWriter = File.Create(fullfilename))
						{

							int size = 2048;
							byte[] data = new byte[2048];
							while (true)
							{
								size = s.Read(data, 0, data.Length);
								if (size > 0)
								{
									streamWriter.Write(data, 0, size);
								}
								else
								{
									break;
								}
							}
						}
					}
				}
			}
		}

		public static string GetZipFileName(string filename)
		{
			string zipfile = filename.Replace (".db", ".zip");
			try {

				using (ZipOutputStream s = new ZipOutputStream (File.Create (zipfile))) {
					s.SetLevel (9); // 0 - store only to 9 - means best compression
					byte[] buffer = new byte[4096];
					ZipEntry entry = new ZipEntry (filename);
					entry.DateTime = DateTime.Now;
					s.PutNextEntry (entry);

					using (FileStream fs = File.OpenRead (filename)) {
						int sourceBytes;
						do {
							sourceBytes = fs.Read (buffer, 0, buffer.Length);
							s.Write (buffer, 0, sourceBytes);
						} while (sourceBytes > 0);
					}
					s.Finish ();
					s.Close ();
				}
			} catch (Exception ex) {
				zipfile = filename;
			}
			return zipfile;
		}
	}
}

