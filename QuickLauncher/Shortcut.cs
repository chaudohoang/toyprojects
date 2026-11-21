using System;
using System.IO;
using System.Reflection;

namespace QuickLauncher
{
    // Desktop shortcuts dropped on the icon. The point is to replace them, so the launcher must
    // keep working after the shortcut is deleted:
    //   .lnk to an .exe with no arguments  → added as that exe (named after the shortcut)
    //   anything else (.lnk with arguments, .lnk to a non-exe, .url such as steam://rungameid)
    //                                       → the shortcut file is COPIED into
    //                                         %APPDATA%\QuickLauncher\Shortcuts and that copy is
    //                                         what gets started, so its arguments / URL are kept.
    static class Shortcut
    {
        public static readonly string Dir = Path.Combine(Settings.Dir, "Shortcuts");

        public static bool Is(string path)
        {
            string ext = Path.GetExtension(path).ToLowerInvariant();
            return ext == ".lnk" || ext == ".url";
        }

        // The path to add as a game: the target exe, or a kept copy of the shortcut.
        public static string Resolve(string shortcut)
        {
            if (Path.GetExtension(shortcut).Equals(".lnk", StringComparison.OrdinalIgnoreCase))
            {
                var (target, args) = ReadLnk(shortcut);
                if (target != null && args == "" && File.Exists(target)
                    && Path.GetExtension(target).Equals(".exe", StringComparison.OrdinalIgnoreCase))
                    return target;
            }
            Directory.CreateDirectory(Dir);
            string copy = Path.Combine(Dir, Path.GetFileName(shortcut));
            File.Copy(shortcut, copy, true);
            return copy;
        }

        // Target and arguments through WScript.Shell (late-bound: no interop assembly needed).
        static (string target, string args) ReadLnk(string path)
        {
            object shell = null, link = null;
            try
            {
                shell = Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell"));
                link = shell.GetType().InvokeMember("CreateShortcut", BindingFlags.InvokeMethod, null, shell, new object[] { path });
                string target = (string)link.GetType().InvokeMember("TargetPath", BindingFlags.GetProperty, null, link, null);
                string args = (string)link.GetType().InvokeMember("Arguments", BindingFlags.GetProperty, null, link, null);
                return (string.IsNullOrWhiteSpace(target) ? null : target, (args ?? "").Trim());
            }
            catch { return (null, ""); }
            finally
            {
                if (link != null) System.Runtime.InteropServices.Marshal.FinalReleaseComObject(link);
                if (shell != null) System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shell);
            }
        }
    }
}
