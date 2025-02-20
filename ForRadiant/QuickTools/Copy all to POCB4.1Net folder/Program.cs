using System;
using System.IO;
using System.Diagnostics;
using System.Linq;

class Program
{
    static void Main()
    {
        // Detect if TrueTest.exe is running and capture its path before killing it
        string trueTestPath = null;
        var process = Process.GetProcessesByName("TrueTest").FirstOrDefault();

        if (process != null)
        {
            trueTestPath = process.MainModule.FileName;  // Capture path before terminating
            Console.WriteLine("TrueTest.exe is running. Terminating process...");
            process.Kill();
            Console.WriteLine("Process terminated.");
        }
        else
        {
            Console.WriteLine("TrueTest.exe is not running.");
        }

        // Ask user to select target directory
        Console.WriteLine("Select a target directory:");
        Console.WriteLine("[1] C:\\Program Files\\Radiant Vision Systems\\TrueTest 1.8\\POCB4.1Net");
        Console.WriteLine("[2] C:\\Program Files\\Radiant Vision Systems\\TrueTest 1.8 debug Mobile\\POCB4.1Net");
        char choice = Console.ReadKey().KeyChar;

        string targetDir = (choice == '1')
            ? @"C:\Program Files\Radiant Vision Systems\TrueTest 1.8\POCB4.1Net"
            : @"C:\Program Files\Radiant Vision Systems\TrueTest 1.8 debug Mobile\POCB4.1Net";

        Console.WriteLine(); // Just to add a new line after the choice

        // Ensure the target directory exists
        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        // Copy all files except the executable, .tif, .csv, and LGD_VNTT.ini
        string scriptName = Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        var files = Directory.GetFiles(Directory.GetCurrentDirectory());

        foreach (var file in files)
        {
            string fileName = Path.GetFileName(file);
            string extension = Path.GetExtension(file).ToLower();

            // Skip files with .tif, .csv, or named "LGD_VNTT.ini"
            if (fileName == scriptName || extension == ".tif" || extension == ".csv" || fileName.Equals("LGD_VNTT.ini", StringComparison.OrdinalIgnoreCase))
            {
                continue; // Skip without logging
            }

            try
            {
                File.Copy(file, Path.Combine(targetDir, fileName), true);
                Console.WriteLine($"Copied: {file}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to copy {file}: {ex.Message}");
            }
        }

        // Ask user if they want to restart TrueTest.exe
        if (!string.IsNullOrEmpty(trueTestPath))
        {
            Console.WriteLine("Would you like to restart TrueTest.exe?");
            Console.WriteLine("[1] Yes");
            Console.WriteLine("[2] No");
            choice = Console.ReadKey().KeyChar;

            Console.WriteLine(); // Add new line for better formatting

            if (choice == '1')
            {
                Console.WriteLine("Restarting TrueTest.exe...");
                Process.Start(trueTestPath);
            }
            else
            {
                Console.WriteLine("Skipping restart.");
            }
        }

        Console.WriteLine("Press any key to exit.");
        Console.ReadKey(); // Final key press to exit
    }
}
