// SeqXC Editor — C# WinForms .seqxc sequence file editor
// Zero dependencies, .NET Framework 4.7.2+, programmatic UI (no designer)
// Build: csc /target:winexe /out:SeqxcEditor.exe *.cs

using System;
using System.Windows.Forms;

namespace SeqxcEditor {
    static class Program {
        [STAThread]
        static void Main(string[] args) {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Show any unhandled exception instead of silently exiting
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) =>
                MessageBox.Show(e.Exception.ToString(), "Unhandled Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                MessageBox.Show(e.ExceptionObject.ToString(), "Fatal Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

            try {
                Application.Run(new MainForm(args.Length > 0 ? args[0] : null));
            } catch (Exception ex) {
                MessageBox.Show(ex.ToString(), "Startup Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
