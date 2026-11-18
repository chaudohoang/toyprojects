using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KitHelper
{
    // key=value lines in %APPDATA%\KitHelper\settings.txt. One instance, shared by the icon and
    // the panel.
    sealed class Settings
    {
        public static Settings Current { get; } = Load();

        static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KitHelper", "settings.txt");
        readonly Dictionary<string, string> values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        static Settings Load()
        {
            var s = new Settings();
            try
            {
                // First run after the rename: start from Script Toolbox's settings (left in place).
                string old = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ScriptToolbox", "settings.txt");
                string path = File.Exists(FilePath) ? FilePath : File.Exists(old) ? old : null;
                if (path != null)
                    foreach (string line in File.ReadAllLines(path))
                    {
                        int i = line.IndexOf('=');
                        if (i > 0) s.values[line.Substring(0, i)] = line.Substring(i + 1);
                    }
            }
            catch { }
            return s;
        }

        public string Get(string key) => values.TryGetValue(key, out var v) ? v : "";
        public void Set(string key, string value) => values[key] = value ?? "";

        public double? GetDouble(string key) =>
            double.TryParse(Get(key), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out double d) ? d : (double?)null;
        public void SetDouble(string key, double value) =>
            Set(key, value.ToString(System.Globalization.CultureInfo.InvariantCulture));

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                File.WriteAllLines(FilePath, values.Select(kv => kv.Key + "=" + kv.Value));
            }
            catch { }
        }
    }
}
