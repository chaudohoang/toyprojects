using System;
using System.Diagnostics;
using System.IO;  // Added for file existence checking
using System.Security.Principal;
using Microsoft.Win32;

class Program
{
    static void Main(string[] args)
    {
        // Check if the application is running as administrator
        if (!IsAdministrator())
        {
            // Relaunch the application with administrator privileges
            RelaunchAsAdministrator();
            return;
        }

        // Proceed with the main functionality
        ToggleCompatibilityMode();
    }

    static bool IsAdministrator()
    {
        using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
        {
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }

    static void RelaunchAsAdministrator()
    {
        try
        {
            ProcessStartInfo proc = new ProcessStartInfo
            {
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory,
                FileName = Process.GetCurrentProcess().MainModule.FileName,
                Verb = "runas" // Specifies to run the process with elevated privileges
            };

            Process.Start(proc);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to restart as administrator.");
            Console.WriteLine(ex.Message);
        }

        // Exit the current instance
        Environment.Exit(0);
    }

    static void ToggleCompatibilityMode()
    {
        string exePath = @"C:\Program Files\Radiant Vision Systems\TrueTest 1.8\POCBMobile\POCBMobile.exe";
        string registryKeyPath = @"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers";

        // Check if the executable exists
        if (!File.Exists(exePath))
        {
            Console.WriteLine($"The specified executable does not exist: {exePath}");
            Console.WriteLine("Exiting...");
            return;
        }

        try
        {
            // Open the registry key for HKEY_CURRENT_USER
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(registryKeyPath, true))
            {
                if (key == null)
                {
                    Console.WriteLine("Registry key not found.");
                    return;
                }

                // Read the current compatibility setting for the exe
                object compatibilitySetting = key.GetValue(exePath);

                bool isWindows7Compat = compatibilitySetting != null && compatibilitySetting.ToString().Contains("WIN7RTM");

                Console.WriteLine($"Compatibility Mode for Windows 7 is {(isWindows7Compat ? "ENALBED" : "DISABLED")} for {exePath}");
                Console.WriteLine("Options:");
                if (isWindows7Compat)
                {
                    Console.WriteLine("1. Toggle Windows 7 Compatibility Mode (Disable it now) ");
                }
                else Console.WriteLine("1. Toggle Windows 7 Compatibility Mode (Enable it now) ");

                Console.WriteLine("2. Exit");

                string choice = Console.ReadLine();

                if (choice == "1")
                {
                    if (isWindows7Compat)
                    {
                        // Remove compatibility setting
                        key.DeleteValue(exePath, false);
                        Console.WriteLine("Windows 7 Compatibility Mode is now disabled.");
                    }
                    else
                    {
                        // Enable compatibility for Windows 7
                        string win7CompatValue = "~ WIN7RTM";
                        key.SetValue(exePath, win7CompatValue, RegistryValueKind.String);
                        Console.WriteLine("Windows 7 Compatibility Mode is now enabled.");
                    }
                }
                else if (choice == "2")
                {
                    Console.WriteLine("Exiting...");
                }
                else
                {
                    Console.WriteLine("Invalid option.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred:");
            Console.WriteLine(ex.Message);
        }
    }
}
