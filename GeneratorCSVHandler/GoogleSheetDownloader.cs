using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GeneratorCSVHandler
{
	public static class GoogleSheetDownloader
	{
		public static async Task DownloadGoogleSheet(string sheetID, string path)
		{
			//https://docs.google.com/spreadsheets/d/1-0C3sAm78A0x7-w_rfuWuH87Fta60m2xNzmAE2KFBNE/export?format=tsv

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"https://docs.google.com/spreadsheets/d/{sheetID}/export?format=tsv");
			request.Method = "GET";

			try
			{
				var webResponse = await request.GetResponseAsync();
				using (Stream webStream = webResponse.GetResponseStream() ?? Stream.Null)
				using (StreamReader responseReader = new StreamReader(webStream))
				{
					string response = responseReader.ReadToEnd();
					File.WriteAllText(Path.Combine(path, $"{sheetID}.tsv"), response);
				}
			}
			catch (Exception e)
			{
				Console.Out.WriteLine(e);
			}
		}
	}
}
