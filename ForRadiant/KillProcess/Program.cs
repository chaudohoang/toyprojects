using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillProcess
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (var process in Process.GetProcessesByName("wfica32"))
            {
                process.Kill();
            }
        }
    }
}
