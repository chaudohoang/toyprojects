using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StartProcess
{
    class Program
    {
        static void Main(string[] args)
        {
            string apppath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string appdir = Path.GetDirectoryName(apppath);
            string startlistdir = appdir + "\\StartList";

            List<string> startlist = GetFilesRecursive(startlistdir);
            foreach (string item in startlist)
            {
                string item2 = Path.GetFileName(item).Replace(".exe", "");
                Process[] processes = Process.GetProcessesByName(item2);
                if (processes.Length < 1)
                {
                    try
                    {
                        Process.Start(item);
                    }
                    catch (Exception ex)
                    {

                        MessageBox.Show(ex.Message);
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
            }

            // Return the list
            return result;
        }
    }
}
