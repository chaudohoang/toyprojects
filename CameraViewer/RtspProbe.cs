using System;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace CameraViewer
{
    /// <summary>
    /// Raw RTSP DESCRIBE against one URL, tried without login, with Digest and with Basic,
    /// so a rejected login can be told apart from a libvlc problem. Returns a readable
    /// report with status lines only (never the password).
    /// </summary>
    static class RtspProbe
    {
        public static string Run(string url, string user, string pass)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Địa chỉ thử: " + url);
            sb.AppendLine("Tài khoản: " + user + "   (mật khẩu " + (pass ?? "").Length + " ký tự)");
            sb.AppendLine();
            try
            {
                var uri = new Uri(url);
                int port = uri.Port > 0 ? uri.Port : 554;
                string cleanUrl = $"rtsp://{uri.Host}:{port}{uri.PathAndQuery}";

                var first = Describe(uri.Host, port, cleanUrl, null);
                sb.AppendLine("1) Không đăng nhập: " + first.Status);

                var digest = Regex.Match(first.Headers, "WWW-Authenticate:\\s*Digest\\s+([^\\r\\n]*)", RegexOptions.IgnoreCase);
                if (digest.Success)
                {
                    string realm = Param(digest.Groups[1].Value, "realm");
                    string nonce = Param(digest.Groups[1].Value, "nonce");
                    string ha1 = Md5($"{user}:{realm}:{pass}");
                    string ha2 = Md5($"DESCRIBE:{cleanUrl}");
                    string resp = Md5($"{ha1}:{nonce}:{ha2}");
                    string auth = $"Digest username=\"{user}\", realm=\"{realm}\", nonce=\"{nonce}\", uri=\"{cleanUrl}\", response=\"{resp}\"";
                    sb.AppendLine("2) Digest:          " + Describe(uri.Host, port, cleanUrl, auth).Status);
                }
                else sb.AppendLine("2) Digest:          (thiết bị không đề nghị Digest)");

                if (Regex.IsMatch(first.Headers, "WWW-Authenticate:\\s*Basic", RegexOptions.IgnoreCase))
                {
                    string auth = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(user + ":" + pass));
                    sb.AppendLine("3) Basic:           " + Describe(uri.Host, port, cleanUrl, auth).Status);
                }
                else sb.AppendLine("3) Basic:           (thiết bị không đề nghị Basic)");
            }
            catch (Exception ex)
            {
                sb.AppendLine("Lỗi: " + ex.Message);
            }
            return sb.ToString();
        }

        public enum Login { Ok, Rejected, Unreachable }

        /// <summary>One login attempt (at most one wrong-password strike on the device):
        /// DESCRIBE without auth, then once with Digest (or Basic if that's all it offers).</summary>
        public static Login Check(string url, string user, string pass)
        {
            try
            {
                var uri = new Uri(url);
                int port = uri.Port > 0 ? uri.Port : 554;
                string cleanUrl = $"rtsp://{uri.Host}:{port}{uri.PathAndQuery}";
                var first = Describe(uri.Host, port, cleanUrl, null);
                if (!first.Status.Contains(" 401")) return Login.Ok;

                string auth;
                var digest = Regex.Match(first.Headers, "WWW-Authenticate:\\s*Digest\\s+([^\\r\\n]*)", RegexOptions.IgnoreCase);
                if (digest.Success)
                {
                    string realm = Param(digest.Groups[1].Value, "realm");
                    string nonce = Param(digest.Groups[1].Value, "nonce");
                    string resp = Md5($"{Md5($"{user}:{realm}:{pass}")}:{nonce}:{Md5($"DESCRIBE:{cleanUrl}")}");
                    auth = $"Digest username=\"{user}\", realm=\"{realm}\", nonce=\"{nonce}\", uri=\"{cleanUrl}\", response=\"{resp}\"";
                }
                else auth = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(user + ":" + pass));

                var second = Describe(uri.Host, port, cleanUrl, auth);
                return second.Status.Contains(" 401") ? Login.Rejected : Login.Ok;
            }
            catch
            {
                return Login.Unreachable;
            }
        }

        struct Reply { public string Status, Headers; }

        static Reply Describe(string host, int port, string url, string auth)
        {
            using (var c = new TcpClient())
            {
                var ar = c.BeginConnect(host, port, null, null);
                if (!ar.AsyncWaitHandle.WaitOne(4000)) throw new IOException("không kết nối được " + host + ":" + port);
                c.EndConnect(ar);
                c.ReceiveTimeout = 5000;
                var s = c.GetStream();
                var req = $"DESCRIBE {url} RTSP/1.0\r\nCSeq: 2\r\nAccept: application/sdp\r\nUser-Agent: CameraViewer\r\n" +
                          (auth != null ? "Authorization: " + auth + "\r\n" : "") + "\r\n";
                var bytes = Encoding.UTF8.GetBytes(req);
                s.Write(bytes, 0, bytes.Length);

                var buf = new byte[8192];
                var text = new StringBuilder();
                while (!text.ToString().Contains("\r\n\r\n"))
                {
                    int n = s.Read(buf, 0, buf.Length);
                    if (n <= 0) break;
                    text.Append(Encoding.UTF8.GetString(buf, 0, n));
                }
                var all = text.ToString();
                int eol = all.IndexOf("\r\n", StringComparison.Ordinal);
                return new Reply { Status = eol > 0 ? all.Substring(0, eol) : all, Headers = all };
            }
        }

        static string Param(string header, string name)
        {
            var m = Regex.Match(header, name + "=\"([^\"]*)\"", RegexOptions.IgnoreCase);
            return m.Success ? m.Groups[1].Value : "";
        }

        static string Md5(string s)
        {
            using (var md5 = MD5.Create())
            {
                var h = md5.ComputeHash(Encoding.UTF8.GetBytes(s));
                var sb = new StringBuilder();
                foreach (var b in h) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
