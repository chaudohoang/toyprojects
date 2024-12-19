using System;
using System.IO;
using System.Text.RegularExpressions;

class Program
{
    static void Main(string[] args)
    {
        // Prompt user to choose an option
        Console.WriteLine("Select an option:");
        Console.WriteLine("1: Replace 'STEP_3_VIGNETTING_CORRECTION = 1;' with 'STEP_3_VIGNETTING_CORRECTION = 0;'");
        Console.WriteLine("2: Replace 'STEP_3_VIGNETTING_CORRECTION = 0;' with 'STEP_3_VIGNETTING_CORRECTION = 1;'");
        string option = Console.ReadLine();

        string searchPattern, replaceText;

        // Determine the regex pattern and replacement text based on the option
        if (option == "1")
        {
            searchPattern = @"(STEP_3_VIGNETTING_CORRECTION\s*=\s*)1;";
            replaceText = "${1}0;";
        }
        else if (option == "2")
        {
            searchPattern = @"(STEP_3_VIGNETTING_CORRECTION\s*=\s*)0;";
            replaceText = "${1}1;";
        }
        else
        {
            Console.WriteLine("Invalid option selected. Exiting...");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            return;
        }

        // Specify the folder to start searching
        Console.WriteLine("Enter the path to the folder:");
        string folderPath = Console.ReadLine();

        // Check if the directory exists
        if (!Directory.Exists(folderPath))
        {
            Console.WriteLine("The specified folder does not exist.");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            return;
        }

        try
        {
            // Get all .ini files in the directory and subdirectories
            string[] iniFiles = Directory.GetFiles(folderPath, "*.ini", SearchOption.AllDirectories);

            foreach (string file in iniFiles)
            {
                // Read the file content
                string content = File.ReadAllText(file);

                // Use regex to replace the line while retaining original formatting
                if (Regex.IsMatch(content, searchPattern))
                {
                    content = Regex.Replace(content, searchPattern, replaceText);

                    // Write the modified content back to the file
                    File.WriteAllText(file, content);
                    Console.WriteLine($"Updated file: {file}");
                }
                else
                {
                    Console.WriteLine($"No match found in file: {file}");
                }
            }

            Console.WriteLine("Processing complete.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }

        // Wait for the user to press a key before exiting
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
