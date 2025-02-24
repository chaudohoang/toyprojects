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
                string adsPath = $"\\\\?\\{filePath}:Zone.Identifier";

                if (File.Exists(adsPath))
                {
                    File.Delete(adsPath);
                    Console.WriteLine($"Unblocked: {filePath}");
                }
                else
                {
                    Console.WriteLine($"Already unblocked: {filePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to unblock {filePath}: {ex.Message}");
            }
        }

        Console.WriteLine("Process completed. Press any key to exit.");
        Console.ReadKey();
    }
}