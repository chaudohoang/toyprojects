Imports System
Imports System.Windows.Forms
Imports System.Runtime.InteropServices
Imports System.Reflection
Imports System.IO
Imports System.Drawing
Imports System.Reflection.Emit

Namespace TemplateAppVB
	Partial Public Class MainForm
		Inherits Form
		Public apppath As String
		Public appdir As String
		Private allowVisible As Boolean = True

		<DllImport("User32.dll")>
		Private Shared Function GetLastInputInfo(ByRef plii As MainForm.LASTINPUTINFO) As Boolean
		End Function

		Friend Structure LASTINPUTINFO
			Public cbSize As UInteger

			Public dwTime As UInteger
		End Structure

		Public Sub New()
			apppath = Assembly.GetExecutingAssembly().Location
			appdir = Path.GetDirectoryName(apppath)
			InitializeComponent()
		End Sub

		Protected Overrides Sub WndProc(ByRef m As Message)
			If m.Msg = NativeMethods.WM_SHOWME Then
				ShowMe()
			End If
			MyBase.WndProc(m)
		End Sub
		Private Sub ShowMe()
			Show()
			If WindowState = FormWindowState.Minimized Then
				WindowState = FormWindowState.Normal
			End If
			' get our current "TopMost" value (ours will always be false though)
			Dim top = TopMost
			' make our form jump to the top of everything
			TopMost = True
			' set it back to whatever it was
			TopMost = top
		End Sub

		Private Sub SetVersionInfo()
			Dim versionInfo As Version = Assembly.GetExecutingAssembly().GetName().Version
			Dim startDate As Date = New DateTime(2000, 1, 1)
			Dim diffDays = versionInfo.Build
			Dim computedDate = startDate.AddDays(diffDays)
			Dim lastBuilt As String = computedDate.ToShortDateString()
			'this.Text = string.Format("{0} - {1} ({2})",
			'            this.Text, versionInfo.ToString(), lastBuilt);
			Text = String.Format("{0} - {1}", Text, versionInfo.ToString())
		End Sub

		Protected Overrides Sub SetVisibleCore(ByVal value As Boolean)
			If Not allowVisible Then
				value = False
				If Not IsHandleCreated Then CreateHandle()
			End If
			MyBase.SetVisibleCore(value)
		End Sub
		Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
			SetVersionInfo()
		End Sub

		Private Sub aboutToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles aboutToolStripMenuItem.Click
			MessageBox.Show("dh.chau@radiantvs.com")
		End Sub

		Private Sub exitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles exitToolStripMenuItem.Click
			Application.Exit()
		End Sub

		Private Sub exit2ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles exit2ToolStripMenuItem.Click
			Application.Exit()
		End Sub

		Private Sub MainForm_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
			If minimizedToTrayToolStripMenuItem.Checked = True AndAlso WindowState = FormWindowState.Minimized Then
				Hide()
			End If
		End Sub
		Private Sub notifyIcon1_DoubleClick(sender As Object, e As EventArgs) Handles notifyIcon1.DoubleClick
			allowVisible = True
			Show()
			Activate()
			WindowState = FormWindowState.Normal
		End Sub

		Private Sub btnBrowse_Click(sender As Object, e As EventArgs) Handles btnBrowse.Click
			Using dialog As OpenFileDialog = New OpenFileDialog()
				dialog.InitialDirectory = "C:\Radiant Vision Systems Data\TrueTest\Sequence"
				dialog.Multiselect = True
				If dialog.ShowDialog(Me) = DialogResult.OK Then
					Dim files = dialog.FileNames
					For Each file In files
						txtSequences.Text += file & Microsoft.VisualBasic.Constants.vbCrLf
					Next
				End If
			End Using
		End Sub

		Private Sub btnEmu2p1Convert_Click(sender As Object, e As EventArgs) Handles btnEmu2p1Convert.Click
			btnEmu2p1Convert.Enabled = False
			Try
				Dim Targets As String()
				Targets = txtSequences.Text.Split(New Char() {Microsoft.VisualBasic.Strings.ChrW(13), Microsoft.VisualBasic.Strings.ChrW(10)}, StringSplitOptions.RemoveEmptyEntries)
				For Each item In Targets
					Dim Text As String = File.ReadAllText(item)
					Dim backupFilename As String = Path.GetDirectoryName(item) + "\Emu2p0_backup\" + Path.GetFileName(item)
					If Not Directory.Exists(Path.GetDirectoryName(item) + "\Emu2p0_backup\") Then
						Directory.CreateDirectory(Path.GetDirectoryName(item) + "\Emu2p0_backup\")
					End If
					File.Copy(item, backupFilename, True)
					Text = Text.Replace("Emu2p0_PG.Emu2p0_Pattern", "Emu2p1_PG.Emu2p1_Pattern")
					File.WriteAllText(item, Text)
				Next
			Catch ex As Exception
				MessageBox.Show(ex.Message)
			End Try

			Dim m_Rnd As Random = New Random()
			Dim tempcolor As Color
			tempcolor = Label1.ForeColor
			While Label1.ForeColor = tempcolor
				Label1.ForeColor = Color.FromArgb(255, m_Rnd.Next(0, 255), m_Rnd.Next(0, 255), m_Rnd.Next(0, 255))
			End While
			Label1.Text = "Converted to Emu2p1, reload sequence !!!"
			btnEmu2p1Convert.Enabled = True

		End Sub

		Private Sub btnCopyDI_Click(sender As Object, e As EventArgs) Handles btnCopyDI.Click
			btnCopyDI.Enabled = False
			Try
				Dim Emu2p0_DI As String = "C:\Radiant Vision Systems Data\TrueTest\AppData\1.8\Emu2p0_PG.Emu2p0_PG.xml"
				Dim Emu2p1_DI As String = "C:\Radiant Vision Systems Data\TrueTest\AppData\1.8\Emu2p1_PG.Emu2p1_PG.xml"
				Dim Text As String = File.ReadAllText(Emu2p0_DI)
				Text = Text.Replace("Emu2p0", "Emu2p1")
				File.WriteAllText(Emu2p1_DI, Text)

			Catch ex As Exception
				MessageBox.Show(ex.Message)
			End Try

			Dim m_Rnd As Random = New Random()
			Dim tempcolor As Color
			tempcolor = Label1.ForeColor
			While Label1.ForeColor = tempcolor
				Label1.ForeColor = Color.FromArgb(255, m_Rnd.Next(0, 255), m_Rnd.Next(0, 255), m_Rnd.Next(0, 255))
			End While
			Label1.Text = "Done!"
			btnCopyDI.Enabled = True
		End Sub

		Private Sub btnEmu2p0Convert_Click(sender As Object, e As EventArgs) Handles btnEmu2p0Convert.Click
			btnEmu2p1Convert.Enabled = False
			Try
				Dim Targets As String()
				Targets = txtSequences.Text.Split(New Char() {Microsoft.VisualBasic.Strings.ChrW(13), Microsoft.VisualBasic.Strings.ChrW(10)}, StringSplitOptions.RemoveEmptyEntries)
				For Each item In Targets
					Dim Text As String = File.ReadAllText(item)
					Dim backupFilename As String = Path.GetDirectoryName(item) + "\Emu2p1_backup\" + Path.GetFileName(item)
					If Not Directory.Exists(Path.GetDirectoryName(item) + "\Emu2p1_backup\") Then
						Directory.CreateDirectory(Path.GetDirectoryName(item) + "\Emu2p1_backup\")
					End If
					File.Copy(item, backupFilename, True)
					Text = Text.Replace("Emu2p1_PG.Emu2p1_Pattern", "Emu2p0_PG.Emu2p0_Pattern")
					File.WriteAllText(item, Text)
				Next
			Catch ex As Exception
				MessageBox.Show(ex.Message)
			End Try

			Dim m_Rnd As Random = New Random()
			Dim tempcolor As Color
			tempcolor = Label1.ForeColor
			While Label1.ForeColor = tempcolor
				Label1.ForeColor = Color.FromArgb(255, m_Rnd.Next(0, 255), m_Rnd.Next(0, 255), m_Rnd.Next(0, 255))
			End While
			Label1.Text = "Converted to Emu2p0, reload sequence !!!"
			btnEmu2p1Convert.Enabled = True
		End Sub
	End Class
End Namespace
