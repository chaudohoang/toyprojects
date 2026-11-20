namespace TrayChat;

static class Program
{
    [STAThread]
    static void Main()
    {
        Log.Write($"Starting from {Environment.ProcessPath}");
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => Fail(e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) => Fail(e.ExceptionObject as Exception);

        try
        {
            ApplicationConfiguration.Initialize();
            // All networking awaits resume on the UI thread, so no locking or Invoke is needed.
            SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
            Application.Run(new TrayApp());
            Log.Write("Exited normally");
        }
        catch (Exception ex)
        {
            Fail(ex);
        }
    }

    static void Fail(Exception? ex)
    {
        Log.Write($"FATAL {ex}");
        MessageBox.Show($"TrayChat hit an error and has to close:\n\n{ex?.Message}\n\nDetails: {Log.FilePath}",
            "TrayChat", MessageBoxButtons.OK, MessageBoxIcon.Error);
        Environment.Exit(1);
    }
}
