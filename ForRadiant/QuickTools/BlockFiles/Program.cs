using System;
using System.IO;

class Program
{
    static void Main()
    {
        string currentDirectory = Directory.GetCurrentDirectory();
        string[] files = Directory.GetFiles(currentDirectory);

        if (files.Length == 0)
        {
            Console.WriteLine("No files found in the current directory.");
            return;
        }

        foreach (string filePath in files)
        {
            try
            {
                string adsPath = $@"\\?\{filePath}:Zone.Identifier";

                using (FileStream fs = new FileStream(adsPath, FileMode.Create, FileAccess.Write))
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    writer.WriteLine("[ZoneTransfer]");
                    writer.WriteLine("ZoneId=3");
                }

                Console.WriteLine($"Blocked: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to block {filePath}: {ex.Message}");
            }
        }

        Console.WriteLine("Process completed. Press any key to exit.");
        Console.ReadKey();
    }
}
