using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace ConsoleApp3
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enable / Disable saving synthetic, apply for Squence, Master Sequence, Master Calibration :");
            Console.WriteLine("1. Enable ");
            Console.WriteLine("2. Disable ");
            Console.Write("Enter choice : ");
            string input = Console.ReadLine();
            string[] paths =
            {
                "C:\\Radiant Vision Systems Data\\TrueTest\\Sequence",
                "C:\\Radiant Vision Systems Data\\TrueTest\\Sequence\\Master",
                "C:\\Radiant Vision Systems Data\\TrueTest\\Sequence\\Calibration"
             };
            //string[] paths =
            //{
            //    "C:\\Radiant Vision Systems Data\\TrueTest\\Sequence\\Test",
            //    "C:\\Radiant Vision Systems Data\\TrueTest\\Sequence\\Test2"
            // };

            if (input == "1")
            {
                foreach (string path in paths) 
                {
                    string backup = path + "\\backup_"+DateTime.Now.ToString("yyyyMMddhhmmss");
                    Backup(path, backup);
                    foreach (var seq in Directory.EnumerateFiles(path, "*.seqxc"))
                    {
                        string text = File.ReadAllText(seq);
                        text = text.Replace("<SaveToDatabase>false", "<SaveToDatabase>true");
                        File.WriteAllText(seq, text);
                        Console.WriteLine("Modifed " +seq);
                    }
                }
                Console.Write("Done, reload sequence ! ");
            }
            else if (input == "2")
            {
                foreach (string path in paths)
                {
                    string backup = path + "\\backup_" + DateTime.Now.ToString("yyyyMMddhhmmss");
                    Backup(path, backup);
                    foreach (var seq in Directory.EnumerateFiles(path, "*.seqxc"))
                    {
                        string text = File.ReadAllText(seq);
                        text = text.Replace("<SaveToDatabase>true", "<SaveToDatabase>false");
                        File.WriteAllText(seq, text);
                        Console.WriteLine("Modifed " +seq);
                    }
                }
                Console.Write("Done, reload sequence ! ");                
            }
            else return;
            Console.ReadKey();
        }

        public static void Backup(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

            CopyFiles(diSource, diTarget);
        }

        public static void CopyFiles(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                Console.WriteLine(@"Backed up {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }
        }


    }
}
