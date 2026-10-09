using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.Win32;

namespace RemoteToolsTabbed
{
    /// <summary>
    /// The TightVNC viewer options used for every session.
    ///
    /// Why this exists: options passed on the command line win over whatever the viewer has
    /// stored, and the viewer keeps its own options per host under
    /// HKCU\Software\TightVNC\Viewer\History\&lt;host&gt;, so settings made in one session do not
    /// necessarily come back in the next one. A profile makes it explicit - one option set,
    /// applied to every machine.
    ///
    /// The viewer reads such a profile with -optionsfile=FILE. That switch restores the whole
    /// connection *including the host*, and ignores -host= and a bare host argument (verified
    /// against this tvnviewer build), so a per-session copy is written with the right host in it.
    ///
    /// There is one profile, applied to every machine. Per-machine overrides
    /// ("viewer-profile.&lt;host&gt;.vnc", laid over this one) used to exist and were removed:
    /// they keyed off the address actually connected to, so an override only ever applied to
    /// whichever of a machine's names or IPs happened to be in the list, and a file that
    /// matched nothing was silently ignored with no way to tell.
    /// </summary>
    internal static class ViewerProfile
    {
        private const string ConnectionSection = "connection";
        private const string OptionsSection = "options";
        private const int DefaultPort = 5900;
        private const string SessionFilePrefix = "RemoteToolsTabbed-";

        /// <summary>Where the viewer keeps the options set through its own Options dialog.</summary>
        private const string ViewerDefaultsKey = @"Software\TightVNC\Viewer\History";

        public static string ProfilePath
        {
            get { return Path.Combine(Paths.AppFolder, "viewer-profile.vnc"); }
        }

        public static bool Exists
        {
            get { return File.Exists(ProfilePath); }
        }

        /// <summary>
        /// Copies the viewer's current default options into the profile. Those defaults are what
        /// its "Connection Options" dialog writes when it is opened before connecting.
        /// Returns the number of options captured, or -1 if the viewer has never stored any.
        /// </summary>
        public static int CaptureFromViewerDefaults()
        {
            Dictionary<string, string> options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(ViewerDefaultsKey))
            {
                if (key == null)
                    return -1;

                foreach (string name in key.GetValueNames())
                {
                    if (string.IsNullOrEmpty(name))
                        continue;

                    object value = key.GetValue(name);
                    if (value == null)
                        continue;

                    // every connection option TightVNC stores is a DWORD
                    if (key.GetValueKind(name) != RegistryValueKind.DWord)
                        continue;

                    options[name] = Convert.ToInt32(value).ToString(CultureInfo.InvariantCulture);
                }
            }

            if (options.Count == 0)
                return -1;

            Write(ProfilePath, string.Empty, DefaultPort, options);
            return options.Count;
        }

        /// <summary>
        /// Writes a copy of the profile pointed at <paramref name="address"/> and returns its
        /// path, or null when there is no profile to apply. The caller deletes it when the
        /// session is over.
        /// </summary>
        public static string CreateSessionFile(string address)
        {
            string host;
            int port;
            SplitAddress(address, out host, out port);

            Dictionary<string, string> options = TryReadOptions(ProfilePath);

            if (options == null || options.Count == 0)
                return null;    // no profile at all - fall back to a plain connection

            string path = Path.Combine(Path.GetTempPath(),
                SessionFilePrefix + Guid.NewGuid().ToString("N") + ".vnc");

            Write(path, host, port, options);
            return path;
        }

        private static Dictionary<string, string> TryReadOptions(string path)
        {
            if (!File.Exists(path))
                return null;

            try
            {
                return ReadOptions(path);
            }
            catch (Exception)
            {
                return null;    // unreadable file - treat it as absent
            }
        }

        /// <summary>
        /// "10.0.0.5", "10.0.0.5::5901" and "10.0.0.5:1" (a display number) all appear in
        /// machine lists, so all three are understood.
        /// </summary>
        public static void SplitAddress(string address, out string host, out int port)
        {
            host = (address ?? string.Empty).Trim();
            port = DefaultPort;

            int doubleColon = host.IndexOf("::", StringComparison.Ordinal);
            if (doubleColon > 0)
            {
                int explicitPort;
                if (int.TryParse(host.Substring(doubleColon + 2), NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out explicitPort))
                    port = explicitPort;

                host = host.Substring(0, doubleColon);
                return;
            }

            int colon = host.LastIndexOf(':');
            if (colon > 0)
            {
                int number;
                if (int.TryParse(host.Substring(colon + 1), NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out number))
                {
                    // small numbers are display numbers, the way VNC has always written them
                    port = number < 100 ? DefaultPort + number : number;
                    host = host.Substring(0, colon);
                }
            }
        }

        /// <summary>
        /// Session files are deleted as soon as the viewer has read them, but a crash or a
        /// killed process can leave one behind - clear out the old ones on start-up.
        /// </summary>
        public static void CleanupStaleSessionFiles()
        {
            try
            {
                DateTime cutoff = DateTime.UtcNow.AddHours(-1);

                foreach (string file in Directory.GetFiles(Path.GetTempPath(), SessionFilePrefix + "*.vnc"))
                {
                    if (File.GetLastWriteTimeUtc(file) < cutoff)
                        File.Delete(file);
                }
            }
            catch (Exception)
            {
                // best effort only
            }
        }

        private static Dictionary<string, string> ReadOptions(string path)
        {
            Dictionary<string, string> options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // Anything before the first [section] header counts as an option, so a profile
            // trimmed down to a handful of lines still reads correctly without one.
            string section = OptionsSection;

            foreach (string rawLine in File.ReadAllLines(path))
            {
                string line = rawLine.Trim();
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                    continue;

                if (line.StartsWith("[", StringComparison.Ordinal) && line.EndsWith("]", StringComparison.Ordinal))
                {
                    section = line.Substring(1, line.Length - 2).Trim();
                    continue;
                }

                if (!string.Equals(section, OptionsSection, StringComparison.OrdinalIgnoreCase))
                    continue;   // [connection] is rewritten per session, so it is not read back

                int equals = line.IndexOf('=');
                if (equals <= 0)
                    continue;

                options[line.Substring(0, equals).Trim()] = line.Substring(equals + 1).Trim();
            }

            return options;
        }

        private static void Write(string path, string host, int port, Dictionary<string, string> options)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[" + ConnectionSection + "]");
            sb.AppendLine("host=" + host);
            sb.AppendLine("port=" + port.ToString(CultureInfo.InvariantCulture));
            // deliberately no password= line: a stored VNC password is recoverable, and the
            // per-session copy of this file lives in the temp folder.
            sb.AppendLine();
            sb.AppendLine("[" + OptionsSection + "]");

            foreach (KeyValuePair<string, string> option in options)
            {
                if (string.Equals(option.Key, "password", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(option.Key, "host", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(option.Key, "port", StringComparison.OrdinalIgnoreCase))
                    continue;

                sb.AppendLine(option.Key + "=" + option.Value);
            }

            File.WriteAllText(path, sb.ToString(), Encoding.ASCII);
        }
    }
}
