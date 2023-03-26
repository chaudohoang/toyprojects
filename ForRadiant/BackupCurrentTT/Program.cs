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
            string sourcefolder = @"C:\Program Files\Radiant Vision Systems\TrueTest 1.8";
            if (!Directory.Exists(sourcefolder))
            {
                Console.WriteLine("C:\\Program Files\\Radiant Vision Systems\\TrueTest 1.8 Not existed ...");
                Console.WriteLine();
                Console.WriteLine("Press any key to close ...");
                Console.ReadKey();
                return;
            }
            var versionInfo = FileVersionInfo.GetVersionInfo(@"C:\Program Files\Radiant Vision Systems\TrueTest 1.8\TrueTest.exe");
            string version = versionInfo.FileVersion.Replace(versionInfo.FilePrivatePart.ToString(),"");
            version = version.Remove(version.Length - 1);            
            string destfolder = @"C:\Program Files\Radiant Vision Systems\TrueTest " + version;
            if (!Directory.Exists(destfolder))
            {
                Console.WriteLine("Backing up ...");
                CopyFolder(sourcefolder, destfolder);               
            }
            Console.WriteLine("Backed up to : " + destfolder);
            Console.WriteLine("Delete TrueTest 1.8 folder ?, y/n");
            char del;
            del = (char)Console.ReadKey().KeyChar;
            if (del=='y')
            {
                foreach (var process in Process.GetProcessesByName("TrueTest"))
                {
                    process.Kill();
                }
                Directory.Delete("C:\\Program Files\\Radiant Vision Systems\\TrueTest 1.8",true);
                Console.WriteLine();
                Console.WriteLine("C:\\Program Files\\Radiant Vision Systems\\TrueTest 1.8 Deleted ...");
                Console.WriteLine();
                Console.WriteLine("Finished, press any key to close ...");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Finished, press any key to close ...");
                Console.ReadKey();
            }
            //CreateShortcut("TrueTest " + version, Environment.GetFolderPath(Environment.SpecialFolder.Desktop), Path.Combine(destfolder, "TrueTest.exe"));
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
