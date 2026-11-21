using System;
using System.Globalization;
using System.Reflection;

namespace QuickLauncher
{
    // Version and build time of this exe, from the informational version the build stamps in
    // ("2.1.0+20261121103000", see QuickLauncher.csproj).
    static class BuildInfo
    {
        public static readonly string Version;
        public static readonly DateTime? Built;

        static BuildInfo()
        {
            string info = typeof(BuildInfo).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "";
            int plus = info.IndexOf('+');
            Version = plus > 0 ? info.Substring(0, plus) : info;
            if (plus > 0 && DateTime.TryParseExact(info.Substring(plus + 1), "yyyyMMddHHmmss",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime t))
                Built = t;
        }

        public static string Text => "v" + Version + (Built is DateTime t ? $" · built {t:yyyy-MM-dd HH:mm}" : "");
    }
}
