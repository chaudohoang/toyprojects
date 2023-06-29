using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoveEZAEAddons
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Remove all EZAE addons ? y/n ");
            char del;
            del = (char)Console.ReadKey().KeyChar;
            if (del == 'y')
            {
                if (Directory.Exists("Apps"))
                {
                    Directory.Delete("Apps", true);
                    Console.WriteLine();
                    Console.WriteLine("Apps Deleted ...");
                }

                if (Directory.Exists("Big Installers"))
                {
                    Directory.Delete("Big Installers", true);
                    Console.WriteLine();
                    Console.WriteLine("Big Installers Deleted ...");
                }

                if (Directory.Exists("TrueTest Installers"))
                {
                    Directory.Delete("TrueTest Installers", true);
                    Console.WriteLine();
                    Console.WriteLine("TrueTest Installers Deleted ...");
                }

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

        }
    }
}
