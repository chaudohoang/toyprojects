﻿Imports System
Imports System.Threading
Imports System.Windows.Forms

Namespace AutoDeleteData
	Friend Module Program
		Private mutex As Mutex = New Mutex(True, "AutoBackupDataDove2p0")
		''' <summary>
		''' The main entry point for the application.
		''' </summary>
		<STAThread>
		Public Sub Main()
			If Program.mutex.WaitOne(TimeSpan.Zero, True) Then
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
