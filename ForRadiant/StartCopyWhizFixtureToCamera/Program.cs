using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartCopyWhizFixtureToCamera
{
    class Program
    {
        static void Main(string[] args)
        {
            Process process1 = new Process();
            process1.StartInfo.FileName = @"\\127.0.0.1\Program\RVS\Tools\CopyWhiz\CopyWhizCopy.exe";
            process1.StartInfo.Arguments = @"\\127.0.0.1\Program\RVS\Tools\CopyWhiz\FromFixture_CopyToDooone1234.czml";
            if (File.Exists(process1.StartInfo.FileName) && File.Exists(process1.StartInfo.Arguments) && Directory.Exists(@"\\192.168.1.11\Program") && Directory.Exists(@"\\192.168.2.12\Program") && Directory.Exists(@"\\192.168.3.13\Program") && Directory.Exists(@"\\192.168.4.14\Program"))
            {
                process1.Start();
            }

            Process process2 = new Process();
            process2.StartInfo.FileName = @"\\127.0.0.1\Program\RVS\Tools\CopyWhiz\CopyWhizCopy.exe";
            process2.StartInfo.Arguments = @"\\127.0.0.1\Program\RVS\Tools\CopyWhiz\FromFixture_CopyToGooil1234.czml";
            if ( File.Exists(process2.StartInfo.FileName) && File.Exists(process2.StartInfo.Arguments) && Directory.Exists(@"\\192.168.0.50\Program") && Directory.Exists(@"\\192.168.0.51\Program") && Directory.Exists(@"\\192.168.0.52\Program") && Directory.Exists(@"\\192.168.0.53\Program"))
            {
                process2.Start();
            }

        }
    }
}
