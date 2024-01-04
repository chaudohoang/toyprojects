using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using CsvHelper;
using CsvHelper.Configuration;
using MatchingSummaryLogForAOIDetect.MatchingSummaryLogForAOIDetect;

namespace MatchingSummaryLogForAOIDetect
{
    class Program
    {
        static void Main()
        {
            List<string> file1Folders = new List<string>
            {
                "D:\\Trace Spot Mono Camera\\Station 1 Color CSV",
                "D:\\Trace Spot Mono Camera\\Station 2 Color CSV"
                // Add more folders as needed
            };

            List<string> file2Folders = new List<string>
            {
                "D:\\Trace Spot Mono Camera\\Station 1 Mono CSV",
                "D:\\Trace Spot Mono Camera\\Station 2 Mono CSV"
                // Add more folders as needed
            };

            string outputCsvFilePath = "D:\\Trace Spot Mono Camera\\Match CSV\\Matched_"+DateTime.Now.ToString("yyyyMMdd")+".csv";

            List<OutputRecord> matchingRecords = new List<OutputRecord>();

            foreach (var file1Folder in file1Folders)
            {
                List<string> file1Paths = Directory.GetFiles(file1Folder, "*.csv").ToList();

                foreach (var file1Path in file1Paths)
                {
                    List<RecordFile1> file1Records = ReadCsvFile<RecordFile1, RecordFile1Map>(file1Path);

                    foreach (var file2Folder in file2Folders)
                    {
                        List<string> file2Paths = Directory.GetFiles(file2Folder, "*.csv").ToList();

                        foreach (var file2Path in file2Paths)
                        {
                            List<RecordFile2> file2Records = ReadCsvFile<RecordFile2, RecordFile2Map>(file2Path);

                            foreach (var record1 in file1Records)
                            {
                                if (record1.Description.IndexOf("spot", StringComparison.OrdinalIgnoreCase) != -1 && !string.IsNullOrEmpty(record1.DefectInfo))
                                {
                                    var matchingRecord2 = file2Records.FirstOrDefault(record2 => record2.PID.Equals(record1.PID, StringComparison.OrdinalIgnoreCase));

                                    if (matchingRecord2 != null)
                                    {
                                        matchingRecords.Add(new OutputRecord
                                        {
                                            PID = record1.PID,
                                            MonoStation = matchingRecord2.EQPID,
                                            ColorStation = record1.EQPID,
                                            MonoChannel = matchingRecord2.CH,
                                            ColorChannel = record1.CH,
                                            Description = record1.Description,
                                            DefectInfo = record1.DefectInfo
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }

            WriteCsvFile(outputCsvFilePath, matchingRecords);
        }

        static List<T> ReadCsvFile<T, TMap>(string filePath)
            where T : class
            where TMap : ClassMap<T>
        {
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture) { HasHeaderRecord = true, PrepareHeaderForMatch = args => args.Header.ToLower() }))
            {
                csv.Context.RegisterClassMap<TMap>();
                return csv.GetRecords<T>().ToList();
            }
        }

        static void WriteCsvFile<T>(string filePath, IEnumerable<T> records)
        {
            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)))
            {
                csv.WriteRecords(records);
            }
        }
    }

    namespace MatchingSummaryLogForAOIDetect
    {
        class RecordFile1
        {
            public string PID { get; set; }
            public string CH { get; set; }
            public string EQPID { get; set; }
            public string Description { get; set; }
            public string DefectInfo { get; set; }
        }

        class RecordFile2
        {
            public string PID { get; set; }
            public string CH { get; set; }
            public string EQPID { get; set; }
        }

        class OutputRecord
        {
            public string PID { get; set; }
            public string MonoStation { get; set; }
            public string MonoChannel { get; set; }
            public string ColorStation { get; set; }            
            public string ColorChannel { get; set; }
            public string Description { get; set; }
            public string DefectInfo { get; set; }
        }

        class RecordFile1Map : ClassMap<RecordFile1>
        {
            public RecordFile1Map()
            {
                Map(m => m.PID).Name("PID");
                Map(m => m.CH).Name("CH");
                Map(m => m.EQPID).Name("EQP ID");
                Map(m => m.Description).Name("Description");
                Map(m => m.DefectInfo).Name("Defect Info");
            }
        }

        class RecordFile2Map : ClassMap<RecordFile2>
        {
            public RecordFile2Map()
            {
                Map(m => m.PID).Name("PID");
                Map(m => m.CH).Name("CH");
                Map(m => m.EQPID).Name("EQP ID");
            }
        }
    }
}
