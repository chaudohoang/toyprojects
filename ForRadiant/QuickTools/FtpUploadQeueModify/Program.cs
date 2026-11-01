using System;
using System.IO;
using System.Linq;
using System.Threading;

class IPUpdater
{
    private static bool isRunning = true;

    static void Main()
    {
        // Handle console close event
        Console.CancelKeyPress += OnConsoleClosing;

        Console.WriteLine("=== FTP Queue IP Updater ===");
        Console.WriteLine();

        // Prompt for NEW IP only
        Console.Write("Enter NEW IP address: ");
        string newIP = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(newIP))
        {
            Console.WriteLine("Invalid IP. Exiting.");
            Thread.Sleep(2000);
            return;
        }

        Console.WriteLine();

        // Prompt for queue folder
        Console.Write("Enter queue folder path (or press Enter for default): ");
        string queueFolder = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(queueFolder))
        {
            queueFolder = @"D:\Program\RVS\UploadQueue";
        }

        if (!Directory.Exists(queueFolder))
        {
            Console.WriteLine($"Error: Folder not found: {queueFolder}");
            Thread.Sleep(2000);
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"Scanning folder: {queueFolder}");

        // Find all .txt files
        string[] files = Directory.GetFiles(queueFolder, "*.txt", SearchOption.TopDirectoryOnly);
        Console.WriteLine($"Found {files.Length} queue files");
        Console.WriteLine();

        // Ask for confirmation
        Console.Write($"Update IP to {newIP} in all files? (y/n): ");
        if (Console.ReadLine()?.ToLower() != "y")
        {
            Console.WriteLine("Cancelled.");
            Thread.Sleep(1000);
            return;
        }

        Console.WriteLine();
        Console.WriteLine("Processing... (Press Ctrl+C to cancel)");
        Console.WriteLine();

        int updatedCount = 0;
        int skippedCount = 0;
        int errorCount = 0;
        int totalFiles = files.Length;
        int currentFile = 0;

        foreach (string filePath in files)
        {
            if (!isRunning)
            {
                Console.WriteLine();
                Console.WriteLine("Cancelled by user.");
                break;
            }

            currentFile++;

            try
            {
                string[] lines = File.ReadAllLines(filePath);

                if (lines.Length > 0 && !string.IsNullOrWhiteSpace(lines[0]))
                {
                    // Detect old IP (assume line 0 is always an IP)
                    string oldIP = lines[0].Trim();

                    // Skip if already the new IP
                    if (oldIP == newIP)
                    {
                        skippedCount++;
                    }
                    else
                    {
                        // Update the IP directly
                        lines[0] = newIP;
                        File.WriteAllLines(filePath, lines);

                        updatedCount++;
                    }
                }
                else
                {
                    skippedCount++;
                }
            }
            catch (Exception)
            {
                errorCount++;
            }

            // Update progress on same line using carriage return
            Console.Write($"Processed {currentFile}/{totalFiles} files... (Updated: {updatedCount}, Skipped: {skippedCount}, Errors: {errorCount})");
            Console.Write("\r");
        }

        // Move to next line after completion
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("=== Summary ===");
        Console.WriteLine($"Total files: {totalFiles}");
        Console.WriteLine($"Updated: {updatedCount}");
        Console.WriteLine($"Skipped: {skippedCount}");
        Console.WriteLine($"Errors: {errorCount}");
        Console.WriteLine();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    private static void OnConsoleClosing(object sender, ConsoleCancelEventArgs e)
    {
        isRunning = false;
        e.Cancel = true; // Prevent immediate termination
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("Stopping gracefully... Please wait.");
        Thread.Sleep(1000);
    }
}