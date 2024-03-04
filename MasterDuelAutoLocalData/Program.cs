using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        string rootFolder = @"E:\Games\Steam\steamapps\common\Yu-Gi-Oh!  Master Duel\LocalData"; // Update this with the actual path to your LocalData folder
        string actualFolder = @"E:\Games\Steam\steamapps\common\Yu-Gi-Oh!  Master Duel\0000"; // Update this with the actual path to the actual "0000" folder
        CheckAndCreateSoftLink(rootFolder, actualFolder);
    }

    static void CheckAndCreateSoftLink(string rootFolder, string actualFolder)
    {
        string[] subfolders = Directory.GetDirectories(rootFolder);

        foreach (string folderPath in subfolders)
        {
            string folderName = Path.GetFileName(folderPath);

            if (Directory.Exists(folderPath))
            {
                string subfolder0000Path = Path.Combine(folderPath, "0000");

                // Check if the "0000" subfolder exists and is not a soft link
                if (Directory.Exists(subfolder0000Path) && !IsSymbolicLink(subfolder0000Path))
                {
                    Console.WriteLine($"Deleting {subfolder0000Path}");
                    Directory.Delete(subfolder0000Path, true); // Delete the folder and its contents
                }

                // Check if "0000" subfolder does not exist or it's not a soft link, then create a soft link
                if (!Directory.Exists(subfolder0000Path) || !IsSymbolicLink(subfolder0000Path))
                {
                    // Create a soft link to "0000" folder
                    CreateSymbolicLink(subfolder0000Path, actualFolder);
                    Console.WriteLine($"Created soft link for {folderName}");
                }
                else
                {
                    Console.WriteLine($"Folder {folderName} already contains '0000' subfolder as a soft link.");
                }
            }
            else
            {
                Console.WriteLine($"{folderName} is not a directory.");
            }
        }
    }


    static bool IsSymbolicLink(string path)
    {
        DirectoryInfo di = new DirectoryInfo(path);
        return di.Attributes.HasFlag(FileAttributes.ReparsePoint);
    }

    static void CreateSymbolicLink(string linkPath, string targetPath)
    {
        if (!File.Exists(linkPath))
        {
            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = $"/c mklink /D \"{linkPath}\" \"{targetPath}\"";
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            process.Start();
            process.WaitForExit();
        }
    }
}
