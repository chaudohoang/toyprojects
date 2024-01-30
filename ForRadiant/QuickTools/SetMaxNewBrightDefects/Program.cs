using System;
using System.IO;
using System.Xml;

class Program
{
    static void Main()
    {
        Console.WriteLine("Enter the new value for MaxNewBrightDefects:");
        string newValue = Console.ReadLine();

        string directoryPath = @"C:\Radiant Vision Systems Data\TrueTest\Sequence";
        UpdateXmlValuesRecursively(directoryPath, newValue);

        Console.WriteLine("XML elements updated in all .seqxc files successfully.");

        Console.WriteLine("Do you want to delete C:\\Radiant Vision Systems Data\\TrueTest\\AppData\\PIXEL_LOG.csv? (Y/N)");
        string deleteConfirmation = Console.ReadLine().ToUpper();

        if (deleteConfirmation == "Y")
        {
            string filePathToDelete = @"C:\Radiant Vision Systems Data\TrueTest\AppData\PIXEL_LOG.csv";
            DeleteFile(filePathToDelete);
            Console.WriteLine("File deleted successfully.");
        }
        else
        {
            Console.WriteLine("File deletion skipped.");
        }
    }

    static void UpdateXmlValuesRecursively(string directoryPath, string newValue)
    {
        // Get all .seqxc files in the specified directory and its subdirectories
        string[] files = Directory.GetFiles(directoryPath, "*.seqxc", SearchOption.AllDirectories);

        foreach (string file in files)
        {
            // Load the XML document
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(file);

            // Find all elements with the specified name
            XmlNodeList elements = xmlDoc.GetElementsByTagName("MaxNewBrightDefects");

            // Update the values of the found elements
            foreach (XmlNode element in elements)
            {
                // Set the new value entered by the user
                element.InnerText = newValue;
            }

            // Save the modified XML document with single-line empty elements
            SaveXmlDocument(xmlDoc, file);
        }
    }

    static void SaveXmlDocument(XmlDocument xmlDoc, string filePath)
    {
        // Save the modified XML document with single-line empty elements
        XmlWriterSettings settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "\t",
            NewLineHandling = NewLineHandling.None
        };

        using (XmlWriter writer = XmlWriter.Create(filePath, settings))
        {
            xmlDoc.Save(writer);
        }
    }

    static void DeleteFile(string filePath)
    {
        // Check if the file exists before attempting to delete
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        else
        {
            Console.WriteLine("File not found. Deletion skipped.");
        }
    }
}
