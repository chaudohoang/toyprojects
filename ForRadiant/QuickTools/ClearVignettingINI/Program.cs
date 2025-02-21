using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string password = $"lgd{DateTime.Now:HHmm}";
        Console.Write("Enter password: ");
        string inputPassword = ReadPassword();

        if (inputPassword != password)
        {
            Console.WriteLine("\nIncorrect password. Exiting.");
            return;
        }

        while (true)
        {
            string folderPath = Directory.GetCurrentDirectory(); // Use current folder
            string[] iniFiles = Directory.GetFiles(folderPath, "*.ini");

            Console.WriteLine("Select an option:");
            Console.WriteLine("1. Apply changes to all files");
            Console.WriteLine("2. Modify files individually");
            Console.Write("Enter your choice (1 or 2): ");
            string choice = Console.ReadKey().KeyChar.ToString().Trim();

            bool applyToAll = choice == "1";
            string globalValue = "";

            if (applyToAll)
            {
                while (true)
                {
                    Console.WriteLine("\nEnter new value for all files (0 or 1): ");
                    globalValue = Console.ReadKey().KeyChar.ToString().Trim();
                    if (globalValue == "0" || globalValue == "1")
                    {
                        break;
                    }
                    Console.WriteLine("Invalid input. Please enter 0 or 1.");
                }
            }

            foreach (string iniFile in iniFiles)
            {
                if (Regex.IsMatch(Path.GetFileName(iniFile), "DX", RegexOptions.IgnoreCase)) continue; // Skip files containing "DX" (case insensitive)

                var lines = File.ReadAllLines(iniFile).ToList();
                bool modified = false;

                for (int i = 0; i < lines.Count; i++)
                {
                    Match match = Regex.Match(lines[i], @"^STEP_3_VIGNETTING_CORRECTION\s*=\s*(\d+\.?\d*)");
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
                                if (newValue == "0" || newValue == "1")
                                {
                                    break;
                                }
                                Console.WriteLine("\nInvalid input. Please enter 0 or 1.");
                            }
                        }

                        lines[i] = Regex.Replace(lines[i], @"(?<=STEP_3_VIGNETTING_CORRECTION\s*=\s*)\d+\.?\d*", newValue);
                        modified = true;
                    }
                }

                if (modified)
                {
                    File.WriteAllLines(iniFile, lines);
                    Console.WriteLine($"File updated with New Value: {globalValue}");
                }
            }

            Console.Write("Restart setting again? (1 for Yes, 2 for No): ");
            string restartChoice = Console.ReadKey().KeyChar.ToString().Trim();
            Console.WriteLine();
            if (restartChoice != "1") break;
        }
    }

    static string ReadPassword()
    {
        string password = "";
        ConsoleKeyInfo key;
        do
        {
            key = Console.ReadKey(true);
            if (key.Key != ConsoleKey.Enter)
            {
                password += key.KeyChar;
                Console.Write("*");
            }
        } while (key.Key != ConsoleKey.Enter);
        Console.WriteLine();
        return password;
    }
}
