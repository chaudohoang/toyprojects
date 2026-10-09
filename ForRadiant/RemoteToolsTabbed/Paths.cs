using System;
using System.IO;

namespace RemoteToolsTabbed
{
    /// <summary>
    /// The machine list and the viewer live next to the exe, not in the working directory -
    /// a shortcut started from anywhere must still find them.
    /// </summary>
    internal static class Paths
    {
        public static string AppFolder
        {
            get { return AppDomain.CurrentDomain.BaseDirectory; }
        }

        public static string Csv
        {
            get { return Path.Combine(AppFolder, "All.csv"); }
        }

        public static string Viewer
        {
            get { return Path.Combine(AppFolder, "tvnviewer.exe"); }
        }
    }
}
