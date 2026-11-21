using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;

namespace QuickLauncher
{
    static class Program
    {
        const string MutexName = @"Local\QuickLauncher.Running";
        const string ShowEventName = @"Local\QuickLauncher.Show";

        public static bool Exiting { get; private set; }
        static MainWindow panel;

        [STAThread]
        static void Main(string[] args)
        {
            // Before anything reads Settings: bring Game Launcher's data over on first start.
            Migration.FromGameLauncher();

            // "--dump out.tsv": scan both tabs, write every entry with its ranked exes, and exit.
            // For checking the detection without the UI.
            int di = Array.FindIndex(args, a => a.TrimStart('/', '-').Equals("dump", StringComparison.OrdinalIgnoreCase));
            if (di >= 0 && di + 1 < args.Length) { Dump(args[di + 1]); return; }

            // One app: a second launch shows the running one's panel and exits.
            var running = new Mutex(true, MutexName, out bool first);
            var show = new EventWaitHandle(false, EventResetMode.AutoReset, ShowEventName);
            if (!first) { show.Set(); return; }

            var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            app.Resources.MergedDictionaries.Add(new Theme());

            panel = new MainWindow();
            var icon = new FloatingIconWindow(panel);
            panel.Anchor = icon;
            icon.Show();
            var tray = new TrayIcon(icon, panel);
            app.Exit += (s, e) => tray.Dispose();

            // Wait for "show" signals from later launches on a background thread.
            var t = new Thread(() =>
            {
                while (show.WaitOne())
                    // Also the way back for a lost icon: run Quick Launcher again (desktop shortcut).
                    app.Dispatcher.BeginInvoke(new Action(() => { if (!Exiting) { icon.KeepOnScreen(); panel.ShowNear(icon); } }));
            }) { IsBackground = true };
            t.Start();

            // Queued, not called: the scan awaits, and must resume on the UI thread, which only
            // has a dispatcher context once app.Run() is pumping.
            app.Dispatcher.BeginInvoke(new Action(panel.StartupScan));
            app.Run();
            GC.KeepAlive(running);
        }

        public static void Exit()
        {
            Exiting = true;
            panel?.SaveAll();
            Application.Current.Shutdown();
        }

        static void Dump(string outFile)
        {
            var games = Tab.Games();
            var apps = Tab.Apps();
            var lines = new System.Collections.Generic.List<string>();
            foreach (var t in new[] { games, apps })
            {
                var found = Library.Rescan(t.Roots, new System.Collections.Generic.List<Game>(), null, t.StartMenu, t == apps ? games.Roots : null);
                lines.AddRange(found.Select(g => t.Key + "\t" + (g.IsStartMenu ? "Start menu" : g.Root) + "\t" + g.Name + "\t" + string.Join("  |  ", g.Candidates.Take(4))));
            }
            File.WriteAllLines(outFile, lines);
        }
    }

    // Quick Launcher was Game Launcher (%APPDATA%\GameLauncher, one list). On the first start,
    // with no Quick Launcher data yet, that becomes the Games tab: library.tsv → games.tsv,
    // Roots / FoldersOff → GamesRoots / GamesFoldersOff, kept shortcut copies moved over (and
    // the paths to them in the list rewritten). The old folder is left as it was.
    // Must not touch Settings: its static initialiser would load the new, still empty, file.
    static class Migration
    {
        public static void FromGameLauncher()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string oldDir = Path.Combine(appData, "GameLauncher"), newDir = Path.Combine(appData, "QuickLauncher");
            if (Directory.Exists(newDir) || !Directory.Exists(oldDir)) return;
            try
            {
                Directory.CreateDirectory(newDir);
                string oldShortcuts = Path.Combine(oldDir, "Shortcuts"), newShortcuts = Path.Combine(newDir, "Shortcuts");
                if (Directory.Exists(oldShortcuts))
                {
                    Directory.CreateDirectory(newShortcuts);
                    foreach (string f in Directory.GetFiles(oldShortcuts)) File.Copy(f, Path.Combine(newShortcuts, Path.GetFileName(f)), true);
                }
                string lib = Path.Combine(oldDir, "library.tsv");
                if (File.Exists(lib))
                    File.WriteAllLines(Path.Combine(newDir, "games.tsv"),
                        File.ReadAllLines(lib).Select(l => l.Replace(oldShortcuts + "\\", newShortcuts + "\\")));
                string set = Path.Combine(oldDir, "settings.txt");
                if (File.Exists(set))
                    File.WriteAllLines(Path.Combine(newDir, "settings.txt"), File.ReadAllLines(set)
                        .Where(l => !l.StartsWith("FolderFilter="))
                        .Select(l => l.StartsWith("Roots=") ? "Games" + l : l.StartsWith("FoldersOff=") ? "Games" + l : l));
            }
            catch { }   // worst case: start empty and scan
        }
    }
}
