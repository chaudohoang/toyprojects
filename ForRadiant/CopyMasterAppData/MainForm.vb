Imports System
Imports System.Windows.Forms
Imports System.Runtime.InteropServices
Imports System.Reflection
Imports System.IO
Imports System.Windows.Forms.VisualStyles.VisualStyleElement
Imports System.Diagnostics
Imports System.Drawing
Imports System.Reflection.Emit

Namespace TemplateAppVB
	Partial Public Class MainForm
		Inherits Form
		Public apppath As String
		Public appdir As String
		Public AppDataPath As String = "C:\Radiant Vision Systems Data\TrueTest\AppData"
		Public MasterAppDataPath As String = "C:\Radiant Vision Systems Data\TrueTest\Master AppData"
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

		Private Sub btnAddSdf_Click(sender As Object, e As EventArgs) Handles btnAddSdf.Click
			Dim files = Directory.GetFiles(AppDataPath)
			For Each item As String In files
				If Not ListBox1.Items.Contains(item) AndAlso Path.GetExtension(item) = ".sdf" Then
					ListBox1.Items.Add(item)
				End If
			Next
		End Sub

		Private Sub btnAddXml_Click(sender As Object, e As EventArgs) Handles btnAddXml.Click
			Dim files = Directory.GetFiles(AppDataPath)
			For Each item As String In files
				If Not ListBox1.Items.Contains(item) AndAlso Path.GetExtension(item) = ".xml" Then
					ListBox1.Items.Add(item)
				End If
			Next
		End Sub

		Private Sub btnAddCsv_Click(sender As Object, e As EventArgs) Handles btnAddCsv.Click
			Dim files = Directory.GetFiles(AppDataPath)
			For Each item As String In files
				If Not ListBox1.Items.Contains(item) AndAlso Path.GetExtension(item) = ".csv" Then
					ListBox1.Items.Add(item)
				End If
			Next
		End Sub

		Private Sub btnAddCustom_Click(sender As Object, e As EventArgs) Handles btnAddCustom.Click
			Using dialog As OpenFileDialog = New OpenFileDialog()
				dialog.InitialDirectory = "C:\Radiant Vision Systems Data\TrueTest\AppData"
				dialog.Multiselect = True
				If dialog.ShowDialog(Me) = DialogResult.OK Then
					Dim files = dialog.FileNames
					For Each file In files
						If Not ListBox1.Items.Contains(file) Then
							ListBox1.Items.Add(file)
						End If
					Next
				End If
			End Using
		End Sub

		Private Sub btnClearList_Click(sender As Object, e As EventArgs) Handles btnClearList.Click
			ListBox1.Items.Clear()
		End Sub

		Private Sub btnDelItems_Click(sender As Object, e As EventArgs) Handles btnDelItems.Click
			For i As Integer = ListBox1.SelectedIndices.Count - 1 To 0 Step -1
				ListBox1.Items.RemoveAt(ListBox1.SelectedIndices.Item(i))
			Next
		End Sub

		Private Sub btnSaveList_Click(sender As Object, e As EventArgs) Handles btnSaveList.Click

			Dim savepath = Path.Combine("C:\Radiant Vision Systems Data\TrueTest\AppData", DateTime.Now.ToString("yyyyMMdd") + "_MasterAppDataList.txt")
			If ListBox1.Items.Count = 0 Then
				MessageBox.Show("Empty List !!!")
			Else
				Dim savefile As SaveFileDialog = New SaveFileDialog()
				' set a default file name
				savefile.FileName = Path.GetFileName(savepath)
				' set filters - this can be done in properties as well
				savefile.Filter = "Text files (*.txt)|*.txt"
				savefile.InitialDirectory = "C:\Radiant Vision Systems Data\TrueTest\AppData"

				If savefile.ShowDialog() = DialogResult.OK Then
					Using sw As StreamWriter = New StreamWriter(savefile.FileName)

						For index = 0 To ListBox1.Items.Count - 1
							sw.WriteLine(ListBox1.Items(index))
						Next
					End Using

				End If

			End If
		End Sub

		Private Sub btnLoadList_Click(sender As Object, e As EventArgs) Handles btnLoadList.Click
			Using dialog As OpenFileDialog = New OpenFileDialog()
				dialog.InitialDirectory = "C:\Radiant Vision Systems Data\TrueTest\AppData"
				dialog.Multiselect = False
				If dialog.ShowDialog(Me) = DialogResult.OK Then
					Dim files = File.ReadAllLines(dialog.FileName)
					ListBox1.Items.Clear()
					For Each file In files
						If Not ListBox1.Items.Contains(file) Then
							ListBox1.Items.Add(file)
						End If
					Next
				End If
			End Using
		End Sub
		Private Sub btnCopy_Click(sender As Object, e As EventArgs) Handles btnCopy.Click
			btnCopy.Enabled = False
			If Not Directory.Exists("C:\Radiant Vision Systems Data\TrueTest\Master AppData") Then
				Directory.CreateDirectory("C:\Radiant Vision Systems Data\TrueTest\Master AppData")
			End If
			For Each item As String In ListBox1.Items
				Dim newPath As String = item.Replace("AppData", "Master AppData")
				File.Copy(item, newPath, True)
			Next
			Static m_Rnd As New Random
			Dim tempcolor As Color
			tempcolor = Label1.ForeColor
			Do While Label1.ForeColor = tempcolor
				Label1.ForeColor = Color.FromArgb(255, m_Rnd.Next(0, 255), m_Rnd.Next(0, 255), m_Rnd.Next(0, 255))
			Loop

			Label1.Text = "DONE !"
			btnCopy.Enabled = True
		End Sub
	End Class
End Namespace
