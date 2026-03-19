using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

class ModifyIniFiles
{
    static void Main()
    {
        Console.WriteLine("=== Set Vignetting Off ===");
        Console.WriteLine();

        string folderPath = Directory.GetCurrentDirectory();
        string[] iniFiles = Directory.GetFiles(folderPath, "*.ini");
        string newValue = "0";

        string[] dxFiles = iniFiles
            .Where(f => Regex.IsMatch(Path.GetFileName(f), "DX", RegexOptions.IgnoreCase))
            .ToArray();

        bool applyDX = false;
        if (dxFiles.Length > 0)
        {
            Console.WriteLine("DX ini file(s) detected. What would you like to do?");
            Console.WriteLine("  1. Apply for DX ini");
            Console.WriteLine("  2. Don't apply for DX ini");
            Console.Write("Enter choice (1/2): ");

            string choice = Console.ReadKey(true).KeyChar.ToString();
            Console.WriteLine($"{choice} chosen");

            if (choice == "1")
                applyDX = true;
            else
                Console.WriteLine("DX files will be skipped.");
        }

        foreach (string iniFile in iniFiles)
        {
            bool isDX = Regex.IsMatch(Path.GetFileName(iniFile), "DX", RegexOptions.IgnoreCase);
            if (isDX && !applyDX)
            {
                foreach (var line in File.ReadAllLines(iniFile))
                {
                    Match m = Regex.Match(line, @"^STEP_3_VIGNETTING_CORRECTION\s*=\s*(\d+\.?\d*)");
                    if (m.Success)
                        Console.WriteLine($"Unchanged: {Path.GetFileName(iniFile)} (STEP_3_VIGNETTING_CORRECTION : {m.Groups[1].Value})");
                }
                continue;
            }

            var lines = File.ReadAllLines(iniFile).ToList();
            bool modified = false;

            for (int i = 0; i < lines.Count; i++)
            {
                Match match = Regex.Match(lines[i], @"^STEP_3_VIGNETTING_CORRECTION\s*=\s*(\d+\.?\d*)");
                if (match.Success)
                {
                    string oldValue = match.Groups[1].Value;
                    lines[i] = Regex.Replace(lines[i], @"(?<=STEP_3_VIGNETTING_CORRECTION\s*=\s*)\d+\.?\d*", newValue);
                    modified = true;
                    Console.WriteLine($"Updated: {Path.GetFileName(iniFile)} (STEP_3_VIGNETTING_CORRECTION {oldValue} -> {newValue})");
                }
            }

            if (modified)
                File.WriteAllLines(iniFile, lines);
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}