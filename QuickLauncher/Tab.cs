using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace QuickLauncher
{
    // One tab of the panel (Games, Apps): its own folders, list, switched-off pills and scan state.
    // Search, sort and "show hidden" are shared by both tabs.
    sealed class Tab
    {
        public string Key;            // "Games" / "Apps": settings prefix and list file name
        public string One, Many;      // "game"/"games", "app"/"apps"
        public string Verb;           // "Play" / "Open"
        public bool StartMenu;        // also list Start menu apps
        public string DefaultRoots;

        public List<Game> Items = new List<Game>();
        public HashSet<string> FoldersOff;
        public bool Scanning;
        public DateTime? LastScan;
        public string Status = "";

        public string ListFile => Path.Combine(Settings.Dir, Key.ToLowerInvariant() + ".tsv");

        public List<string> Roots
        {
            get => Settings.Current.GetList(Key + "Roots", DefaultRoots);
            set => Settings.Current.SetList(Key + "Roots", value);
        }

        public void LoadState()
        {
            FoldersOff = new HashSet<string>(Settings.Current.GetList(Key + "FoldersOff"), StringComparer.OrdinalIgnoreCase);
            Items = Library.Load(ListFile);
        }

        public void SaveState() => Settings.Current.SetList(Key + "FoldersOff", FoldersOff);

        // Games: every local drive's \Games folder (F: and G: on the main PC, E: on the desktops,
        // D: on the laptops), so a fresh copy on any of Chau's PCs finds its games with no setup.
        // Network drives are left out: Z: on the other PCs is the main PC's F:.
        public static Tab Games() => new Tab
        {
            Key = "Games", One = "game", Many = "games", Verb = "Play",
            DefaultRoots = string.Join("|", DriveInfo.GetDrives()
                .Where(d => d.DriveType == DriveType.Fixed && d.IsReady)
                .Select(d => Path.Combine(d.RootDirectory.FullName, "Games"))
                .Where(Directory.Exists)),
        };

        // Apps: the Start menu, plus D:\PortableApps (one folder per portable app) if this PC has it.
        public static Tab Apps() => new Tab
        {
            Key = "Apps", One = "app", Many = "apps", Verb = "Open", StartMenu = true,
            DefaultRoots = Directory.Exists(@"D:\PortableApps") ? @"D:\PortableApps" : "",
        };
    }
}
