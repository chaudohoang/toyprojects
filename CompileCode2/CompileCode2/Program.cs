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
            string exeFolder = AppDomain.CurrentDomain.BaseDirectory;
            string logFile = Path.Combine(exeFolder, "log.txt");

            int exitCode = 0; // ✅ Default: success

            // ✅ Overwrite log file every run (no append)
            using (var logWriter = new StreamWriter(logFile, append: false))
            {
                try
                {
                    logWriter.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] --- CompileCode2.exe Started ---");

                    if (args.Length > 0)
                    {
                        if (File.Exists(args[0]))
                        {
                            logWriter.WriteLine($"Compiling: {args[0]}");
                            bool result = CompileExecutable(args[0], logWriter);
                            logWriter.WriteLine($"Compile result: {(result ? "SUCCESS" : "FAIL")}");

                            if (!result)
                                exitCode = 1; // ❌ Compilation failed → non-zero exit
                        }
                        else
                        {
                            string msg = $"Input source file not found - {args[0]}";
                            Console.WriteLine(msg);
                            logWriter.WriteLine(msg);
                            exitCode = 2; // ❌ Source file missing
                        }
                    }
                    else
                    {
                        string msg = "Input source file not specified on command line!";
                        Console.WriteLine(msg);
                        logWriter.WriteLine(msg);
                        exitCode = 3; // ❌ No argument provided
                    }

                    logWriter.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] --- CompileCode2.exe Finished ---");
                }
                catch (Exception ex)
                {
                    string msg = $"Unexpected error: {ex}";
                    Console.WriteLine(msg);
                    logWriter.WriteLine(msg);
                    exitCode = 99; // ❌ Unexpected runtime error
                }
            }

            Environment.Exit(exitCode); // ✅ Exit with proper code
        }

        public static bool CompileExecutable(string sourceName, StreamWriter logWriter)
        {
            var sourceFile = new FileInfo(sourceName);
            bool compileOk = false;
            string filePostfix = "";

            // ✅ Use the script's folder, not the compiler's working folder
            string scriptDir = sourceFile.DirectoryName ?? Directory.GetCurrentDirectory();
            string compiledFolder = Path.Combine(scriptDir, "Compiled");
            Directory.CreateDirectory(compiledFolder);

            // Read the source
            string sourceCode = File.ReadAllText(sourceName);

            // Pick language
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
                    string msg = "Source file must have a .cs or .vb extension";
                    Console.WriteLine(msg);
                    logWriter.WriteLine(msg);
                    return false;
            }

            if (compilation == null)
            {
                string msg = "Compilation object could not be created.";
                Console.WriteLine(msg);
                logWriter.WriteLine(msg);
                return false;
            }

            // Output EXE inside the script's Compiled folder
            string exeName = Path.Combine(
                compiledFolder,
                $"{Path.GetFileNameWithoutExtension(sourceFile.Name)}{filePostfix}_{DateTime.Now:yyyyMMddHHmmss}.exe"
            );

            var result = compilation.Emit(exeName);

            if (!result.Success)
            {
                string msg = $"Errors compiling {sourceName}:";
                Console.WriteLine(msg);
                logWriter.WriteLine(msg);

                foreach (var d in result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))
                {
                    Console.WriteLine(d);
                    logWriter.WriteLine(d.ToString());
                }

                string totalErr = $"Total Errors: {result.Diagnostics.Count(d => d.Severity == DiagnosticSeverity.Error)}";
                Console.WriteLine(totalErr);
                logWriter.WriteLine(totalErr);
            }
            else
            {
                string msg = $"Compilation successful: {exeName}";
                Console.WriteLine(msg);
                logWriter.WriteLine(msg);
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
