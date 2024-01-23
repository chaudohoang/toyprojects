using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using CsvHelper;
using CsvHelper.Configuration;

namespace MatchingSummaryLogForAOIDetect
{
    class Program
    {
        static void Main()
        {
            List<string> colorLogFolders = new List<string>
            {
                "D:\\Trace Spot Mono Camera\\Station 1 Color CSV",
                "D:\\Trace Spot Mono Camera\\Station 2 Color CSV"
                // Add more folders as needed
            };

            List<string> monoLogFolders = new List<string>
            {
                "D:\\Trace Spot Mono Camera\\Station 1 Mono CSV",
                "D:\\Trace Spot Mono Camera\\Station 2 Mono CSV"
                // Add more folders as needed
            };

            string outputCsvFilePath = "D:\\Trace Spot Mono Camera\\Match CSV\\Matched_"+DateTime.Now.ToString("yyyyMMdd")+".csv";

            List<OutputRecord> matchingRecords = new List<OutputRecord>();

            foreach (var coloLogFolder in colorLogFolders)
            {
                List<string> colorCsvs = Directory.GetFiles(coloLogFolder, "*.csv").ToList();

                foreach (var colorCsv in colorCsvs)
                {
                    List<ColorRecordFile> colorRecords = ReadCsvFile<ColorRecordFile, ColorRecordFileMap>(colorCsv);

                    foreach (var monoLogFolder in monoLogFolders)
                    {
                        List<string> monoCsvs = Directory.GetFiles(monoLogFolder, "*.csv").ToList();

                        foreach (var monoCsv in monoCsvs)
                        {
                            List<MonoRecordFile> monoRecords = ReadCsvFile<MonoRecordFile, MonoRecordFileMap>(monoCsv);

                            foreach (var colorRecord in colorRecords)
                            {
                                if (colorRecord.Description.IndexOf("spot", StringComparison.OrdinalIgnoreCase) != -1 && !string.IsNullOrEmpty(colorRecord.DefectInfo))
                                {
                                    var matchingRecord = monoRecords.FirstOrDefault(monoRecord => monoRecord.PID.Equals(colorRecord.PID, StringComparison.OrdinalIgnoreCase));

                                    if (matchingRecord != null)
                                    {
                                        matchingRecords.Add(new OutputRecord
                                        {
                                            PID = colorRecord.PID,
                                            MonoStation = matchingRecord.EQPID,
                                            ColorStation = colorRecord.EQPID,
                                            MonoChannel = matchingRecord.CH,
                                            ColorChannel = colorRecord.CH,
                                            DefectInfo = colorRecord.DefectInfo
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

   
    class ColorRecordFile
    {
        public string PID { get; set; }
        public string CH { get; set; }
        public string EQPID { get; set; }
        public string Description { get; set; }
        public string DefectInfo { get; set; }
    }

    class MonoRecordFile
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
        public string DefectInfo { get; set; }
    }

    class ColorRecordFileMap : ClassMap<ColorRecordFile>
    {
        public ColorRecordFileMap()
        {
            Map(m => m.PID).Name("PID");
            Map(m => m.CH).Name("CH");
            Map(m => m.EQPID).Name("EQP ID");
            Map(m => m.Description).Name("Description");
            Map(m => m.DefectInfo).Name("Defect Info");
        }
    }

    class MonoRecordFileMap : ClassMap<MonoRecordFile>
    {
        public MonoRecordFileMap()
        {
            Map(m => m.PID).Name("PID");
            Map(m => m.CH).Name("CH");
            Map(m => m.EQPID).Name("EQP ID");
        }
    }
   
}
