using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Security.Principal;
using System.Diagnostics;
using System.Runtime.InteropServices;

class SymlinkCreator
{
    // P/Invoke for CreateSymbolicLink (since .NET Framework doesn't have Directory.CreateSymbolicLink)
    [DllImport("kernel32.dll")]
    static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, int dwFlags);

    const int SYMBOLIC_LINK_FLAG_DIRECTORY = 1;

    static void Main()
    {
        // Check if running as administrator, if not, restart as admin
        if (!IsAdministrator())
        {
            Console.WriteLine("Restarting as administrator...");
            RestartAsAdmin();
            return;
        }

        // Configuration
        string localDataPath = "LocalData";
        string masterFolder = "0000";

        Console.WriteLine("========================================");
        Console.WriteLine(" Symlink Auto-Setup Script");
        Console.WriteLine("========================================");
        Console.WriteLine();

        // Get full path to master folder
        string scriptDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        string masterFolderFullPath = Path.Combine(scriptDir, localDataPath, masterFolder);

        // Check if master folder exists
        if (!Directory.Exists(masterFolderFullPath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ERROR: Master folder does not exist: " + masterFolderFullPath);
            Console.ResetColor();
            Console.WriteLine("Please create it first and populate with the actual 0000 content.");
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            return;
        }

        Console.WriteLine("Master folder: " + masterFolderFullPath);
        Console.WriteLine();

        // Find all directories in LocalData
        string localDataFullPath = Path.Combine(scriptDir, localDataPath);

        if (!Directory.Exists(localDataFullPath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ERROR: LocalData folder does not exist: " + localDataFullPath);
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            return;
        }

        var directories = Directory.GetDirectories(localDataFullPath);

        int count = 0;
        Regex hexPattern = new Regex("^[a-fA-F0-9]{7,8}$");

        foreach (var dir in directories)
        {
            string folderName = Path.GetFileName(dir);

            // Check if folder name is 7-8 hex characters
            if (hexPattern.IsMatch(folderName))
            {
                // Skip if this is the master folder itself
                if (folderName.Equals(masterFolder, StringComparison.OrdinalIgnoreCase))
                    continue;

                count++;
                string symlinkPath = Path.Combine(dir, masterFolder);

                // Check if 0000 exists
                if (Directory.Exists(symlinkPath) || File.Exists(symlinkPath))
                {
                    // Check if it's a symlink/junction
                    FileAttributes attributes = File.GetAttributes(symlinkPath);
                    if ((attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine("  [OK] " + folderName + "\\" + masterFolder + " - Already a symlink (skipped)");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("  [--] " + folderName + "\\" + masterFolder + " - Removing normal folder...");
                        Console.ResetColor();

                        Directory.Delete(symlinkPath, true);

                        if (CreateSymbolicLink(symlinkPath, masterFolderFullPath, SYMBOLIC_LINK_FLAG_DIRECTORY))
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("       Replaced with symlink");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("       ERROR: Failed to create symlink");
                            Console.ResetColor();
                        }
                    }
                }
                else
                {
                    if (CreateSymbolicLink(symlinkPath, masterFolderFullPath, SYMBOLIC_LINK_FLAG_DIRECTORY))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("  [OK] " + folderName + "\\" + masterFolder + " - Symlink created");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("  [ERROR] " + folderName + "\\" + masterFolder + " - Failed to create symlink");
                        Console.ResetColor();
                    }
                }
            }
        }

        Console.WriteLine();
        Console.WriteLine("========================================");
        Console.WriteLine(" Done! Processed " + count + " user folder(s)");
        Console.WriteLine("========================================");
        Console.WriteLine();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    static bool IsAdministrator()
    {
        try
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }

    static void RestartAsAdmin()
    {
        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = System.Reflection.Assembly.GetExecutingAssembly().Location,
                UseShellExecute = true,
                Verb = "runas"
            };
            Process.Start(startInfo);
        }
        catch
        {
            Console.WriteLine("Failed to restart as administrator.");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}