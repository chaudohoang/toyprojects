using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace ScriptToolbox
{
    // Version + build time of a ScriptToolbox.exe, read from its file version resource.
    public sealed class BuildInfo : IComparable<BuildInfo>
    {
        public Version Version;
        public DateTime Built;          // MinValue for builds made before the stamp existed (1.0.0)

        public static BuildInfo Of(string exe)
        {
            try
            {
                var fv = FileVersionInfo.GetVersionInfo(exe);
                Version.TryParse(fv.FileVersion ?? "", out Version v);
                string pv = fv.ProductVersion ?? "";
                int plus = pv.IndexOf('+');
                DateTime built = DateTime.MinValue;
                if (plus >= 0)
                    DateTime.TryParseExact(pv.Substring(plus + 1).Trim(), "yyyyMMddHHmmss",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out built);
                return new BuildInfo { Version = v ?? new Version(0, 0), Built = built };
            }
            catch { return null; }
        }

        // Version first; build time breaks a tie (two builds of the same version).
        public int CompareTo(BuildInfo other)
        {
            if (other == null) return 1;
            int c = Version.CompareTo(other.Version);
            return c != 0 ? c : Built.CompareTo(other.Built);
        }

        public override string ToString() =>
            Version.ToString(3) + (Built == DateTime.MinValue ? "" : $" (built {Built:yyyy-MM-dd HH:mm:ss})");
    }

    // Engineers keep copies of the exe all over the place. One fixed copy is "installed": the
    // Explorer right-click entry runs it, and any older copy hands over to it on start.
    public static class InstalledCopy
    {
        public const string Folder = @"D:\Program\RVS";
        public static string StaticExe => Path.Combine(Folder, "ScriptToolbox.exe");
        public static string SelfExe => Assembly.GetEntryAssembly().Location;

        public static bool IsStatic(string exe) =>
            !string.IsNullOrEmpty(exe) &&
            string.Equals(Path.GetFullPath(exe), Path.GetFullPath(StaticExe), StringComparison.OrdinalIgnoreCase);

        public static BuildInfo Self => BuildInfo.Of(SelfExe);
        public static BuildInfo Installed => File.Exists(StaticExe) ? BuildInfo.Of(StaticExe) : null;

        // True when this is some other copy and the installed one is strictly newer.
        public static bool InstalledIsNewer
        {
            get
            {
                if (IsStatic(SelfExe)) return false;
                var inst = Installed;
                return inst != null && inst.CompareTo(Self) > 0;
            }
        }

        // Puts this exe in the static folder unless the copy there is the same or newer.
        // Returns what happened, for the log.
        public static string EnsureInstalled()
        {
            if (IsStatic(SelfExe)) return $"Running the installed copy ({Self}).";
            var inst = Installed;
            if (inst != null && inst.CompareTo(Self) >= 0) return $"{StaticExe} is already {inst}.";
            Directory.CreateDirectory(Folder);
            File.Copy(SelfExe, StaticExe, true);
            return inst == null ? $"Installed {Self} to {StaticExe}." : $"Updated {StaticExe}: {inst} → {Self}.";
        }

        // Command-line form of args, for handing them to another copy.
        public static string Join(string[] args)
        {
            var parts = new string[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                string a = args[i];
                if (a.IndexOf(' ') < 0 && a != "") { parts[i] = a; continue; }
                // A trailing "\" would escape the closing quote ("D:\a b\" → D:\a b"), so double it.
                parts[i] = "\"" + (a.EndsWith("\\") ? a + "\\" : a) + "\"";
            }
            return string.Join(" ", parts);
        }
    }
}
