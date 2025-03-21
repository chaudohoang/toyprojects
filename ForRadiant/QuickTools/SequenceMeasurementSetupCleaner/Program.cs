using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SequenceMeasurementSetupCleaner
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Drag and drop sequence files onto this executable.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            foreach (string filePath in args)
            {
                if (!File.Exists(filePath) || Path.GetExtension(filePath).ToLower() != ".seqxc")
                {
                    Console.WriteLine($"Skipping invalid file: {filePath}");
                    continue;
                }

                Console.WriteLine($"\nProcessing: {Path.GetFileName(filePath)}");

                try
                {
                    ProcessXml(filePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing {filePath}: {ex.Message}");
                }
            }

            Console.WriteLine("\nAll files processed. Press any key to exit...");
            Console.ReadKey();
        }

        static void ProcessXml(string filePath)
        {
            XDocument doc = XDocument.Load(filePath);

            // Collect PatternSetupNames with Selected=true
            var usedPatterns = new HashSet<string>(
                doc.Descendants("SequenceItem")
                   .Where(x => (string)x.Element("Selected") == "true")
                   .Select(x => (string)x.Element("PatternSetupName"))
                   .Where(x => !string.IsNullOrEmpty(x))
            );

            Console.WriteLine($"Found {usedPatterns.Count} used patterns to keep.");

            // Remove PatternSetups not in the usedPatterns set
            var patternSetups = doc.Descendants("PatternSetup").ToList();
            int removedCount = 0;
            foreach (var pattern in patternSetups)
            {
                var patternName = pattern.Descendants("PatternSetupName").FirstOrDefault()?.Value;
                if (patternName == null || !usedPatterns.Contains(patternName))
                {
                    pattern.Remove();
                    removedCount++;
                }
            }

            Console.WriteLine($"Removed {removedCount} unused PatternSetup entries.");

            // Backup original file
            string backupPath = Path.Combine(Path.GetDirectoryName(filePath),
                                             Path.GetFileNameWithoutExtension(filePath) + "_backup.seqxc");
            File.Copy(filePath, backupPath, overwrite: true);
            Console.WriteLine($"Backup created: {Path.GetFileName(backupPath)}");

            // Overwrite original file with cleaned XML
            doc.Save(filePath);
            Console.WriteLine($"Cleaned sequence saved: {Path.GetFileName(filePath)}");

            Console.WriteLine("\nAll files processed. Press any key to exit...");
            Console.ReadKey();
        }
    }
}
