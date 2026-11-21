using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace QuickLauncher
{
    // key=value lines in %APPDATA%\QuickLauncher\settings.txt (same format as Kit Helper).
    // Per-tab keys are prefixed with the tab: GamesRoots, AppsRoots, GamesFoldersOff, AppsFoldersOff.
    sealed class Settings
    {
        // Must stay ABOVE Current: static initialisers run in text order.
        public static readonly string Dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QuickLauncher");
        static readonly string FilePath = Path.Combine(Dir, "settings.txt");

        public static Settings Current { get; } = Load();
        readonly Dictionary<string, string> values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        static Settings Load()
        {
            var s = new Settings();
            try
            {
                if (File.Exists(FilePath))
                    foreach (string line in File.ReadAllLines(FilePath))
                    {
                        int i = line.IndexOf('=');
                        if (i > 0) s.values[line.Substring(0, i)] = line.Substring(i + 1);
                    }
            }
            catch { }
            return s;
        }

        public string Get(string key, string fallback = "") => values.TryGetValue(key, out var v) ? v : fallback;
        public void Set(string key, string value) => values[key] = value ?? "";
        public void Remove(string key) => values.Remove(key);

        public double? GetDouble(string key) =>
            double.TryParse(Get(key), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out double d) ? d : (double?)null;
        public void SetDouble(string key, double value) =>
            Set(key, value.ToString(System.Globalization.CultureInfo.InvariantCulture));

        // "|"-separated lists: a tab's folders to scan, its switched-off folder pills.
        public List<string> GetList(string key, string fallback = "") =>
            Get(key, fallback).Split('|').Select(r => r.Trim().TrimEnd('\\'))
                .Where(r => r != "").Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        public void SetList(string key, IEnumerable<string> value) => Set(key, string.Join("|", value));

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Dir);
                File.WriteAllLines(FilePath, values.Select(kv => kv.Key + "=" + kv.Value));
            }
            catch { }
        }
    }
}
