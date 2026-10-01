using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SeqxcToolset.Core
{
    /// <summary>
    /// What one task last chose in the Excel import dialog.
    ///
    /// Columns are remembered by their **header text**, not by index or letter. That
    /// matters: if someone inserts a column into the rule sheet, a remembered index
    /// would silently point at the wrong column and write wrong values into the file,
    /// whereas a remembered name either still matches the same column or fails to
    /// match and falls back to auto-detection. A wrong restore is also visible in the
    /// dialog's preview before anything is applied.
    /// </summary>
    public class ImportPrefs
    {
        public string SheetName;
        public int HeaderRow;
        public string NameHeader;
        public string ValueHeader;
        public string AsIsHeader;   // null or empty means "(none)"
    }

    /// <summary>
    /// Small persisted settings file — the last sequence and workbook opened, and each
    /// task's last import column choices.
    ///
    /// Stored under %APPDATA%\SeqxcToolset\settings.xml rather than beside the exe:
    /// build.bat deletes bin\, so settings next to the exe would be wiped by every
    /// rebuild, and a tool folder on a line PC may not be writable.
    ///
    /// Written as XML through XDocument, which the project already uses — no JSON
    /// serializer, and still no NuGet dependency. Every read and write is best-effort:
    /// a corrupt, locked or missing file leaves defaults in place rather than stopping
    /// the app, because nothing here is worth failing a launch over.
    /// </summary>
    public class AppSettings
    {
        public string LastSeqxcPath;

        private string _lastXlsxPath;

        /// <summary>
        /// The workbook imports read from. A property rather than a field because it is
        /// app-wide state that four task panels display: changing it from one task has
        /// to update the label on the others, hence the change event below.
        /// </summary>
        public string LastXlsxPath
        {
            get => _lastXlsxPath;
            set
            {
                if (_lastXlsxPath == value) return;
                _lastXlsxPath = value;
                LastXlsxPathChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Raised when <see cref="LastXlsxPath"/> changes. Static because the settings
        /// are effectively a singleton; the handful of subscribers live for the whole
        /// session, so there is nothing to unsubscribe.
        /// </summary>
        public static event EventHandler LastXlsxPathChanged;

        /// <summary>Keyed by task name, e.g. "Pattern Strings".</summary>
        public Dictionary<string, ImportPrefs> Imports =
            new Dictionary<string, ImportPrefs>(StringComparer.OrdinalIgnoreCase);

        private static AppSettings _current;

        /// <summary>
        /// The settings, loaded on first use. The assignment happens before anything can
        /// observe it incomplete; combined with Load() not raising change events, a
        /// handler that reads Current during initialisation can no longer recurse.
        /// </summary>
        public static AppSettings Current
        {
            get
            {
                if (_current == null) _current = Load();
                return _current;
            }
        }

        public static string SettingsPath
        {
            get
            {
                string dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "SeqxcToolset");
                return Path.Combine(dir, "settings.xml");
            }
        }

        public ImportPrefs PrefsFor(string taskName) =>
            taskName != null && Imports.TryGetValue(taskName, out var p) ? p : null;

        private static AppSettings Load()
        {
            var settings = new AppSettings();
            try
            {
                string path = SettingsPath;
                if (!File.Exists(path)) return settings;

                var root = XDocument.Load(path).Root;
                if (root == null) return settings;

                settings.LastSeqxcPath = (string)root.Element("LastSeqxc");

                // Backing field, NOT the property: the property raises
                // LastXlsxPathChanged, and a subscriber's handler reads
                // AppSettings.Current — which is still null while Load() is running, so
                // it would call Load() again and recurse until the stack blew. Loading
                // is also not a change in the sense the event means: nothing the user
                // did, and every binding reads the value fresh on first display anyway.
                settings._lastXlsxPath = (string)root.Element("LastXlsx");

                var imports = root.Element("Imports");
                if (imports != null)
                {
                    foreach (var el in imports.Elements("Import"))
                    {
                        string task = (string)el.Attribute("task");
                        if (string.IsNullOrEmpty(task)) continue;

                        int headerRow = 0;
                        int.TryParse((string)el.Attribute("headerRow"),
                            NumberStyles.Integer, CultureInfo.InvariantCulture, out headerRow);

                        settings.Imports[task] = new ImportPrefs
                        {
                            SheetName = (string)el.Attribute("sheet"),
                            HeaderRow = headerRow,
                            NameHeader = (string)el.Attribute("nameCol"),
                            ValueHeader = (string)el.Attribute("valueCol"),
                            AsIsHeader = (string)el.Attribute("asIsCol")
                        };
                    }
                }
            }
            catch
            {
                // Unreadable or malformed settings are simply ignored — a fresh set of
                // defaults is always a valid state to start from.
                return new AppSettings();
            }
            return settings;
        }

        public static void Save()
        {
            if (_current == null) return;
            try
            {
                string path = SettingsPath;
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                var root = new XElement("SeqxcToolset");
                if (!string.IsNullOrEmpty(_current.LastSeqxcPath))
                    root.Add(new XElement("LastSeqxc", _current.LastSeqxcPath));
                if (!string.IsNullOrEmpty(_current.LastXlsxPath))
                    root.Add(new XElement("LastXlsx", _current.LastXlsxPath));

                if (_current.Imports.Count > 0)
                {
                    var imports = new XElement("Imports");
                    foreach (var kv in _current.Imports.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
                    {
                        var p = kv.Value;
                        if (p == null) continue;
                        imports.Add(new XElement("Import",
                            new XAttribute("task", kv.Key),
                            new XAttribute("sheet", p.SheetName ?? ""),
                            new XAttribute("headerRow", p.HeaderRow.ToString(CultureInfo.InvariantCulture)),
                            new XAttribute("nameCol", p.NameHeader ?? ""),
                            new XAttribute("valueCol", p.ValueHeader ?? ""),
                            new XAttribute("asIsCol", p.AsIsHeader ?? "")));
                    }
                    root.Add(imports);
                }

                new XDocument(root).Save(path);
            }
            catch
            {
                // Losing a preference is not worth surfacing an error for.
            }
        }
    }
}
