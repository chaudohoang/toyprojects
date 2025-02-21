using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        while (true)
        {
            Console.WriteLine("Select a target directory:");
            Console.WriteLine("[1] C:\\Program Files\\Radiant Vision Systems\\TrueTest 1.8\\POCB4.1Net");
            Console.WriteLine("[2] C:\\Program Files\\Radiant Vision Systems\\TrueTest 1.8 debug Mobile\\POCB4.1Net");
            char mainChoice = Console.ReadKey().KeyChar;
            Console.WriteLine();

            if (mainChoice == '0')
            {
                if (AuthenticateUser())
                    ModifyIniFiles();
            }
            else if (mainChoice == '1' || mainChoice == '2')
            {
                ManageTrueTest(mainChoice);
                Environment.Exit(0);
            }
            else
            {
                Console.WriteLine("Invalid choice. Try again.");
            }
        }
    }

    static void ModifyIniFiles()
    {
        while (true)
        {
            string folderPath = Directory.GetCurrentDirectory();
            string[] iniFiles = Directory.GetFiles(folderPath, "*.ini");

            Console.WriteLine("Select an option:");
            Console.WriteLine("1. Apply changes to all files");
            Console.WriteLine("2. Modify files individually");
            Console.Write("Enter your choice (1 or 2): ");
            char choice = Console.ReadKey().KeyChar;
            Console.WriteLine();

            bool applyToAll = choice == '1';
            string globalValue = "";

            if (applyToAll)
            {
                while (true)
                {
                    Console.Write("Enter new value for all files (0 or 1): ");
                    globalValue = Console.ReadKey().KeyChar.ToString().Trim();
                    Console.WriteLine();
                    if (globalValue == "0" || globalValue == "1") break;
                    Console.WriteLine("Invalid input. Please enter 0 or 1.");
                }
            }

            foreach (string iniFile in iniFiles)
            {
                if (Regex.IsMatch(Path.GetFileName(iniFile), "DX", RegexOptions.IgnoreCase)) continue;

                var lines = File.ReadAllLines(iniFile).ToList();
                bool modified = false;

                for (int i = 0; i < lines.Count; i++)
                {
                    Match match = Regex.Match(lines[i], "^STEP_3_VIGNETTING_CORRECTION\\s*=\\s*(\\d+\\.?\\d*)");
                    if (match.Success)
                    {
                        Console.Write($"\nFile: {Path.GetFileName(iniFile)}\nCurrent Value: {match.Groups[1].Value} ");

                        string newValue = globalValue;
                        if (!applyToAll)
                        {
                            while (true)
                            {
                                Console.Write("Enter new value (0 or 1): ");
                                newValue = Console.ReadKey().KeyChar.ToString().Trim();
                                Console.WriteLine();
                                if (newValue == "0" || newValue == "1") break;
                                Console.WriteLine("Invalid input. Please enter 0 or 1.");
                            }
                        }

                        lines[i] = Regex.Replace(lines[i], "(?<=STEP_3_VIGNETTING_CORRECTION\\s*=\\s*)\\d+\\.?\\d*", newValue);
                        modified = true;
                    }
                }

                if (modified)
                {
                    File.WriteAllLines(iniFile, lines);
                    Console.WriteLine($"File updated with New Value: {globalValue}");
                }
            }

            Console.WriteLine("Select an option:");
            Console.WriteLine("1. Modify INI files again");
            Console.WriteLine("2. Return to the main menu");
            char nextAction = Console.ReadKey().KeyChar;
            Console.WriteLine();

            if (nextAction == '2') break;
        }
    }

    static void ManageTrueTest(char choice)
    {
        string trueTestPath = null;
        bool wasRunning = false;
        var process = Process.GetProcessesByName("TrueTest").FirstOrDefault();

        if (process != null)
        {
            trueTestPath = process.MainModule.FileName;
            Console.WriteLine("TrueTest.exe is running. Terminating process...");
            process.Kill();
            Console.WriteLine("Process terminated.");
            wasRunning = true;
        }
        else
        {
            Console.WriteLine("TrueTest.exe is not running.");
        }

        string targetDir = (choice == '1')
            ? @"C:\\Program Files\\Radiant Vision Systems\\TrueTest 1.8\\POCB4.1Net"
            : @"C:\\Program Files\\Radiant Vision Systems\\TrueTest 1.8 debug Mobile\\POCB4.1Net";

        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        string scriptName = Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        var files = Directory.GetFiles(Directory.GetCurrentDirectory());

        foreach (var file in files)
        {
            string fileName = Path.GetFileName(file);
            string extension = Path.GetExtension(file).ToLower();
            if (fileName == scriptName || extension == ".tif" || extension == ".csv" || fileName.Equals("LGD_VNTT.ini", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            bool copySuccessful = false;
            int retryCount = 3; // Retry limit
            int retryDelay = 500; // 500 milliseconds delay between retries

            for (int attempt = 1; attempt <= retryCount; attempt++)
            {
                try
                {
                    File.Copy(file, Path.Combine(targetDir, fileName), true);
                    Console.WriteLine($"Copied: {file}");
                    copySuccessful = true;
                    break; // Exit the loop if copy is successful
                }
                catch (IOException ex) when (attempt < retryCount)
                {
                    // If the file is locked, retry
                    Console.WriteLine($"Failed to copy {file} (Attempt {attempt}/{retryCount}): {ex.Message}. Retrying...");
                    System.Threading.Thread.Sleep(retryDelay); // Wait before retrying
                }
                catch (Exception ex)
                {
                    // If an unexpected error occurs, log it and stop retrying
                    Console.WriteLine($"Failed to copy {file}: {ex.Message}");
                    break;
                }
            }

            // If after retries the copy failed, log the failure
            if (!copySuccessful)
            {
                Console.WriteLine($"Failed to copy {file} after {retryCount} attempts.");
            }
        }

        if (wasRunning && trueTestPath != null)
        {
            PromptExitOrRestart(trueTestPath);
        }
        else
        {
            // Removed Environment.Exit(0) to allow the program to wait
        }

        // Wait for user input before exiting
        Console.WriteLine("Press any key to exit.");
        Console.ReadKey();
    }



    static void PromptExitOrRestart(string trueTestPath)
    {
        Console.WriteLine("Manage TrueTest completed. Select an option:");
        Console.WriteLine("1. Restart TrueTest");
        Console.WriteLine("2. Exit");
        char nextAction = Console.ReadKey().KeyChar;
        Console.WriteLine();

        if (nextAction == '1')
        {
            Console.WriteLine("Restarting TrueTest...");
            Process.Start(trueTestPath);
        }
        else
        {
            Console.WriteLine("Exiting program.");
            Environment.Exit(0);
        }
    }

    static bool AuthenticateUser()
    {
        string password = $"{DateTime.Now:HHmm}";
        Console.Write("Enter password: ");
        string inputPassword = ReadPassword();

        if (inputPassword != password)
        {
            Console.WriteLine("\nIncorrect password. Returning to menu.");
            return false;
        }
        return true;
    }

    static string ReadPassword()
    {
        string password = "";
        ConsoleKeyInfo key;
        do
        {
            key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password = password.Substring(0, password.Length - 1);
                Console.Write("\b \b");
            }
            else if (!char.IsControl(key.KeyChar))
            {
                password += key.KeyChar;
                Console.Write("*");
            }
        } while (key.Key != ConsoleKey.Enter);
        Console.WriteLine();
        return password;
    }
}
