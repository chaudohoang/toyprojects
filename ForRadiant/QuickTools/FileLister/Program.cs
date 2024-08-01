using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace FileLister
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Enter the path to the folder: ");
            string inputFolder = Console.ReadLine();

            Console.Write("Enter the path to the output CSV file: ");
            string outputCsv = Console.ReadLine();

            var filesInfo = ListFilesInDirectory(inputFolder);
            SaveToCsv(filesInfo, outputCsv);

            Console.WriteLine($"File information saved to {outputCsv}");
        }

        static List<FileInfoModel> ListFilesInDirectory(string directory)
        {
            var filesInfo = new List<FileInfoModel>();
            foreach (var filePath in Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories))
            {
                var fileInfo = new FileInfo(filePath);
                filesInfo.Add(new FileInfoModel
                {
                    Filename = fileInfo.FullName,
                    CreatedTime = fileInfo.CreationTime,
                    ModifiedTime = fileInfo.LastWriteTime
                });
            }
            return filesInfo;
        }

        static void SaveToCsv(List<FileInfoModel> filesInfo, string outputCsv)
        {
            using (var writer = new StreamWriter(outputCsv))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                csv.WriteRecords(filesInfo);
            }
        }
    }

    public class FileInfoModel
    {
        public string Filename { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime ModifiedTime { get; set; }
    }
}
