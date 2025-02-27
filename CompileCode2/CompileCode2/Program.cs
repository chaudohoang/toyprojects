using System;
using System.IO;
using System.Linq;
using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;

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
            bool compileOk = false;
            string filePostfix = "";

            // Ensure the "Compiled" directory exists
            string compiledFolder = Path.Combine(Environment.CurrentDirectory, "Compiled");
            Directory.CreateDirectory(compiledFolder);

            // Read the source file content
            string sourceCode = File.ReadAllText(sourceName);

            // Determine the language provider
            Compilation compilation = null;
            switch (sourceFile.Extension.ToUpper(CultureInfo.InvariantCulture))
            {
                case ".CS":
                    compilation = CreateCSharpCompilation(sourceFile, sourceCode);
                    filePostfix = "_cs";
                    break;
                case ".VB":
                    compilation = CreateVBCompilation(sourceFile, sourceCode);
                    filePostfix = "_vb";
                    break;
                default:
                    Console.WriteLine("Source file must have a .cs or .vb extension");
                    return false;
            }

            if (compilation == null)
            {
                Console.WriteLine("Compilation object could not be created.");
                return false;
            }

            // Format the executable file name inside the "Compiled" folder
            string exeName = Path.Combine(compiledFolder,
                $"{Path.GetFileNameWithoutExtension(sourceFile.Name)}{filePostfix}_{DateTime.Now:yyyyMMddHHmmss}.exe");

            // Emit (compile) the executable
            var result = compilation.Emit(exeName);

            if (!result.Success)
            {
                Console.WriteLine($"Errors compiling {sourceName}:");
                foreach (var diagnostic in result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))
                {
                    Console.WriteLine(diagnostic);
                }
                Console.WriteLine($"Total Errors: {result.Diagnostics.Count(d => d.Severity == DiagnosticSeverity.Error)}");
            }
            else
            {
                Console.WriteLine($"Compilation successful: {exeName}");
                compileOk = true;
            }

            return compileOk;
        }

        private static CSharpCompilation CreateCSharpCompilation(FileInfo sourceFile, string sourceCode)
        {
            return CSharpCompilation.Create(
                Path.GetFileNameWithoutExtension(sourceFile.Name),
                new[] { CSharpSyntaxTree.ParseText(sourceCode) },
                new[]
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Console).Assembly.Location)
                },
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));
        }

        private static VisualBasicCompilation CreateVBCompilation(FileInfo sourceFile, string sourceCode)
        {
            return VisualBasicCompilation.Create(
                Path.GetFileNameWithoutExtension(sourceFile.Name),
                new[] { VisualBasicSyntaxTree.ParseText(sourceCode) },
                new[]
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Console).Assembly.Location)
                },
                new VisualBasicCompilationOptions(OutputKind.ConsoleApplication));
        }
    }
}
