using System;
using System.Diagnostics;
using System.IO;
using System.Xml;

class Program
{
    static void Main()
    {
        Console.WriteLine("Change all sequence files Mask/Feature of Interest to None.");
        Console.WriteLine("This will force shutdown TrueTest before running.");
        Console.WriteLine("Press Enter to run the tasks...");
        Console.ReadLine();
        string trueTestExePath = @"C:\Program Files\Radiant Vision Systems\TrueTest 1.8\TrueTest.exe";
        string appSettingsPath = @"C:\Radiant Vision Systems Data\TrueTest\AppData\1.8\app.settings";
        string sequenceFolderPath = @"C:\Radiant Vision Systems Data\TrueTest\Sequence";

        // Task 1: Check and shut down TrueTest.exe if running
        ShutDownTrueTest(trueTestExePath);

        // Task 2: Modify <DisplayAnalysisFOIEnabled> in app.settings
        ModifyDisplayAnalysisFOIEnabled(appSettingsPath);

        // Task 3: Modify <XMLMASKFOISETID> in each .seqxc file recursively
        ModifyXMLMASKFOISETIDInSequenceFiles(sequenceFolderPath);

        Console.WriteLine("Tasks completed successfully.");

        // Keep the console window open until the user presses Enter
        Console.ReadLine();
    }

    static void ShutDownTrueTest(string exePath)
    {
        try
        {
            Process[] processes = Process.GetProcessesByName("TrueTest");

            foreach (Process process in processes)
            {
                if (process.MainModule.FileName.Equals(exePath, StringComparison.OrdinalIgnoreCase))
                {
                    process.CloseMainWindow();
                    process.WaitForExit(5000); // Wait up to 5 seconds for the application to exit
                    if (!process.HasExited)
                    {
                        process.Kill(); // Forcefully terminate the process if it hasn't exited
                    }
                    Console.WriteLine("TrueTest.exe was running and has been shut down.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error shutting down TrueTest.exe: {ex.Message}");
        }
    }

    static void ModifyDisplayAnalysisFOIEnabled(string filePath)
    {
        try
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);

            XmlNode node = doc.SelectSingleNode("/Settings/DisplayAnalysisFOIEnabled");

            if (node != null)
            {
                if (node.InnerText.ToLower() == "false")
                {
                    node.InnerText = "true";
                    doc.Save(filePath);
                    Console.WriteLine("DisplayAnalysisFOIEnabled updated to true in app.settings");
                }
                else
                {
                    Console.WriteLine("DisplayAnalysisFOIEnabled is already true in app.settings");
                }
            }
            else
            {
                Console.WriteLine("Element <DisplayAnalysisFOIEnabled> not found in app.settings");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error modifying app.settings: {ex.Message}");
        }
    }

    static void ModifyXMLMASKFOISETIDInSequenceFiles(string folderPath)
    {
        try
        {
            string[] sequenceFiles = Directory.GetFiles(folderPath, "*.seqxc", SearchOption.AllDirectories);

            foreach (string file in sequenceFiles)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(file);

                XmlNodeList nodes = doc.SelectNodes("/Sequence/Items/SequenceItem/Analysis/XMLMASKFOISETID");

                if (nodes.Count > 0)
                {
                    foreach (XmlNode node in nodes)
                    {
                        node.InnerText = "0";
                    }

                    // Set XmlWriterSettings to control formatting
                    XmlWriterSettings settings = new XmlWriterSettings
                    {
                        Indent = true,
                        IndentChars = "\t",
                        NewLineHandling = NewLineHandling.Entitize
                    };

                    // Save the document with the specified settings
                    using (XmlWriter writer = XmlWriter.Create(file, settings))
                    {
                        doc.Save(writer);
                    }

                    Console.WriteLine($"Mask/Feature of Interest updated to None in {Path.GetFullPath(file)}");
                    //Console.WriteLine($"XMLMASKFOISETID updated to 0 in {Path.GetFullPath(file)}");
                }
                else
                {
                    Console.WriteLine($"Mask/Feature of Interest not found in {Path.GetFullPath(file)}");
                    //Console.WriteLine($"Element <XMLMASKFOISETID> not found in {Path.GetFullPath(file)}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error modifying sequence files: {ex.Message}");
        }
    }
}
