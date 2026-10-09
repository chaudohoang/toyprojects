Imports System
Imports System.Threading
Imports System.Windows.Forms

Namespace TrueTestWatcher
	Friend Module Program
		Private mutex As Mutex = New Mutex(True, "TrueTestWatcher")
		''' <summary>
		''' The main entry point for the application.
		''' </summary>
		<STAThread>
		Public Sub Main()
			If Program.mutex.WaitOne(TimeSpan.Zero, True) Then
				' Nothing caught exceptions before this. One escaping a FileSystemWatcher callback
				' went straight to Windows Error Reporting, which suspends the process: the window
				' freezes, End Task fights WER, and the single-instance mutex above is still held so
				' relaunching does nothing either. Hence "the only thing doable is restart the PC".
				' These handlers cannot keep the process alive on their own - that is what the
				' Try blocks around the checks are for - but they leave a stack trace behind.
				AddHandler AppDomain.CurrentDomain.UnhandledException,
					Sub(sender As Object, args As UnhandledExceptionEventArgs)
						MainForm.LogFatal("AppDomain", TryCast(args.ExceptionObject, Exception))
					End Sub
				Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException)
				AddHandler Application.ThreadException,
					Sub(sender As Object, args As ThreadExceptionEventArgs)
						MainForm.LogFatal("UIThread", args.Exception)
					End Sub
				Call Application.EnableVisualStyles()
				Application.SetCompatibleTextRenderingDefault(False)
				Call Application.Run(New MainForm())
				Call Program.mutex.ReleaseMutex()
			Else
				' send our Win32 message to make the currently running instance
				' jump on top of all the other windows
				NativeMethods.PostMessage(CType(NativeMethods.HWND_BROADCAST, IntPtr), NativeMethods.WM_SHOWME, IntPtr.Zero, IntPtr.Zero)
			End If
		End Sub
	End Module
End Namespace
