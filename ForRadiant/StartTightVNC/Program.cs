using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartTightVNC
{
    class Program
    {
        public static void Main(string[] args)
        {
            if (File.Exists(@"D:\Program\RVS\Tools\Installer\TightVNCDProgram.exe"))
            {
                ExecuteAsAdmin(@"D:\Program\RVS\Tools\Installer\TightVNCDProgram.exe");
            }
            else if (File.Exists(@"C:\Program\RVS\Tools\Installer\TightVNCCProgram.exe"))
            {
                ExecuteAsAdmin(@"C:\Program\RVS\Tools\Installer\TightVNCCProgram.exe");
            }
        }

        public static void ExecuteAsAdmin(string fileName)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = fileName;
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.Verb = "runas";
            proc.Start();
        }

    }
}
