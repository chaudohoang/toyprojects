using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Shell32;

namespace StartProcess
{
	class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            string apppath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string appdir = Path.GetDirectoryName(apppath);
            string startlistdir = appdir + "\\StartList";

            List<string> startlist = GetFilesRecursive(startlistdir);
            foreach (string item in startlist)
            {
                string ext = Path.GetExtension(item);
                string process;

                process = Path.GetFileName(item).Replace(ext, "");

                Process[] processes = Process.GetProcessesByName(process);
                if (processes.Length < 1)
                {
                    try
                    {
                        Process.Start(item);
                    }
                    catch (Exception ex)
                    {

                    }
                    
                }    
                    
            }
        }

        public static List<string> GetFilesRecursive(string initial)
        {
            // This list stores the results.
            List<string> result = new List<string>();

            // This stack stores the directories to process.
            Stack<string> stack = new Stack<string>();

            // Add the initial directory
            stack.Push(initial);

            // Continue processing for each stacked directory
            while ((stack.Count > 0))
            {
                // Get top directory string
                string dir = stack.Pop();
                try
                {
                    // Add all immediate file paths
                    result.AddRange(Directory.GetFiles(dir, "*.exe"));
                    foreach (var directoryName in Directory.GetDirectories(dir))
                        stack.Push(directoryName);
                }
                catch (Exception ex)
                {
                }
                try
                {
                    // Add all immediate file paths
                    result.AddRange(Directory.GetFiles(dir, "*.lnk"));
                    foreach (var directoryName in Directory.GetDirectories(dir))
                        stack.Push(directoryName);
                }
                catch (Exception ex)
                {
                }
            }

            // Return the list
            return result;
        }

        public static string GetShortcutTargetFile(string shortcutFilename)
        {
            string pathOnly = System.IO.Path.GetDirectoryName(shortcutFilename);
            string filenameOnly = System.IO.Path.GetFileName(shortcutFilename);

            Shell shell = new Shell();
            Folder folder = shell.NameSpace(pathOnly);
            FolderItem folderItem = folder.ParseName(filenameOnly);
            if (folderItem != null)
            {
                Shell32.ShellLinkObject link = (Shell32.ShellLinkObject)folderItem.GetLink;
                return link.Path;
            }

            return string.Empty;
        }

    }
}
