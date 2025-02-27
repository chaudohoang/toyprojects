using System;
using System.IO;
using System.Globalization;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using Microsoft.VisualBasic;

namespace CodeProviders
{
    class CompileSample
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (File.Exists(args[0]))
                {
                    CompileExecutable(args[0]);
                }
                else
                {
                    Console.WriteLine($"Input source file not found - {args[0]}");
                }
            }
            else
            {
                Console.WriteLine("Input source file not specified on command line!");
            }

            // Keep console window open
            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }

        public static bool CompileExecutable(string sourceName)
        {
            FileInfo sourceFile = new FileInfo(sourceName);
            CodeDomProvider provider = null;
            bool compileOk = false;
            string filePostfix = "";

            // Ensure the "Compiled" directory exists
            string compiledFolder = Path.Combine(Environment.CurrentDirectory, "Compiled");
            Directory.CreateDirectory(compiledFolder);

            // Select the code provider based on the input file extension.
            switch (sourceFile.Extension.ToUpper(CultureInfo.InvariantCulture))
            {
                case ".CS":
                    provider = CodeDomProvider.CreateProvider("CSharp");
                    filePostfix = "_cs";
                    break;
                case ".VB":
                    provider = CodeDomProvider.CreateProvider("VisualBasic");
                    filePostfix = "_vb";
                    break;
                default:
                    Console.WriteLine("Source file must have a .cs or .vb extension");
                    return false;
            }

            // Format the executable file name inside the "Compiled" folder
            string exeName = Path.Combine(compiledFolder,
                $"{Path.GetFileNameWithoutExtension(sourceFile.Name)}{filePostfix}_{DateTime.Now:yyyyMMddHHmmss}.exe");

            CompilerParameters cp = new CompilerParameters
            {
                GenerateExecutable = true,
                OutputAssembly = exeName,
                GenerateInMemory = false,
                TreatWarningsAsErrors = false
            };

            // Compile the source file
            CompilerResults cr = provider.CompileAssemblyFromFile(cp, sourceName);

            if (cr.Errors.Count > 0)
            {
                Console.WriteLine($"Errors building {sourceName} into {cr.PathToAssembly}:");
                foreach (CompilerError ce in cr.Errors)
                {
                    Console.WriteLine($"  {ce}");
                }
                Console.WriteLine($"Total Errors: {cr.Errors.Count}");
            }
            else
            {
                Console.WriteLine($"Source {sourceName} built into {cr.PathToAssembly} successfully.");
                compileOk = true;
            }

            return compileOk;
        }
    }
}
