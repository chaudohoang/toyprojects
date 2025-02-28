using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

class ModifyIniFiles
{
    static void Main()
    {
        string folderPath = Directory.GetCurrentDirectory();
        string[] iniFiles = Directory.GetFiles(folderPath, "*.ini");
        string newValue = "1"; // Automatically set to 1

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
                    string oldValue = match.Groups[1].Value;
                    lines[i] = Regex.Replace(lines[i], "(?<=STEP_3_VIGNETTING_CORRECTION\\s*=\\s*)\\d+\\.?\\d*", newValue);
                    modified = true;
                    Console.WriteLine($"Updated: {Path.GetFileName(iniFile)} (STEP_3_VIGNETTING_CORRECTION {oldValue} -> {newValue})");
                }
            }

            if (modified)
            {
                File.WriteAllLines(iniFile, lines);
            }
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
