using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace VideoBrowser
{
    /// <summary>Plain key=value settings.txt next to the exe.</summary>
    sealed class Settings
    {
        public string LastFolder;
        public bool Recursive;
        public bool ConfirmRecycle = true;
        public bool AutoNext = true;
        public int Volume = 80;
        public int PlaylistWidth = 420;
        public bool Maximized;
        public Rectangle WindowBounds = new Rectangle(100, 80, 1400, 850);

        static string FilePath => Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "settings.txt");

        public static Settings Load()
        {
            var s = new Settings();
            try
            {
                if (!File.Exists(FilePath)) return s;
                var d = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var line in File.ReadAllLines(FilePath))
                {
                    int eq = line.IndexOf('=');
                    if (eq > 0) d[line.Substring(0, eq).Trim()] = line.Substring(eq + 1).Trim();
                }
                if (d.TryGetValue("LastFolder", out var v)) s.LastFolder = v;
                if (d.TryGetValue("Recursive", out v)) s.Recursive = v == "1";
                if (d.TryGetValue("ConfirmRecycle", out v)) s.ConfirmRecycle = v == "1";
                if (d.TryGetValue("AutoNext", out v)) s.AutoNext = v == "1";
                if (d.TryGetValue("Volume", out v) && int.TryParse(v, out var i)) s.Volume = Math.Max(0, Math.Min(100, i));
                if (d.TryGetValue("PlaylistWidth", out v) && int.TryParse(v, out i)) s.PlaylistWidth = Math.Max(150, i);
                if (d.TryGetValue("Maximized", out v)) s.Maximized = v == "1";
                if (d.TryGetValue("WindowBounds", out v))
                {
                    var p = v.Split(',');
                    if (p.Length == 4 && int.TryParse(p[0], out var x) && int.TryParse(p[1], out var y) &&
                        int.TryParse(p[2], out var w) && int.TryParse(p[3], out var h) && w > 200 && h > 200)
                        s.WindowBounds = new Rectangle(x, y, w, h);
                }
            }
            catch { }
            return s;
        }

        public void Save()
        {
            try
            {
                File.WriteAllLines(FilePath, new[]
                {
                    "LastFolder=" + LastFolder,
                    "Recursive=" + (Recursive ? "1" : "0"),
                    "ConfirmRecycle=" + (ConfirmRecycle ? "1" : "0"),
                    "AutoNext=" + (AutoNext ? "1" : "0"),
                    "Volume=" + Volume,
                    "PlaylistWidth=" + PlaylistWidth,
                    "Maximized=" + (Maximized ? "1" : "0"),
                    $"WindowBounds={WindowBounds.X},{WindowBounds.Y},{WindowBounds.Width},{WindowBounds.Height}",
                });
            }
            catch { }
        }
    }
}
