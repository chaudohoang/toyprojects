Imports System
Imports System.Windows.Forms
Imports System.Runtime.InteropServices
Imports System.Reflection
Imports System.IO
Imports System.Windows.Forms.VisualStyles.VisualStyleElement
Imports System.Diagnostics
Imports System.Drawing
Imports System.Reflection.Emit
Imports System.Security.AccessControl
Imports System.Security.Cryptography
Imports System.ComponentModel
Imports System.Collections.Generic
Imports System.Xml
Imports System.Xml.Linq
Imports System.Collections.Specialized
Imports System.Linq
Imports System.Net

Namespace CopyMasterAppData
    Public Class MainForm
        Inherits Form
        Public apppath As String
        Public appdir As String
        Public password As String = ""
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

        Private Sub btnAddRFD_Click(sender As Object, e As EventArgs) Handles btnAddRFD.Click
            Dim files = Directory.GetFiles(AppDataPath)
            For Each item As String In files
                If Not ListBox1.Items.Contains(item) AndAlso Path.GetExtension(item) = ".txt" AndAlso item.Contains("RegisterPixelsFalseDefects") Then
                    ListBox1.Items.Add(item)
                End If
            Next
        End Sub
        Private Function GetSN(xmlFile As String) As String

            Dim SN As String = ""
            Dim node As XmlNode
            Dim xmlDoc = New XmlDocument()
            If File.Exists(xmlFile) Then
                xmlDoc.Load(xmlFile)
                node = xmlDoc.DocumentElement.SelectSingleNode("/LensCosineCorrection/SerialNumber")
                If node IsNot Nothing Then
                    SN = node.InnerText
                End If
            End If
            If SN <> "" Then
                Return SN
            Else
                Return Nothing
            End If

        End Function

        Private Sub btnLock_Click(sender As Object, e As EventArgs) Handles btnLock.Click
            Dim sameSN As Boolean = True
            Dim alreadyLocked As Boolean = False
            Dim SNs As New Dictionary(Of String, String)

            Dim appdataFiles = Directory.GetFiles(AppDataPath)
            For Each item As String In appdataFiles
                If Path.GetExtension(item) = ".xml" Then
                    Dim SN As String = GetSN(item)
                    If SN <> Nothing Then
                        SNs.Add(item, SN)
                    End If
                End If
            Next

            Try
                Dim masterAppDataFiles = Directory.GetFiles(MasterAppDataPath)
                For Each item As String In masterAppDataFiles
                    If Path.GetExtension(item) = ".xml" Then
                        Dim SN As String = GetSN(item)
                        If SN <> Nothing Then
                            SNs.Add(item, SN)
                        End If
                    End If
                Next
            Catch ex As UnauthorizedAccessException
                alreadyLocked = True
            End Try

            Dim keys() As String = SNs.Keys.ToArray
            For i = 0 To keys.Count - 2
                If SNs(keys(i)) <> SNs(keys(i + 1)) Then
                    sameSN = False
                    Exit For
                End If
            Next
            If alreadyLocked Then
                MessageBox.Show("Already Locked")
            ElseIf sameSN Then
                Try
                    Dim folderPath As String = "C:\Radiant Vision Systems Data\TrueTest\Master AppData"
                    Dim adminUserName = Environment.UserName
                    Dim ds As DirectorySecurity = Directory.GetAccessControl(folderPath)
                    Dim fsa As FileSystemAccessRule = New FileSystemAccessRule(adminUserName, FileSystemRights.FullControl, AccessControlType.Deny)
                    ds.AddAccessRule(fsa)
                    Directory.SetAccessControl(folderPath, ds)
                    MessageBox.Show("Locked")
                Catch ex As Exception
                    MessageBox.Show(ex.Message)
                End Try
            Else
                Dim errorMessage As String = "Serial Number Not Matching, Lock Failed :" + Environment.NewLine + Environment.NewLine
                For i = 0 To keys.Count - 1
                    errorMessage += keys(i) + " : " + SNs(keys(i)) + Environment.NewLine
                Next
                MessageBox.Show(errorMessage)

            End If


        End Sub

        Private Sub btnUnlock_Click(sender As Object, e As EventArgs) Handles btnUnlock.Click
            Dim AlreadyUnlocked As Boolean = True

            Try
                Dim folderPath As String = "C:\Radiant Vision Systems Data\TrueTest\Master AppData"
                Dim adminUserName = Environment.UserName
                Dim ds As DirectorySecurity = Directory.GetAccessControl(folderPath)
                Dim fsa As FileSystemAccessRule = New FileSystemAccessRule(adminUserName, FileSystemRights.FullControl, AccessControlType.Deny)
                Dim fSecurity As AuthorizationRuleCollection =
                ds.GetAccessRules(True, True,
                Type.GetType("System.Security.Principal.NTAccount"))

                For Each myacc As Security.AccessControl.AccessRule In fSecurity

                    If (myacc.AccessControlType = AccessControlType.Deny) Then
                        AlreadyUnlocked = False
                        Exit For
                    End If
                Next
            Catch ex As Exception

            End Try


            If AlreadyUnlocked Then
                MessageBox.Show("Already Unlocked")
            Else
                Dim passwordForm = New Password
                passwordForm.ShowDialog()
                password = passwordForm.password
                If password.ToLower = "rvskorea1234" Then
                    Try
                        Dim folderPath As String = "C:\Radiant Vision Systems Data\TrueTest\Master AppData"
                        Dim adminUserName = Environment.UserName
                        Dim ds As DirectorySecurity = Directory.GetAccessControl(folderPath)
                        Dim fsa As FileSystemAccessRule = New FileSystemAccessRule(adminUserName, FileSystemRights.FullControl, AccessControlType.Deny)
                        ds.RemoveAccessRule(fsa)
                        Directory.SetAccessControl(folderPath, ds)
                        MessageBox.Show("Unlocked")
                        password = ""
                    Catch ex As Exception
                        MessageBox.Show(ex.Message)
                    End Try
                ElseIf password = "" Then
                Else
                    MessageBox.Show("Incorrect password, try again !")
                End If
            End If

        End Sub
    End Class
End Namespace
