using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace RenameVNTT
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string pathInput,station, channel;
            string path = Directory.GetCurrentDirectory();
            Console.WriteLine("VNTTMAP Rename ... ");
            string appdata = "C:\\Radiant Vision Systems Data\\TrueTest\\AppData";
            Console.Write("Path : ");
            pathInput = Console.ReadLine();
            List<string> files = new List<string>();
            if (pathInput == "appdata")
            {
                path = appdata;
            }
            else if (pathInput == "")
            {
            }
            else
            {
                path = pathInput;
            }
            foreach (var item in Directory.GetFiles(path))
            {
                if (Path.GetExtension(item)==".csv" && Path.GetFileNameWithoutExtension(item).ToUpper().Contains("VNTTMAP_GEN"))
                {
                    files.Add(item);
                }
            }
            
            Console.Write("Station : ");
            station = Console.ReadLine();
            Console.Write("Channel : ");
            channel = Console.ReadLine();

            foreach (var item in files)
            {
                string newName = "";
                int resultIndex = Path.GetFileNameWithoutExtension(item).IndexOf("VNTTMAP_GEN") +11;
                if (resultIndex != -1)
                {
                    newName = Path.GetFileNameWithoutExtension(item).Substring(0, resultIndex);
                }
                string newPath = Path.Combine(path, newName + "_T" + station + "_CH" + channel + Path.GetExtension(item));
                File.Move(item, newPath);
            }

            for (int a = 3; a >= 0; a--)
            {
                Console.Write("\rFinished, closing in {0} !", a);
                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}
