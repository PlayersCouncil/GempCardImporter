using GeneratorCSVHandler;

using System;
using System.IO;
using System.Threading.Tasks;

namespace GempCardImporter
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var reader = new GeneratorSheetReader();

            if (Directory.Exists("data"))
            {
                Directory.Delete("data", true);
            }

            Directory.CreateDirectory("data");

            if (Directory.Exists("output"))
            {
                Directory.Delete("output", true);
            }

            Directory.CreateDirectory("output");

			var cards = await reader.DownloadAndReadAllSheets("sheets.txt", "data");

			CardFileGenerator.GenerateFilesForCards(cards, "output");
		}
	}
}
