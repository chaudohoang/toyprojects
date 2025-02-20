using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        // Ensure at least one file is provided via drag-and-drop or arguments
        if (args.Length == 0)
        {
            Console.WriteLine("Error: No files provided. Drag and drop files onto this executable or pass them as arguments.");
            return;
        }

        // Get the current directory
        string currentDir = Directory.GetCurrentDirectory();

        // Get all directories in the current directory
        string[] directories = Directory.GetDirectories(currentDir);

        Console.WriteLine("Starting the file copy process...");

        foreach (var file in args)
        {
            if (!File.Exists(file))
            {
                Console.WriteLine($"Skipping: {file} (File not found)");
                continue;
            }

            string fileName = Path.GetFileName(file);

            foreach (var directory in directories)
            {
                string folderName = new DirectoryInfo(directory).Name;

                // Check if the folder starts with "D" and does not contain "00"
                if (folderName.StartsWith("D", StringComparison.OrdinalIgnoreCase) && !folderName.Contains("00"))
                {
                    try
                    {
                        // Copy the file to the folder
                        string destination = Path.Combine(directory, fileName);
                        File.Copy(file, destination, true);
                        Console.WriteLine($"Copied '{fileName}' to: {folderName}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to copy '{fileName}' to {folderName}: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"Skipping folder: {folderName} (Does not meet conditions)");
                }
            }
        }

        Console.WriteLine("Done!");
    }
}
