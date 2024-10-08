using System;
using System.Diagnostics;
using System.IO;  // For file existence checking
using System.Security.Principal;
using Microsoft.Win32;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("C:\\Program Files\\Radiant Vision Systems\\TrueTest 1.8\\POCBMobile\\POCBMobile.exe Compatibility Mode Check ");
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

    /// <summary>
    /// Checks if the current user has administrative privileges.
    /// </summary>
    /// <returns>True if administrator, else false.</returns>
    static bool IsAdministrator()
    {
        using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
        {
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }

    /// <summary>
    /// Relaunches the current application with administrative privileges.
    /// </summary>
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

    /// <summary>
    /// Handles the logic to toggle compatibility mode for the specified executable.
    /// </summary>
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
            // Open the registry key for HKEY_CURRENT_USER with write access
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(registryKeyPath, writable: true))
            {
                if (key == null)
                {
                    Console.WriteLine("Registry key not found.");
                    return;
                }

                // Read the current compatibility setting for the exe
                object compatibilitySetting = key.GetValue(exePath);

                // Determine current compatibility mode
                string currentMode = GetCompatibilityMode(compatibilitySetting);

                Console.WriteLine($"Compatibility Mode for the executable is: {currentMode}");
                Console.WriteLine("Options:");

                if (currentMode == "Disabled")
                {
                    Console.WriteLine("1. Enable Compatibility Mode");
                    Console.WriteLine("2. Exit");
                }
                else if (currentMode == "Windows 7")
                {
                    Console.WriteLine("1. Disable Compatibility Mode");
                    Console.WriteLine("2. Switch to Windows 8 Compatibility Mode");
                    Console.WriteLine("3. Exit");
                }
                else if (currentMode == "Windows 8")
                {
                    Console.WriteLine("1. Disable Compatibility Mode");
                    Console.WriteLine("2. Switch to Windows 7 Compatibility Mode");
                    Console.WriteLine("3. Exit");
                }
                else
                {
                    Console.WriteLine("1. Disable Compatibility Mode");
                    Console.WriteLine("2. Switch Compatibility Mode");
                    Console.WriteLine("3. Exit");
                }

                Console.Write("Enter your choice: ");
                string choice = Console.ReadLine();

                if (currentMode == "Disabled")
                {
                    HandleDisabledOptions(choice, key, exePath);
                }
                else if (currentMode == "Windows 7")
                {
                    HandleEnabledOptions(choice, key, exePath, "Windows 7");
                }
                else if (currentMode == "Windows 8")
                {
                    HandleEnabledOptions(choice, key, exePath, "Windows 8");
                }
                else
                {
                    // Handle unknown or other modes
                    HandleUnknownModeOptions(choice, key, exePath);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred:");
            Console.WriteLine(ex.Message);
        }
    }

    /// <summary>
    /// Determines the current compatibility mode based on the registry value.
    /// </summary>
    /// <param name="compatibilitySetting">The registry value for compatibility.</param>
    /// <returns>A string representing the current compatibility mode.</returns>
    static string GetCompatibilityMode(object compatibilitySetting)
    {
        if (compatibilitySetting == null)
        {
            return "Disabled";
        }

        string setting = compatibilitySetting.ToString();

        if (setting.Contains("WIN7RTM"))
        {
            return "Windows 7";
        }
        else if (setting.Contains("WIN8RTM"))
        {
            return "Windows 8";
        }
        else
        {
            return "Unknown or Other";
        }
    }

    /// <summary>
    /// Handles user options when compatibility mode is disabled.
    /// </summary>
    /// <param name="choice">User's choice input.</param>
    /// <param name="key">Registry key.</param>
    /// <param name="exePath">Path to the executable.</param>
    static void HandleDisabledOptions(string choice, RegistryKey key, string exePath)
    {
        switch (choice)
        {
            case "1":
                // Enable compatibility mode
                EnableCompatibilityMode(key, exePath);
                break;
            case "2":
                Console.WriteLine("Exiting...");
                break;
            default:
                Console.WriteLine("Invalid option.");
                break;
        }
    }

    /// <summary>
    /// Handles user options when compatibility mode is enabled.
    /// </summary>
    /// <param name="choice">User's choice input.</param>
    /// <param name="key">Registry key.</param>
    /// <param name="exePath">Path to the executable.</param>
    /// <param name="currentMode">Current compatibility mode.</param>
    static void HandleEnabledOptions(string choice, RegistryKey key, string exePath, string currentMode)
    {
        switch (choice)
        {
            case "1":
                // Disable compatibility mode
                DisableCompatibilityMode(key, exePath);
                break;
            case "2":
                // Switch compatibility mode
                if (currentMode == "Windows 7")
                {
                    SetCompatibilityMode(key, exePath, "WIN8RTM");
                    Console.WriteLine("Switched to Windows 8 Compatibility Mode.");
                }
                else if (currentMode == "Windows 8")
                {
                    SetCompatibilityMode(key, exePath, "WIN7RTM");
                    Console.WriteLine("Switched to Windows 7 Compatibility Mode.");
                }
                break;
            case "3":
                Console.WriteLine("Exiting...");
                break;
            default:
                Console.WriteLine("Invalid option.");
                break;
        }
    }

    /// <summary>
    /// Handles user options when compatibility mode is in an unknown state.
    /// </summary>
    /// <param name="choice">User's choice input.</param>
    /// <param name="key">Registry key.</param>
    /// <param name="exePath">Path to the executable.</param>
    static void HandleUnknownModeOptions(string choice, RegistryKey key, string exePath)
    {
        switch (choice)
        {
            case "1":
                // Disable compatibility mode
                DisableCompatibilityMode(key, exePath);
                break;
            case "2":
                // Switch compatibility mode
                Console.WriteLine("Select Compatibility Mode to Switch:");
                Console.WriteLine("1. Windows 7");
                Console.WriteLine("2. Windows 8");
                Console.Write("Enter your choice (1 or 2): ");
                string modeChoice = Console.ReadLine();

                switch (modeChoice)
                {
                    case "1":
                        SetCompatibilityMode(key, exePath, "WIN7RTM");
                        Console.WriteLine("Switched to Windows 7 Compatibility Mode.");
                        break;
                    case "2":
                        SetCompatibilityMode(key, exePath, "WIN8RTM");
                        Console.WriteLine("Switched to Windows 8 Compatibility Mode.");
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Returning to main menu.");
                        break;
                }
                break;
            case "3":
                Console.WriteLine("Exiting...");
                break;
            default:
                Console.WriteLine("Invalid option.");
                break;
        }
    }

    /// <summary>
    /// Enables compatibility mode by prompting the user to choose the OS version.
    /// </summary>
    /// <param name="key">Registry key.</param>
    /// <param name="exePath">Path to the executable.</param>
    static void EnableCompatibilityMode(RegistryKey key, string exePath)
    {
        Console.WriteLine("Select Compatibility Mode to Enable:");
        Console.WriteLine("1. Windows 7");
        Console.WriteLine("2. Windows 8");
        Console.Write("Enter your choice (1 or 2): ");
        string modeChoice = Console.ReadLine();

        switch (modeChoice)
        {
            case "1":
                SetCompatibilityMode(key, exePath, "WIN7RTM");
                Console.WriteLine("Windows 7 Compatibility Mode is now enabled.");
                break;
            case "2":
                SetCompatibilityMode(key, exePath, "WIN8RTM");
                Console.WriteLine("Windows 8 Compatibility Mode is now enabled.");
                break;
            default:
                Console.WriteLine("Invalid choice. Returning to main menu.");
                break;
        }
    }

    /// <summary>
    /// Disables compatibility mode by removing the registry entry.
    /// </summary>
    /// <param name="key">Registry key.</param>
    /// <param name="exePath">Path to the executable.</param>
    static void DisableCompatibilityMode(RegistryKey key, string exePath)
    {
        key.DeleteValue(exePath, throwOnMissingValue: false);
        Console.WriteLine("Compatibility Mode has been disabled.");
    }

    /// <summary>
    /// Sets the compatibility mode for the executable in the registry.
    /// </summary>
    /// <param name="key">The registry key to modify.</param>
    /// <param name="exePath">The path to the executable.</param>
    /// <param name="mode">The compatibility mode flag (e.g., "WIN7RTM").</param>
    static void SetCompatibilityMode(RegistryKey key, string exePath, string mode)
    {
        string compatValue = $"~ {mode}";
        key.SetValue(exePath, compatValue, RegistryValueKind.String);
    }
}
