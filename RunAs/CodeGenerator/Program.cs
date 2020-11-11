using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace CodeGenerator
{
    class Program
    {

        static void Main(string[] args)
        {

            string code;
            string filename, domain, user, pass, hash;


            //Create launcher
            var csc = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v4.0" } });
            var parameters = new CompilerParameters(new[] { "mscorlib.dll", "System.Core.dll" }, "Launcher.exe", true);
            parameters.GenerateExecutable = true;
            parameters.GenerateInMemory = true;

            code = File.ReadAllText(@"Run.cs");
            Console.Write("Applilcation Name: "); filename = Console.ReadLine(); filename = filename.Replace("\\", "\\\\");
            Console.Write("Domain (leave blank for local domain): "); domain = Console.ReadLine();
            Console.Write("User: "); user = Console.ReadLine();
            Console.Write("Password: "); pass = Console.ReadLine();
            Console.Write("Hash: "); hash = Console.ReadLine();
            hash = "8a2a945f21b8deac4dc93c9591a5e4ba46bbcf3b0be9a2615541cb6ba7746ec6";
            code = code.Replace(
                "nameholder",
                filename

                );
            code = code.Replace(
                "domainholder",
                domain

                );
            code = code.Replace(
                "userholder",
                user

                );
            code = code.Replace(
                "passholder",
                pass

                );
            code = code.Replace(
                "hashholder",
                hash

                );

            CompilerResults results = csc.CompileAssemblyFromSource(parameters, code);
            results.Errors.Cast<CompilerError>().ToList().ForEach(error => Console.WriteLine(error.ErrorText));

            //Run test
            if (File.Exists(filename))

            {
                try
                {
                    Console.WriteLine("Starting application . . .");
                    LaunchCommand1(filename, domain, user, pass);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("LaunchCommand error: " + ex.Message);
                }

            }
            else Console.WriteLine("Application not found . . .");

            Console.ReadKey();
        }

        public static string checkOrigin(string filename)
        {
            using (FileStream stream = File.OpenRead(filename))
            {
                SHA256Managed sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", String.Empty).ToLower();
            }
        }


        #region "CONTS"


        const UInt32 INFINITE = 0xFFFFFFFF;

        const UInt32 WAIT_FAILED = 0xFFFFFFFF;


        #endregion


        #region "ENUMS"


        [Flags]

        public enum LogonType

        {

            LOGON32_LOGON_INTERACTIVE = 2,

            LOGON32_LOGON_NETWORK = 3,

            LOGON32_LOGON_BATCH = 4,

            LOGON32_LOGON_SERVICE = 5,

            LOGON32_LOGON_UNLOCK = 7,

            LOGON32_LOGON_NETWORK_CLEARTEXT = 8,

            LOGON32_LOGON_NEW_CREDENTIALS = 9

        }


        [Flags]

        public enum LogonProvider

        {

            LOGON32_PROVIDER_DEFAULT = 0,

            LOGON32_PROVIDER_WINNT35,

            LOGON32_PROVIDER_WINNT40,

            LOGON32_PROVIDER_WINNT50

        }


        #endregion


        #region "STRUCTS"


        [StructLayout(LayoutKind.Sequential)]

        public struct STARTUPINFO

        {

            public Int32 cb;

            public String lpReserved;

            public String lpDesktop;

            public String lpTitle;

            public Int32 dwX;

            public Int32 dwY;

            public Int32 dwXSize;

            public Int32 dwYSize;

            public Int32 dwXCountChars;

            public Int32 dwYCountChars;

            public Int32 dwFillAttribute;

            public Int32 dwFlags;

            public Int16 wShowWindow;

            public Int16 cbReserved2;

            public IntPtr lpReserved2;

            public IntPtr hStdInput;

            public IntPtr hStdOutput;

            public IntPtr hStdError;

        }


        [StructLayout(LayoutKind.Sequential)]

        public struct PROCESS_INFORMATION

        {

            public IntPtr hProcess;

            public IntPtr hThread;

            public Int32 dwProcessId;

            public Int32 dwThreadId;

        }


        #endregion


        #region "FUNCTIONS (P/INVOKE)"


        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]

        public static extern Boolean LogonUser

        (

            String lpszUserName,

            String lpszDomain,

            String lpszPassword,

            LogonType dwLogonType,

            LogonProvider dwLogonProvider,

            out IntPtr phToken

        );


        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]

        public static extern Boolean CreateProcessAsUser

        (

            IntPtr hToken,

            String lpApplicationName,

            String lpCommandLine,

            IntPtr lpProcessAttributes,

            IntPtr lpThreadAttributes,

            Boolean bInheritHandles,

            Int32 dwCreationFlags,

            IntPtr lpEnvironment,

            String lpCurrentDirectory,

            ref STARTUPINFO lpStartupInfo,

            out PROCESS_INFORMATION lpProcessInformation

        );


        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]

        public static extern Boolean CreateProcessWithLogonW

        (

            String lpszUsername,

            String lpszDomain,

            String lpszPassword,

            Int32 dwLogonFlags,

            String applicationName,

            String commandLine,

            Int32 creationFlags,

            IntPtr environment,

            String currentDirectory,

            ref STARTUPINFO sui,

            out PROCESS_INFORMATION processInfo

        );


        [DllImport("kernel32.dll", SetLastError = true)]

        public static extern UInt32 WaitForSingleObject

        (

            IntPtr hHandle,

            UInt32 dwMilliseconds

        );


        [DllImport("kernel32", SetLastError = true)]

        public static extern Boolean CloseHandle(IntPtr handle);


        #endregion


        #region "FUNCTIONS"


        public static void LaunchCommand1(string strCommand, string strDomain, string strName, string strPassword)

        {

            // Variables

            PROCESS_INFORMATION processInfo = new PROCESS_INFORMATION();

            STARTUPINFO startInfo = new STARTUPINFO();

            bool bResult = false;

            UInt32 uiResultWait = WAIT_FAILED;


            try

            {

                // Create process

                startInfo.cb = Marshal.SizeOf(startInfo);


                bResult = CreateProcessWithLogonW(

                    strName,

                    strDomain,

                    strPassword,

                    0,

                    null,

                    strCommand,

                    0,

                    IntPtr.Zero,

                    null,

                    ref startInfo,

                    out processInfo

                );

                if (!bResult)
                {
                    throw new Exception("CreateProcessWithLogonW error #" + Marshal.GetLastWin32Error().ToString());
                }


                // Wait for process to end

                uiResultWait = WaitForSingleObject(processInfo.hProcess, INFINITE);

                if (uiResultWait == WAIT_FAILED)
                {
                    throw new Exception("WaitForSingleObject error #" + Marshal.GetLastWin32Error());
                }


            }

            finally

            {

                // Close all handles

                CloseHandle(processInfo.hProcess);

                CloseHandle(processInfo.hThread);

            }

        }


        #endregion
    }
}