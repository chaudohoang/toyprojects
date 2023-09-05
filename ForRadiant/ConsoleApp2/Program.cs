using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var processName = "truetest.exe";

            var connectoptions = new ConnectionOptions();
            connectoptions.Username = "leo.jeon";
            connectoptions.Password = "3117";

            string ipAddress = "192.168.111.119";
            ManagementScope scope = new ManagementScope(@"\\" + ipAddress + @"\root\cimv2", connectoptions);

            // WMI query
            var query = new SelectQuery("select * from Win32_process where name = '" + processName + "'");

            using (var searcher = new ManagementObjectSearcher(scope, query))
            {
                foreach (ManagementObject process in searcher.Get()) // this is the fixed line
                {
                    process.InvokeMethod("Terminate", null);
                }
            }
            Console.ReadLine();
        }
    }
}
