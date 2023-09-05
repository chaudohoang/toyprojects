Imports System
Imports System.Runtime.InteropServices

Namespace AutoDeleteData
	Friend Class NativeMethods
		Public Const HWND_BROADCAST As Integer = &HFFFF
		Public Shared ReadOnly WM_SHOWME As Integer = NativeMethods.RegisterWindowMessage("WM_SHOWME")
		<DllImport("user32")>
		Public Shared Function PostMessage(ByVal hwnd As IntPtr, ByVal msg As Integer, ByVal wparam As IntPtr, ByVal lparam As IntPtr) As Boolean
		End Function
		<DllImport("user32")>
		Public Shared Function RegisterWindowMessage(ByVal message As String) As Integer
		End Function
	End Class
End Namespace
