using FileHelpers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratorCSVHandler
{
	public class GeneratorSheetReader
	{
		public List<CardRow> ReadSheet(string path)
		{
			var engine = new FileHelperEngine<CardRow>(Encoding.UTF8);
			var cards = engine.ReadFile(path).ToList();
			cards.ForEach(x => x.Origin = path);

			return cards;
		}

		public List<CardRow> ReadSheets(string folder)
		{
			var cards = new List<CardRow>();
			foreach(string file in Directory.EnumerateFiles(folder, "*.tsv", SearchOption.AllDirectories))
			{
				cards.AddRange(ReadSheet(file));
			}

			return cards;
		}

		public async Task<List<CardRow>> DownloadAndReadAllSheets(IEnumerable<string> sheetIDs, string folder)
		{
			foreach (string sheetID in sheetIDs)
			{
				if(!File.Exists(Path.Combine(folder, $"{sheetID}.tsv")))
				{
					await GoogleSheetDownloader.DownloadGoogleSheet(sheetID, folder);
				}
			}

			return ReadSheets(folder);
		}

		public async Task<List<CardRow>> DownloadAndReadAllSheets(string sheetPath, string folder)
		{
			return await DownloadAndReadAllSheets(File.ReadLines(sheetPath), folder);
		}
	}
}
