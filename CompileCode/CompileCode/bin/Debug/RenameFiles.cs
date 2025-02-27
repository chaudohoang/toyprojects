// Rename all files in the current directory (recursively)
using System;
using System.IO;

namespace Program
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Rename all files in the current directory (recursively) ");
            Console.Write("Enter the string to be replaced: ");
            string oldString = Console.ReadLine();

            Console.Write("Enter the new string: ");
            string newString = Console.ReadLine();

            if (string.IsNullOrEmpty(oldString) || string.IsNullOrEmpty(newString))
            {
                Console.WriteLine("Invalid input. Exiting...");
                return;
            }

            var rootDir = Directory.GetCurrentDirectory();
            var fileNames = Directory.EnumerateFiles(rootDir, "*", SearchOption.AllDirectories);
            
            foreach (string path in fileNames)
            {
                var dir = Path.GetDirectoryName(path);
                var fileName = Path.GetFileNameWithoutExtension(path).Replace(oldString, newString);
                var ext = Path.GetExtension(path);
                var newPath = Path.Combine(dir, fileName + ext);
                
                if (path != newPath)
                {
                    File.Move(path, newPath);
                    Console.WriteLine(string.Format("Renamed: {0} -> {1}", path, newPath));
                }
            }

            Console.WriteLine("Operation completed.");
        }
    }
}
