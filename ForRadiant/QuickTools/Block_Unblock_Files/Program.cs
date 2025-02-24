using System;
using System.IO;

class Program
{
    static void Main()
    {
        string[] targetDirectories = { @"D:\DATABASE\InputIMG", @"E:\DATABASE\InputIMG", @"C:\Program Files\Radiant Vision Systems\TrueTest 1.8" };

        Console.WriteLine("Processing the following directories:");
        foreach (string dir in targetDirectories)
        {
            Console.WriteLine(dir);
        }

        Console.WriteLine("Choose an option: ");
        Console.WriteLine("1 - Block files");
        Console.WriteLine("2 - Unblock files");
        Console.Write("Enter choice: ");
        string choice = Console.ReadLine();

        Console.WriteLine("Processing the following directories:");
        foreach (string dir in targetDirectories)
        {
            Console.WriteLine(dir);
        }

        if (choice == "1")
        {
            ProcessFiles(targetDirectories, true);
        }
        else if (choice == "2")
        {
            ProcessFiles(targetDirectories, false);
        }
        else
        {
            Console.WriteLine("Invalid choice. Exiting.");
        }

        Console.WriteLine("Process completed. Press any key to exit.");
        Console.ReadKey();
    }

    static void ProcessFiles(string[] directories, bool block)
    {
        foreach (string dir in directories)
        {
            if (Directory.Exists(dir))
            {
                foreach (string filePath in Directory.GetFiles(dir, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        string adsPath = $"\\\\?\\{filePath}:Zone.Identifier";

                        if (block)
                        {
                            using (FileStream fs = new FileStream(adsPath, FileMode.Create, FileAccess.Write))
                            using (StreamWriter writer = new StreamWriter(fs))
                            {
                                writer.WriteLine("[ZoneTransfer]");
                                writer.WriteLine("ZoneId=3");
                            }
                            Console.WriteLine($"Blocked: {filePath}");
                        }
                        else
                        {
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
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to process {filePath}: {ex.Message}");
                    }
                }
            }
            else
            {
                Console.WriteLine($"Directory not found: {dir}");
            }
        }
    }
}
