using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace CameraViewer
{
    sealed class CamInfo
    {
        public bool Enabled = true;
        public string Name = "";
        /// <summary>Main stream: sharp, used when one camera is enlarged.</summary>
        public string Main = "";
        /// <summary>Sub stream: light, used in the grid. Empty = use Main.</summary>
        public string Sub = "";
        /// <summary>Own login (e.g. EZVIZ: admin + sticker verification code).
        /// Empty User = use the shared NVR login from Settings.</summary>
        public string User = "";
        public string Pass = "";

        public string GridUrl => string.IsNullOrWhiteSpace(Sub) ? Main : Sub;
        public CamInfo Clone() => (CamInfo)MemberwiseClone();
    }

    /// <summary>
    /// cameras.ini next to the exe, so copying the folder to another PC carries the
    /// whole setup. The password is only obfuscated (base64), not encrypted.
    /// </summary>
    sealed class Config
    {
        public string NvrIp = "192.168.100.100";
        public int NvrChannels = 11;
        public string User = "viewer";
        public string Password = "";
        /// <summary>0 = pick the column count that gives the biggest tiles.</summary>
        public int Columns;
        public bool Maximized = true;
        public Rectangle WindowBounds = new Rectangle(80, 60, 1400, 850);
        public List<CamInfo> Cameras = new List<CamInfo>();

        public static string FilePath => Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "cameras.ini");
        public bool FileExisted { get; private set; }

        /// <summary>Uniview NVR stream address: c = channel, s0 = main, s1 = sub.</summary>
        public static string UniviewUrl(string ip, int channel, bool main) =>
            $"rtsp://{ip}:554/unicast/c{channel}/s{(main ? 0 : 1)}/live";

        public static List<CamInfo> UniviewCameras(string ip, int count, IList<CamInfo> keepNamesFrom)
        {
            var list = new List<CamInfo>();
            for (int c = 1; c <= count; c++)
            {
                var cam = new CamInfo { Name = "D" + c, Main = UniviewUrl(ip, c, true), Sub = UniviewUrl(ip, c, false) };
                if (keepNamesFrom != null)
                    foreach (var old in keepNamesFrom)
                        if (string.Equals(old.Main, cam.Main, StringComparison.OrdinalIgnoreCase))
                        { cam.Name = old.Name; cam.Enabled = old.Enabled; break; }
                list.Add(cam);
            }
            return list;
        }

        public static Config Load()
        {
            var cfg = new Config();
            try
            {
                if (File.Exists(FilePath))
                {
                    cfg.FileExisted = true;
                    foreach (var raw in File.ReadAllLines(FilePath, Encoding.UTF8))
                    {
                        var line = raw.Trim();
                        if (line.Length == 0 || line.StartsWith("#") || line.StartsWith(";")) continue;
                        int eq = line.IndexOf('=');
                        if (eq <= 0) continue;
                        string k = line.Substring(0, eq).Trim(), v = line.Substring(eq + 1).Trim();
                        switch (k.ToLowerInvariant())
                        {
                            case "nvrip": cfg.NvrIp = v; break;
                            case "nvrchannels": if (int.TryParse(v, out var n)) cfg.NvrChannels = Math.Max(1, Math.Min(64, n)); break;
                            case "user": cfg.User = v; break;
                            case "password": cfg.Password = Unprotect(v); break;
                            case "columns": if (int.TryParse(v, out n)) cfg.Columns = Math.Max(0, Math.Min(8, n)); break;
                            case "maximized": cfg.Maximized = v == "1"; break;
                            case "windowbounds":
                                var p = v.Split(',');
                                if (p.Length == 4 && int.TryParse(p[0], out var x) && int.TryParse(p[1], out var y) &&
                                    int.TryParse(p[2], out var w) && int.TryParse(p[3], out var h) && w > 300 && h > 200)
                                    cfg.WindowBounds = new Rectangle(x, y, w, h);
                                break;
                            case "camera":
                                // Camera=1|Name|main url|sub url|user|password
                                var f = v.Split('|');
                                if (f.Length >= 3)
                                    cfg.Cameras.Add(new CamInfo
                                    {
                                        Enabled = f[0].Trim() != "0",
                                        Name = f[1].Trim(),
                                        Main = f[2].Trim(),
                                        Sub = f.Length > 3 ? f[3].Trim() : "",
                                        User = f.Length > 4 ? f[4].Trim() : "",
                                        Pass = f.Length > 5 ? Unprotect(f[5].Trim()) : "",
                                    });
                                break;
                        }
                    }
                }
            }
            catch { }

            if (!cfg.FileExisted && cfg.Cameras.Count == 0)
            {
                cfg.Cameras = UniviewCameras(cfg.NvrIp, cfg.NvrChannels, null);
                cfg.Cameras[3].Name = "D4 Phong khach T2";
            }
            return cfg;
        }

        public void Save()
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("# Camera Viewer - cau hinh. Sua trong app (nut Cai dat) cho an toan.");
                sb.AppendLine("# Camera=Hien(1/0)|Ten|Luong chinh (xem lon)|Luong phu (luoi)|Tai khoan rieng|Mat khau rieng");
                sb.AppendLine("NvrIp=" + NvrIp);
                sb.AppendLine("NvrChannels=" + NvrChannels);
                sb.AppendLine("User=" + User);
                sb.AppendLine("Password=" + Protect(Password));
                sb.AppendLine("Columns=" + Columns);
                sb.AppendLine("Maximized=" + (Maximized ? "1" : "0"));
                sb.AppendLine($"WindowBounds={WindowBounds.X},{WindowBounds.Y},{WindowBounds.Width},{WindowBounds.Height}");
                foreach (var c in Cameras)
                    sb.AppendLine($"Camera={(c.Enabled ? 1 : 0)}|{c.Name.Replace("|", "/")}|{c.Main}|{c.Sub}|{c.User.Replace("|", "")}|{Protect(c.Pass)}");
                File.WriteAllText(FilePath, sb.ToString(), new UTF8Encoding(false));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không lưu được cameras.ini:\n" + ex.Message, "Camera Viewer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>The login a camera's stream uses: user:pass written in the URL, else the
        /// camera's own login, else the shared NVR login.</summary>
        public void LoginFor(CamInfo c, string url, out string user, out string pass)
        {
            try
            {
                var u = new Uri(url);
                if (!string.IsNullOrEmpty(u.UserInfo))
                {
                    int colon = u.UserInfo.IndexOf(':');
                    user = Uri.UnescapeDataString(colon >= 0 ? u.UserInfo.Substring(0, colon) : u.UserInfo);
                    pass = colon >= 0 ? Uri.UnescapeDataString(u.UserInfo.Substring(colon + 1)) : "";
                    return;
                }
            }
            catch { }
            if (!string.IsNullOrEmpty(c.User)) { user = c.User; pass = c.Pass ?? ""; return; }
            user = User;
            pass = Password;
        }

        /// <summary>Cameras sharing a key share one login check: same device, same login.</summary>
        public static string LoginKey(string url, string user, string pass)
        {
            string host = url;
            try { var u = new Uri(url); host = u.Host + ":" + (u.Port > 0 ? u.Port : 554); } catch { }
            return host + "|" + user + "|" + pass;
        }

        static string Protect(string s) =>
            string.IsNullOrEmpty(s) ? "" : "enc:" + Convert.ToBase64String(Encoding.UTF8.GetBytes(s));

        static string Unprotect(string s)
        {
            if (!s.StartsWith("enc:")) return s;
            try { return Encoding.UTF8.GetString(Convert.FromBase64String(s.Substring(4))); }
            catch { return ""; }
        }
    }
}
