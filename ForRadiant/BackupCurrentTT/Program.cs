using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackupCurrentTT
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (var process in Process.GetProcessesByName("TrueTest"))
            {
                process.Kill();
            }
            var versionInfo = FileVersionInfo.GetVersionInfo(@"C:\Program Files\Radiant Vision Systems\TrueTest 1.8\TrueTest.exe");
            string version = versionInfo.ProductVersion;
            string sourcefolder = @"C:\Program Files\Radiant Vision Systems\TrueTest 1.8";
            string destfolder = @"C:\Program Files\Radiant Vision Systems\TrueTest " + version;
            CopyFolder(sourcefolder, destfolder);
            CreateShortcut("TrueTest " + version, Environment.GetFolderPath(Environment.SpecialFolder.Desktop), Path.Combine(destfolder, "TrueTest.exe"));
        }

        public static void CopyFolder(string sourceFolder, string destFolder)
        {
            if (!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);
            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                string dest = Path.Combine(destFolder, name);
                System.IO.File.Copy(file, dest, true);
            }
            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(destFolder, name);
                CopyFolder(folder, dest);
            }
        }

        public static void CreateShortcut(string shortcutName, string shortcutPath, string targetFileLocation)
        {
            string shortcutLocation = System.IO.Path.Combine(shortcutPath, shortcutName + ".lnk");
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);
            shortcut.TargetPath = targetFileLocation;
            shortcut.Save();  
        }


    }
}
