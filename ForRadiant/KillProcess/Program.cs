using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace KillProcess
{
    class Program
    {
        static void Main(string[] args)
        {
            string apppath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string appdir = Path.GetDirectoryName(apppath);

            string killlistpath = Path.Combine(appdir, "KillList.txt");

            string[] processes = File.ReadAllLines(killlistpath);
            foreach (var process in processes)
            {
                try
                {
                    EndTask(process);
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message);
                }
                
            }
        }

        public static void EndTask(string taskname)
        {
            foreach (Process process in Process.GetProcessesByName(taskname))
            {
                process.Kill();
            }
        }

    }
}
