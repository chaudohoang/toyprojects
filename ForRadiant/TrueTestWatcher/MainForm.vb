Imports System
Imports System.Windows.Forms
Imports System.Runtime.InteropServices
Imports System.Reflection
Imports System.IO
Imports System.Data.SqlServerCe
Imports System.Diagnostics
Imports System.Collections.Generic
Imports System.Management
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Xml
Imports System.Data
Imports System.Linq
Imports System.ComponentModel
Imports System.Drawing

Namespace TrueTestWatcher
    Partial Public Class MainForm
        Inherits Form
        Dim apppath As String
        Dim appdir As String
        Private allowVisible As Boolean = True
        Dim conn As SqlCeConnection
        Dim CameraSN As String
        Dim exePath As String = My.Application.Info.DirectoryPath
        Dim runningSequencePath As String
        Dim masterSequencePath As String
        Dim masterCalibrationPath As String
        Dim parameterNG As Boolean
        Dim calibrationNG As Boolean
        Dim appdataNG As Boolean
        Dim watchingTask As Tasks.Task
        Dim TasksCancellationTokenSource As New CancellationTokenSource
        Dim needtoCheck As Boolean
        Dim MasterStatusPath As String = "C:\Radiant Vision Systems Data\TrueTest\UserData\MasterStatus.txt"
        Dim watchPath1 As String = "C:\Radiant Vision Systems Data\TrueTest\Sequence"
        Dim watchPath2 As String = "C:\Radiant Vision Systems Data\TrueTest\Sequence\Master"
        Dim watchPath3 As String = "C:\Radiant Vision Systems Data\TrueTest\Sequence\Calibration"
        Dim watchPath4 As String = "C:\Radiant Vision Systems Data\TrueTest\AppData"
        Dim watchPath5 As String = "C:\Radiant Vision Systems Data\TrueTest\Master AppData"
        Public watchfolder As FileSystemWatcher

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
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = False
            SetVersionInfo()
            startTask()
            watchFolder1()
            watchFolder2()
            watchFolder3()
            watchFolder4()
            watchFolder5()
            If StartMinimizedToolStripMenuItem.Checked = True Then
                Me.WindowState = FormWindowState.Minimized
            End If
        End Sub
        Private Sub StartWatching()
            Dim newProcs As New Dictionary(Of String, Process)
            While True
                For Each process In System.Diagnostics.Process.GetProcesses()
                    If process.ProcessName.CompareTo("TrueTest") = 0 Then
                        If Not searchForProcess(newProcs, process.Id) Then
                            Dim searcher As ManagementObjectSearcher = New ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " & process.Id.ToString())
                            For Each [object] As ManagementObject In searcher.[Get]()
                                CommLogUpdateText("----------Process Started: ----------")
                                CommLogUpdateText([object]("CommandLine").ToString() & " ")

                                newProcs.Add([object]("CommandLine").ToString() & " ", process)
                            Next
                            If newProcs.Count <> 0 Then
                                needtoCheck = True
                            End If
                        End If
                    End If
                Next
                checkProcesses(newProcs)
                compareAll()

            End While
        End Sub

        Private Function searchForProcess(newProcs As Dictionary(Of String, Process), newKey As Integer) As Boolean
            For Each process As Process In newProcs.Values
                If process.Id = newKey Then Return True
            Next

            Return False
        End Function

        Private Sub checkProcesses(newProcs As Dictionary(Of String, Process))
            For Each currProc In newProcs.Keys
                Dim processExists = False
                For Each process In System.Diagnostics.Process.GetProcesses()
                    If process.Id = newProcs(currProc).Id Then
                        processExists = True
                        Exit For
                    End If

                Next
                If Not processExists Then
                    CommLogUpdateText("Process Closed: ")
                    CommLogUpdateText(currProc)

                    newProcs.Remove(currProc)
                    If newProcs.Count = 0 Then Exit For
                End If
            Next
        End Sub
        Private Sub watchFolder1()
            watchfolder = New System.IO.FileSystemWatcher()

            'this is the path we want to monitor
            watchfolder.Path = watchPath1

            'Add a list of Filter we want to specify
            'make sure you use OR for each Filter as we need to
            'all of those 

            watchfolder.NotifyFilter = IO.NotifyFilters.DirectoryName
            watchfolder.NotifyFilter = watchfolder.NotifyFilter Or
                           IO.NotifyFilters.FileName
            watchfolder.NotifyFilter = watchfolder.NotifyFilter Or
                           IO.NotifyFilters.Attributes

            ' add the handler to each event
            AddHandler watchfolder.Changed, AddressOf logchange1
            AddHandler watchfolder.Created, AddressOf logchange1
            AddHandler watchfolder.Deleted, AddressOf logchange1

            ' add the rename handler as the signature is different
            AddHandler watchfolder.Renamed, AddressOf logrename1

            'Set this property to true to start watching
            watchfolder.EnableRaisingEvents = True

            'End of code for btn_start_click
        End Sub

        Private Sub watchFolder2()
            watchfolder = New System.IO.FileSystemWatcher()

            'this is the path we want to monitor
            watchfolder.Path = watchPath2

            'Add a list of Filter we want to specify
            'make sure you use OR for each Filter as we need to
            'all of those 

            watchfolder.NotifyFilter = IO.NotifyFilters.DirectoryName
            watchfolder.NotifyFilter = watchfolder.NotifyFilter Or
                           IO.NotifyFilters.FileName
            watchfolder.NotifyFilter = watchfolder.NotifyFilter Or
                           IO.NotifyFilters.Attributes

            ' add the handler to each event
            AddHandler watchfolder.Changed, AddressOf logchange2
            AddHandler watchfolder.Created, AddressOf logchange2
            AddHandler watchfolder.Deleted, AddressOf logchange2

            ' add the rename handler as the signature is different
            AddHandler watchfolder.Renamed, AddressOf logrename2

            'Set this property to true to start watching
            watchfolder.EnableRaisingEvents = True

            'End of code for btn_start_click
        End Sub

        Private Sub watchFolder3()
            watchfolder = New System.IO.FileSystemWatcher()

            'this is the path we want to monitor
            watchfolder.Path = watchPath3

            'Add a list of Filter we want to specify
            'make sure you use OR for each Filter as we need to
            'all of those 

            watchfolder.NotifyFilter = IO.NotifyFilters.DirectoryName
            watchfolder.NotifyFilter = watchfolder.NotifyFilter Or
                           IO.NotifyFilters.FileName
            watchfolder.NotifyFilter = watchfolder.NotifyFilter Or
                           IO.NotifyFilters.Attributes

            ' add the handler to each event
            AddHandler watchfolder.Changed, AddressOf logchange3
            AddHandler watchfolder.Created, AddressOf logchange3
            AddHandler watchfolder.Deleted, AddressOf logchange3

            ' add the rename handler as the signature is different
            AddHandler watchfolder.Renamed, AddressOf logrename3

            'Set this property to true to start watching
            watchfolder.EnableRaisingEvents = True

            'End of code for btn_start_click
        End Sub

        Private Sub watchFolder4()
            watchfolder = New System.IO.FileSystemWatcher()

            'this is the path we want to monitor
            watchfolder.Path = watchPath4

            'Add a list of Filter we want to specify
            'make sure you use OR for each Filter as we need to
            'all of those 

            watchfolder.NotifyFilter = IO.NotifyFilters.DirectoryName
            watchfolder.NotifyFilter = watchfolder.NotifyFilter Or
                           IO.NotifyFilters.FileName
            watchfolder.NotifyFilter = watchfolder.NotifyFilter Or
                           IO.NotifyFilters.Attributes

            ' add the handler to each event
            AddHandler watchfolder.Changed, AddressOf logchange4
            AddHandler watchfolder.Created, AddressOf logchange4
            AddHandler watchfolder.Deleted, AddressOf logchange4

            ' add the rename handler as the signature is different
            AddHandler watchfolder.Renamed, AddressOf logrename4

            'Set this property to true to start watching
            watchfolder.EnableRaisingEvents = True

            'End of code for btn_start_click
        End Sub

        Private Sub watchFolder5()
            watchfolder = New System.IO.FileSystemWatcher()

            'this is the path we want to monitor
            watchfolder.Path = watchPath5

            'Add a list of Filter we want to specify
            'make sure you use OR for each Filter as we need to
            'all of those 

            watchfolder.NotifyFilter = IO.NotifyFilters.DirectoryName
            watchfolder.NotifyFilter = watchfolder.NotifyFilter Or
                           IO.NotifyFilters.FileName
            watchfolder.NotifyFilter = watchfolder.NotifyFilter Or
                           IO.NotifyFilters.Attributes

            ' add the handler to each event
            AddHandler watchfolder.Changed, AddressOf logchange5
            AddHandler watchfolder.Created, AddressOf logchange5
            AddHandler watchfolder.Deleted, AddressOf logchange5

            ' add the rename handler as the signature is different
            AddHandler watchfolder.Renamed, AddressOf logrename5

            'Set this property to true to start watching
            watchfolder.EnableRaisingEvents = True

            'End of code for btn_start_click
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
        Private Delegate Sub UpdateCommLogDelegate(text As String)
        Private Sub CommLogUpdateText(text As String)
            If InvokeRequired Then BeginInvoke(New UpdateCommLogDelegate(AddressOf CommLogUpdateText), New Object() {text}) : Exit Sub
            If text <> vbCrLf Then
                ListBox1.Items.Add(Now.ToString("yyyyMMdd HH:mm:ss") + " : " + text)
            Else
                ListBox1.Items.Add(text)
            End If
            ListBox1.TopIndex = ListBox1.Items.Count - 1
        End Sub

        Private Sub btnClearlog_Click(sender As Object, e As EventArgs) Handles btnClearlog.Click
            ListBox1.Items.Clear()
        End Sub

        Private Sub btnExportAnalysesCompareLog_Click(sender As Object, e As EventArgs) Handles btnExportAnalysesCompareLog.Click
            Dim logPath = Path.Combine(exePath, DateTime.Now.ToString("yyyyMMddHHmmss") + "_Activity.txt")
            If ListBox1.Items.Count = 0 Then
                MessageBox.Show("Empty Log !!!")
            Else
                Dim savefile As SaveFileDialog = New SaveFileDialog()
                ' set a default file name
                savefile.FileName = Path.GetFileName(logPath)
                ' set filters - this can be done in properties as well
                savefile.Filter = "Text files (*.txt)|*.txt"
                savefile.InitialDirectory = exePath

                If savefile.ShowDialog() = DialogResult.OK Then
                    Using sw As StreamWriter = New StreamWriter(savefile.FileName)
                        sw.WriteLine("Running Sequence : " + runningSequencePath)
                        sw.WriteLine("Master Sequence : " + masterSequencePath)
                        sw.WriteLine("Master Calibration : " + masterSequencePath)
                        sw.WriteLine("Ignore List : " + cbxIgnoreList.Text)

                        For index = 0 To ListBox1.Items.Count - 1
                            sw.WriteLine(ListBox1.Items(index))
                        Next
                    End Using
                    Process.Start("notepad", savefile.FileName)
                End If
            End If
        End Sub
        Private Sub startTask()
            TasksCancellationTokenSource = New CancellationTokenSource
            watchingTask = New Tasks.Task(New Action(Sub() StartWatching()), TasksCancellationTokenSource.Token)
            watchingTask.Start()
        End Sub

        Public Sub CheckForMatchingSequenceParameters(ByVal file1FullPath As String, ByVal file2FullPath As String)
            parameterNG = False
            If Not chkCompareSequenceParameters.Checked Then
                CommLogUpdateText("SKIPPED PARAMETERS COMPARISION !!!")
                Exit Sub
            End If
            Dim timeLogString As New List(Of String)
            Dim tempString As String = ""

            Dim sw As New Stopwatch
            sw.Start()
            Dim equal As Boolean = True
            Dim log As New List(Of String)

            Dim sw2 As New Stopwatch
            sw2.Start()

            If Not File.Exists(file1FullPath) Then
                equal = False
                CommLogUpdateText("Parameters Check : Running Sequence is not existed !!!")
                parameterNG = True
                Exit Sub
            ElseIf Not File.Exists(file2FullPath) Then
                equal = False
                CommLogUpdateText("Parameters Check : Master Master Sequence is not existed !!!")
                parameterNG = True
                Exit Sub

            ElseIf file1FullPath = file2FullPath Then
                equal = False
                CommLogUpdateText("Parameters Check : Running Sequence and Master Sequence is the same file !!!")
                parameterNG = True
                Exit Sub
            End If
            sw2.Stop()
            timeLogString.Add("Check sequence files existence : " + sw2.ElapsedMilliseconds.ToString + "ms")

            sw2.Restart()
            Dim ignoreList As List(Of String)

            ignoreList = (From s In cbxIgnoreList.Text.Split(CChar(","))
                          Select s).ToList()

            sw2.Stop()
            timeLogString.Add("Get ignore parameters : " + sw2.ElapsedMilliseconds.ToString + "ms")
            If cbxIgnoreList.Text <> "" Then timeLogString.Add(cbxIgnoreList.Text)

            Dim sequence1AnaList As New List(Of String)
            Dim sequence2AnaList As New List(Of String)

            Dim node1 As XmlNode
            Dim nodes1 As XmlNodeList

            Dim node2 As XmlNode
            Dim nodes2 As XmlNodeList

            Dim xmlDoc1 = New XmlDocument()
            Dim xmlDoc2 = New XmlDocument()

            Dim fileloaded As Boolean
            While Not fileloaded
                Try
                    xmlDoc1.Load(file1FullPath)
                    xmlDoc2.Load(file2FullPath)
                    fileloaded = True
                Catch ex As Exception
                    fileloaded = False
                End Try
            End While

            nodes1 = xmlDoc1.DocumentElement.SelectNodes("/Sequence/Items/SequenceItem")
            nodes2 = xmlDoc2.DocumentElement.SelectNodes("/Sequence/Items/SequenceItem")

            sw2.Restart()
            'conpare sequence analysis list
            For i1 = 0 To nodes1.Count - 1
                If nodes1(i1).SelectSingleNode("Selected").InnerText.ToLower = "true" Then
                    sequence1AnaList.Add(nodes1(i1).SelectSingleNode("PatternSetupName").InnerText + "_" + (nodes1(i1).SelectSingleNode("Analysis/UserName").InnerText))
                End If
            Next
            sw2.Stop()
            timeLogString.Add("Get Running Sequence analysis list : " + sw2.ElapsedMilliseconds.ToString + "ms")
            timeLogString.Add("Running Sequence analysis : " + String.Join(",", sequence1AnaList))

            sw2.Restart()
            For i2 = 0 To nodes2.Count - 1
                If nodes2(i2).SelectSingleNode("Selected").InnerText.ToLower = "true" Then
                    sequence2AnaList.Add(nodes2(i2).SelectSingleNode("PatternSetupName").InnerText + "_" + (nodes2(i2).SelectSingleNode("Analysis/UserName").InnerText))
                End If
            Next

            sw2.Stop()
            timeLogString.Add("Get Master Sequence analysis list : " + sw2.ElapsedMilliseconds.ToString + "ms")
            timeLogString.Add("Master Sequence analysis : " + String.Join(",", sequence2AnaList))


            sw2.Restart()

            If String.Join(",", sequence1AnaList) <> String.Join(",", sequence2AnaList) Then
                equal = False
                CommLogUpdateText("Parameters Check : Analysis list does not match !!!")
                CommLogUpdateText("Parameters Check : Running Sequence analyses : " + String.Join(",", sequence1AnaList))
                CommLogUpdateText("Parameters Check : Master Sequence analyses : " + String.Join(",", sequence2AnaList))
                Exit Sub
            End If
            sw2.Stop()
            timeLogString.Add("Check if 2 sequences having same number of analysis : " + sw2.ElapsedMilliseconds.ToString + "ms" + Environment.NewLine)

            Dim SequenceItemCount As Integer
            If nodes1.Count < nodes2.Count Then
                SequenceItemCount = nodes1.Count
            Else
                SequenceItemCount = nodes2.Count
            End If
            For index = 0 To SequenceItemCount - 1
                If Not nodes1(index).SelectSingleNode("Selected").InnerText.ToLower = "true" Or Not nodes2(index).SelectSingleNode("Selected").InnerText.ToLower = "true" Then
                    Continue For
                End If
                node1 = nodes1(index).SelectSingleNode("Analysis")
                node2 = nodes2(index).SelectSingleNode("Analysis")

                Dim seq1AnalysisName = nodes1(index).SelectSingleNode("PatternSetupName").InnerText + "_" + nodes1(index).SelectSingleNode("Analysis/UserName").InnerText
                Dim seq2AnalysisName = nodes2(index).SelectSingleNode("PatternSetupName").InnerText + "_" + nodes2(index).SelectSingleNode("Analysis/UserName").InnerText
                timeLogString.Add("Checking step : " + seq1AnalysisName)
                sw2.Restart()
                'Remove items in ignoreList

                For Each item As String In ignoreList
                    For Each childNode1 As XmlNode In node1.ChildNodes
                        If childNode1.Name.ToLower = item.ToLower Then
                            node1.RemoveChild(childNode1)
                            tempString = tempString + childNode1.Name + ","
                        End If
                    Next
                Next
                sw2.Stop()
                timeLogString.Add("Running Sequence remove ignored parameters in Analysis " + seq1AnalysisName + " and removed " + If(tempString.Length = 0, "nothing", tempString.Trim().Remove(tempString.Length - 1)) + " : " + sw2.ElapsedMilliseconds.ToString + "ms")
                tempString = ""
                sw2.Restart()
                For Each item As String In ignoreList
                    For Each childNode2 As XmlNode In node2.ChildNodes
                        If childNode2.Name.ToLower = item.ToLower Then
                            node2.RemoveChild(childNode2)
                            tempString = tempString + childNode2.Name + ","
                        End If
                    Next
                Next
                sw2.Stop()
                timeLogString.Add("Master Sequence remove ignored parameters in Analysis " + seq2AnalysisName + " and removed " + If(tempString.Length = 0, "nothing", tempString.Trim().Remove(tempString.Length - 1)) + " : " + sw2.ElapsedMilliseconds.ToString + "ms")
                tempString = ""
                sw2.Restart()
                For childindex = node1.ChildNodes.Count - 1 To 0 Step -1
                    If node2.SelectSingleNode(node1.ChildNodes(childindex).Name) Is Nothing Then
                        tempString = tempString + node1.ChildNodes(childindex).Name + ","
                        node1.RemoveChild(node1.ChildNodes(childindex))
                    End If
                Next
                sw2.Stop()
                Dim seq2NeedSorting As Boolean
                seq2NeedSorting = If(tempString.Length = 0, False, True)
                timeLogString.Add("Running Sequence removing extra element in Analysis " + seq1AnalysisName + " and removed " + If(tempString.Length = 0, "nothing", tempString.Trim().Remove(tempString.Length - 1)) + " : " + sw2.ElapsedMilliseconds.ToString + "ms")
                tempString = ""

                sw2.Restart()
                For childIndex = node2.ChildNodes.Count - 1 To 0 Step -1
                    If node1.SelectSingleNode(node2.ChildNodes(childIndex).Name) Is Nothing Then
                        tempString = tempString + node2.ChildNodes(childIndex).Name + ","
                        node2.RemoveChild(node2.ChildNodes(childIndex))
                    End If
                Next
                sw2.Stop()
                Dim seq1NeedSorting As Boolean
                seq1NeedSorting = If(tempString.Length = 0, False, True)
                timeLogString.Add("Master Sequence removing extra element in Analysis " + seq2AnalysisName + " and removed " + If(tempString.Length = 0, "nothing", tempString.Trim().Remove(tempString.Length - 1)) + " : " + sw2.ElapsedMilliseconds.ToString + "ms")
                tempString = ""

                sw2.Restart()

                For childIndex = 0 To node1.ChildNodes.Count - 1
                    If node1.ChildNodes(childIndex).Name = "FilterList" AndAlso node1.ChildNodes(childIndex).SelectSingleNode("FilterList") IsNot Nothing AndAlso node2.ChildNodes(childIndex).SelectSingleNode("FilterList") IsNot Nothing Then

                        Dim filterFileName1 As String = node1.ChildNodes(childIndex).SelectSingleNode("FilterList/MoireFilter/MoireFilterList").InnerText
                        Dim filterFileName2 As String = node2.ChildNodes(childIndex).SelectSingleNode("FilterList/MoireFilter/MoireFilterList").InnerText

                        If filterFileName1 <> filterFileName2 Then
                            equal = False
                            parameterNG = True
                            log.Add("Step : " + seq1AnalysisName + ", Parameter : " + node1.ChildNodes(childIndex).Name + ", Running Sequence Value : " + filterFileName1 + ", Master Sequence Value : " + filterFileName2)
                        End If

                    Else
                        If node1.ChildNodes(childIndex).InnerText.ToLower <> node2.ChildNodes(childIndex).InnerText.ToLower Then
                            equal = False
                            parameterNG = True
                            log.Add("Step : " + seq1AnalysisName + ", Parameter : " + node1.ChildNodes(childIndex).Name + ", Running Sequence Value : " + node1.ChildNodes(childIndex).InnerText + ", Master Sequence Value : " + node2.ChildNodes(childIndex).InnerText)
                        End If
                    End If

                    tempString = tempString + node1.ChildNodes(childIndex).Name + ","
                Next
                sw2.Stop()

                timeLogString.Add("Comparing done for analysis step : " + seq1AnalysisName + " : " + sw2.ElapsedMilliseconds.ToString + "ms")
                timeLogString.Add("Compared " + node1.ChildNodes.Count.ToString + " parameters : " + If(tempString.Length = 0, "nothing", tempString.Trim().Remove(tempString.Length - 1)) + Environment.NewLine)
                tempString = ""
            Next

            If equal Then
                CommLogUpdateText("NO PARAMETER DIFFERENCE !!!")
            Else
                parameterNG = True
                CommLogUpdateText("FOUND PARAMETER DIFFERENCE !!!")
            End If

            sw.Stop()
            timeLogString.Add("Total time : " + sw.ElapsedMilliseconds.ToString + "ms")

            For i = 0 To log.Count - 1
                CommLogUpdateText(log(i))
            Next

            CommLogUpdateText("Compare Parameters Time : " + (sw.ElapsedMilliseconds / 1000).ToString + "s")
            'File.WriteAllLines(Path.Combine(Path.GetTempPath(), "Parameter Compare Time.txt"), timeLogString)
            'Process.Start("notepad.exe", Path.Combine(Path.GetTempPath(), "Parameter Compare Time.txt"))

        End Sub

        Public Sub CompareCalSettings(ByVal file1FullPath As String, ByVal file2FullPath As String)
            calibrationNG = False
            If Not chkCompareSequenceCalibrations.Checked Then
                CommLogUpdateText("SKIPPED CALIBRATIONS COMPARISION !!!")
                Exit Sub
            End If
            Dim timeLogString As New List(Of String)
            Dim tempString As String = ""
            Dim sw As New Stopwatch
            sw.Start()
            Dim equal As Boolean = True
            Dim log As New List(Of String)
            Dim sw2 As New Stopwatch
            sw2.Start()

            If Not File.Exists(file1FullPath) Then
                equal = False
                calibrationNG = True
                CommLogUpdateText("Calibrations Check : Running Sequence is not existed !!!")
                Exit Sub
            ElseIf Not File.Exists(file2FullPath) Then
                equal = False
                calibrationNG = True
                CommLogUpdateText("Calibrations Check : Master Calibration is not existed !!!")
                Exit Sub
            ElseIf file1FullPath = file2FullPath Then
                equal = False
                calibrationNG = True
                CommLogUpdateText("Calibrations Check : Running Sequence and Master Calibration is the same file !!!")
                Exit Sub
            End If
            sw2.Stop()
            timeLogString.Add("Check sequence files existence : " + sw2.ElapsedMilliseconds.ToString + "ms")
            Dim SN1 As String = ""
            Dim SN2 As String = ""

            Dim ColorCalSetting1 As New List(Of String)
            Dim ColorCalSetting2 As New List(Of String)
            Dim ImgScaleSetting1 As New List(Of String)
            Dim ImgScaleSetting2 As New List(Of String)
            Dim FFCSetting1 As New List(Of String)
            Dim FFCSetting2 As New List(Of String)

            Dim sequence1AnaList As New List(Of String)
            Dim sequence2AnaList As New List(Of String)

            Dim demuraStepList1 As New List(Of String)
            Dim demuraStepList2 As New List(Of String)

            Dim node1 As XmlNode
            Dim nodes1 As XmlNodeList

            Dim node2 As XmlNode
            Dim nodes2 As XmlNodeList

            Dim xmlDoc1 = New XmlDocument()
            Dim xmlDoc2 = New XmlDocument()
            xmlDoc1.Load(file1FullPath)
            xmlDoc2.Load(file2FullPath)

            nodes1 = xmlDoc1.DocumentElement.SelectNodes("/Sequence/Items/SequenceItem")
            nodes2 = xmlDoc2.DocumentElement.SelectNodes("/Sequence/Items/SequenceItem")
            sw2.Restart()

            For i = 0 To nodes1.Count - 1
                If nodes1(i).SelectSingleNode("Selected").InnerText.ToLower = "true" Then
                    sequence1AnaList.Add(nodes1(i).SelectSingleNode("PatternSetupName").InnerText)
                End If
                If (nodes1(i).SelectSingleNode("Analysis").Attributes("xsi:type").Value.Contains("DemuraLGDCustomerAnalysis") Or nodes1(i).SelectSingleNode("Analysis").Attributes("xsi:type").Value.Contains("DemuraLGDNCustomerAnalysis")) AndAlso
                Not nodes1(i).SelectSingleNode("Analysis").Attributes("xsi:type").Value.Contains("DemuraLGDNPOCB4p1") Then
                    demuraStepList1.Add(nodes1(i).SelectSingleNode("PatternSetupName").InnerText)
                End If
            Next
            sw2.Stop()
            timeLogString.Add("Get Running Sequence analysis list and demura step list (to skip demura step) : " + sw2.ElapsedMilliseconds.ToString + "ms")

            sw2.Restart()
            nodes1 = xmlDoc1.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup")
            For index = 0 To nodes1.Count - 1

                If sequence1AnaList.Contains(nodes1(index).SelectSingleNode("Name").InnerText) Then
                    If demuraStepList1.Contains(nodes1(index).SelectSingleNode("Name").InnerText) Then Continue For
                    node1 = nodes1(index).SelectSingleNode("CameraSettingsList")
                    For Each childNode As XmlNode In node1.ChildNodes
                        Dim lastChild As XmlNode = node1.LastChild.Clone()
                        node1.RemoveAll()
                        node1.AppendChild(lastChild)
                    Next
                    Dim CCID = node1.SelectSingleNode("CameraSettings/ColorCalID").InnerText
                    Dim IMCID = node1.SelectSingleNode("CameraSettings/ImageScalingCalibrationID").InnerText
                    Dim FFID = node1.SelectSingleNode("CameraSettings/FlatFieldID").InnerText
                    SN1 = node1.SelectSingleNode("CameraSettings/SerialNumber").InnerText
                    ColorCalSetting1.Add(SN1 + "," + nodes1(index).SelectSingleNode("Name").InnerText + "," + CCID)
                    ImgScaleSetting1.Add(SN1 + "," + nodes1(index).SelectSingleNode("Name").InnerText + "," + IMCID)
                    FFCSetting1.Add(SN1 + "," + nodes1(index).SelectSingleNode("Name").InnerText + "," + FFID)
                    'log.Add("SN : " + SN1 + " , Step : " + nodes1(index).SelectSingleNode("Name").InnerText + " , ColorCalID : " + CCID + " , ImageScalingID : " + IMCID + " , FlatFieldID : " + FFID)
                End If
            Next
            sw2.Stop()
            timeLogString.Add("Get Running Sequence Serial Number : " + sw2.ElapsedMilliseconds.ToString + "ms")

            sw2.Restart()

            For i = 0 To nodes2.Count - 1
                If nodes2(i).SelectSingleNode("Selected").InnerText.ToLower = "true" Then
                    sequence2AnaList.Add(nodes2(i).SelectSingleNode("PatternSetupName").InnerText)
                End If
                If (nodes2(i).SelectSingleNode("Analysis").Attributes("xsi:type").Value.Contains("DemuraLGDCustomerAnalysis") Or nodes2(i).SelectSingleNode("Analysis").Attributes("xsi:type").Value.Contains("DemuraLGDNCustomerAnalysis")) AndAlso
                Not nodes2(i).SelectSingleNode("Analysis").Attributes("xsi:type").Value.Contains("DemuraLGDNPOCB4p1") Then
                    demuraStepList2.Add(nodes2(i).SelectSingleNode("PatternSetupName").InnerText)
                End If
            Next
            sw2.Stop()
            timeLogString.Add("Get Master Calibration analysis list and demura step list (to skip demura step) : " + sw2.ElapsedMilliseconds.ToString + "ms")

            sw2.Restart()
            nodes2 = xmlDoc2.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup")
            For index = 0 To nodes2.Count - 1
                If sequence2AnaList.Contains(nodes2(index).SelectSingleNode("Name").InnerText) Then
                    If demuraStepList2.Contains(nodes2(index).SelectSingleNode("Name").InnerText) Then Continue For
                    node2 = nodes2(index).SelectSingleNode("CameraSettingsList")
                    For Each childNode As XmlNode In node2.ChildNodes
                        Dim lastChild As XmlNode = node2.LastChild.Clone()
                        node2.RemoveAll()
                        node2.AppendChild(lastChild)
                    Next
                    Dim CCID = node2.SelectSingleNode("CameraSettings/ColorCalID").InnerText
                    Dim IMCID = node2.SelectSingleNode("CameraSettings/ImageScalingCalibrationID").InnerText
                    Dim FFID = node2.SelectSingleNode("CameraSettings/FlatFieldID").InnerText
                    SN2 = node2.SelectSingleNode("CameraSettings/SerialNumber").InnerText
                    ColorCalSetting2.Add(SN2 + "," + nodes2(index).SelectSingleNode("Name").InnerText + "," + CCID)
                    ImgScaleSetting2.Add(SN2 + "," + nodes2(index).SelectSingleNode("Name").InnerText + "," + IMCID)
                    FFCSetting2.Add(SN2 + "," + nodes2(index).SelectSingleNode("Name").InnerText + "," + FFID)
                    'log.Add("SN : " + SN2 + " , Step : " + nodes1(index).SelectSingleNode("Name").InnerText + " , ColorCalID : " + CCID + " , ImageScalingID : " + IMCID + " , FlatFieldID : " + FFID)
                End If
            Next
            sw2.Stop()
            timeLogString.Add("Get Master Calibration Serial Number : " + sw2.ElapsedMilliseconds.ToString + "ms")

            If SN1 <> SN2 Then
                calibrationNG = True
                CommLogUpdateText("Running Sequence and master sequence is from different camera, cannot compare calibration")
                Exit Sub
            Else

            End If

            Dim colorCalRef1 As New Dictionary(Of String, String)
            Dim colorCalRef2 As New Dictionary(Of String, String)

            sw2.Restart()
            GetColorCalRef(file1FullPath, colorCalRef1)
            GetColorCalRef(file2FullPath, colorCalRef2)
            sw2.Stop()
            timeLogString.Add("Get color calibrations from cal files : " + sw2.ElapsedMilliseconds.ToString + "ms")

            Dim imgScaleRef1 As New Dictionary(Of String, String)
            Dim imgScaleRef2 As New Dictionary(Of String, String)

            sw2.Restart()
            GetIMGScaleCalRef(file1FullPath, imgScaleRef1)
            GetIMGScaleCalRef(file2FullPath, imgScaleRef2)
            sw2.Stop()
            timeLogString.Add("Get image scaling calibrations from cal files : " + sw2.ElapsedMilliseconds.ToString + "ms")

            Dim flatFieldRef1 As New Dictionary(Of String, String)
            Dim flatFieldRef2 As New Dictionary(Of String, String)

            sw2.Restart()
            GetFFCCalRef(file1FullPath, flatFieldRef1)
            GetFFCCalRef(file2FullPath, flatFieldRef2)
            sw2.Stop()
            timeLogString.Add("Get flat field calibrations from cal files : " + sw2.ElapsedMilliseconds.ToString + "ms")

            sw2.Restart()
            For index = 0 To ColorCalSetting1.Count - 1
                If ColorCalSetting1(index) <> ColorCalSetting2(index) Then
                    equal = False
                    calibrationNG = True
                    log.Add("Step : " + ColorCalSetting1(index).Split(",")(1) + ", Running Sequence Color Cal : " + colorCalRef1(ColorCalSetting1(index).Split(",")(2)) + ", Master Calibration Color Cal : " + colorCalRef2(ColorCalSetting2(index).Split(",")(2)))
                End If
            Next
            sw2.Stop()
            timeLogString.Add("Done checking color calibrations : " + sw2.ElapsedMilliseconds.ToString + "ms")

            sw2.Restart()
            For index = 0 To ImgScaleSetting1.Count - 1
                If ImgScaleSetting1(index) <> ImgScaleSetting2(index) Then
                    equal = False
                    calibrationNG = True
                    log.Add("Step : " + ImgScaleSetting1(index).Split(",")(1) + ", Running Sequence Img Scale Cal : " + imgScaleRef1(ImgScaleSetting1(index).Split(",")(2)) + ", Master Calibration Img Scale Cal : " + imgScaleRef2(ImgScaleSetting2(index).Split(",")(2)))
                End If
            Next
            sw2.Stop()
            timeLogString.Add("Done checking image scaling calibrations : " + sw2.ElapsedMilliseconds.ToString + "ms")

            sw2.Restart()
            For index = 0 To FFCSetting1.Count - 1
                If FFCSetting1(index) <> FFCSetting2(index) Then
                    equal = False
                    calibrationNG = True
                    log.Add("Step : " + FFCSetting1(index).Split(",")(1) + ", Running Sequence FFC Cal : " + flatFieldRef1(FFCSetting1(index).Split(",")(2)) + ", Master Calibration FFC Cal : " + flatFieldRef2(FFCSetting2(index).Split(",")(2)))
                End If
            Next
            sw2.Stop()
            timeLogString.Add("Done checking flat field calibrations : " + sw2.ElapsedMilliseconds.ToString + "ms")

            If equal Then
                CommLogUpdateText("NO CAL DIFFERENCE !!!")
            Else
                calibrationNG = True
                CommLogUpdateText("FOUND CAL DIFFERENCE !!!")
            End If

            sw.Stop()
            timeLogString.Add("Total time : " + sw.ElapsedMilliseconds.ToString + "ms")

            For i = 0 To log.Count - 1
                CommLogUpdateText(log(i))
            Next

            CommLogUpdateText("Compare Calibrations Time : " + (sw.ElapsedMilliseconds / 1000).ToString + "s")
            'File.WriteAllLines(Path.Combine(Path.GetTempPath(), "Calibration Compare Time.txt"), timeLogString)
            'Process.Start("notepad.exe", Path.Combine(Path.GetTempPath(), "Calibration Compare Time.txt"))

        End Sub

        Private Sub CompareAppDataFiles()
            appdataNG = False
            If Not chkCompareAppdataFiles.Checked Then
                CommLogUpdateText("SKIPPED APPDATA COMPARISION !!!")
                Exit Sub
            End If
            Dim sw As New Stopwatch
            sw.Start()
            Dim AppDataFolder As String = "C:\Radiant Vision Systems Data\TrueTest\AppData"
            Dim MasterAppDataFolder As String = "C:\Radiant Vision Systems Data\TrueTest\Master AppData"
            If Not Directory.Exists(MasterAppDataFolder) Then
                Directory.CreateDirectory(MasterAppDataFolder)
            End If
            Dim files As IEnumerable(Of String) = IO.Directory.EnumerateFiles(MasterAppDataFolder)
            If files.Count = 0 Then
                CommLogUpdateText("No files in Master AppData folder to compare ! ")
                Exit Sub
            End If
            Dim runningFileMissing As Boolean
            Dim ngFiles As New List(Of String)
            Dim okFiles As New List(Of String)
            For Each masterFile As String In files
                Dim filename = Path.GetFileName(masterFile)
                Dim runningFile As String = Path.Combine(AppDataFolder, filename)
                If Not File.Exists(runningFile) Then
                    runningFileMissing = True
                    appdataNG = True
                    CommLogUpdateText("NG AppData file : " + runningFile + " is not existed or deleted !")
                    Continue For
                End If
                If {"xml", "csv", "txt"}.Contains(Path.GetExtension(masterFile).Remove(0, 1)) Then
                    Dim compareProcess As Process = New Process
                    Dim startinfo As System.Diagnostics.ProcessStartInfo = New System.Diagnostics.ProcessStartInfo()
                    startinfo.FileName = System.IO.Path.Combine(My.Application.Info.DirectoryPath, "WinMergeU.exe")
                    startinfo.Arguments = "-noninteractive -minimize -enableexitcode -cfg Settings/DiffContextV2=0 " + Chr(34) + runningFile + Chr(34) + " " + Chr(34) + masterFile + Chr(34)
                    compareProcess = Process.Start(startinfo)

                    If compareProcess.WaitForExit(15000) Then
                        Dim ExitCode As Integer = compareProcess.ExitCode
                        If ExitCode <> 0 Then
                            ngFiles.Add(runningFile)
                        Else
                            okFiles.Add(runningFile)
                        End If
                    Else
                        appdataNG = True
                        CommLogUpdateText("Timed out comparing appdata files")
                    End If

                ElseIf {"sdf"}.Contains(Path.GetExtension(masterFile).Remove(0, 1)) Then
                    Dim masterInfo As FileInfo = New FileInfo(masterFile)
                    Dim runningInfo As FileInfo = New FileInfo(runningFile)
                    Dim masterSize As Long = masterInfo.Length
                    Dim runningSize As Long = runningInfo.Length
                    If masterSize <> runningSize Then
                        ngFiles.Add(runningFile)
                    Else
                        okFiles.Add(runningFile)
                    End If
                End If

            Next
            If okFiles.Count > 0 Then
                For Each item As String In okFiles
                    CommLogUpdateText("OK AppData file : " + item)
                Next
            End If
            If ngFiles.Count > 0 Or runningFileMissing Then
                appdataNG = True
                For Each item As String In ngFiles
                    CommLogUpdateText("NG AppData file : " + item)
                Next
                CommLogUpdateText("Checking Appdata File Finished, NG")
            Else
                CommLogUpdateText("Checking Appdata File Finished, OK")
            End If
            sw.Stop()
            CommLogUpdateText("Elapsed time : " + (sw.ElapsedMilliseconds / 1000).ToString + " seconds.")
        End Sub

        Public Sub GetColorCalRef(SequencePath As String, ByRef RefDict As Dictionary(Of String, String))
            Dim conn As SqlCeConnection
            Dim cmdColorCalibration As New SqlCeCommand
            Dim daColorCalibration As New SqlCeDataAdapter
            Dim dsColorCalibration As New DataSet
            Dim SN As String = ""
            Dim sequenceAnaList As New List(Of String)
            Dim xmlDoc = New XmlDocument()
            Dim node As XmlNode
            Dim nodes As XmlNodeList
            xmlDoc.Load(SequencePath)
            nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/Items/SequenceItem")
            For i = 0 To nodes.Count - 1
                If nodes(i).SelectSingleNode("Selected").InnerText.ToLower = "true" Then
                    sequenceAnaList.Add(nodes(i).SelectSingleNode("PatternSetupName").InnerText)
                End If
            Next
            nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup")

            For index = 0 To nodes.Count - 1
                If sequenceAnaList.Contains(nodes(index).SelectSingleNode("Name").InnerText) Then

                    node = nodes(index).SelectSingleNode("CameraSettingsList")
                    For Each childNode As XmlNode In node.ChildNodes
                        Dim lastChild As XmlNode = node.LastChild.Clone()
                        node.RemoveAll()
                        node.AppendChild(lastChild)
                    Next
                    SN = node.SelectSingleNode("CameraSettings/SerialNumber").InnerText
                End If
            Next
            Try
                dsColorCalibration.Clear()
                conn = GetConnect(SN)
                cmdColorCalibration = conn.CreateCommand
                cmdColorCalibration.CommandText = "SELECT ColorCalibrationID, Description FROM ColorCalibrations"
                daColorCalibration.SelectCommand = cmdColorCalibration
                daColorCalibration.Fill(dsColorCalibration, "ColorCalibrations")
                Dim newNoneRow = dsColorCalibration.Tables("ColorCalibrations").NewRow()
                newNoneRow(0) = "0"
                newNoneRow(1) = "(None)"
                dsColorCalibration.Tables("ColorCalibrations").Rows.InsertAt(newNoneRow, 0)
                Dim newFactoryRow = dsColorCalibration.Tables("ColorCalibrations").NewRow()
                newFactoryRow(0) = "-1"
                newFactoryRow(1) = "Factory"
                dsColorCalibration.Tables("ColorCalibrations").Rows.InsertAt(newFactoryRow, 0)
            Catch ex As Exception
                CommLogUpdateText("Error: " & ex.Source & ": " & ex.Message & "Connection Error !!")
                Exit Sub
            End Try

            For Each Row As DataRow In dsColorCalibration.Tables("ColorCalibrations").Rows
                RefDict.Add(Row(0), Row(1))
            Next
        End Sub

        Public Sub GetIMGScaleCalRef(SequencePath As String, ByRef RefDict As Dictionary(Of String, String))
            Dim conn As SqlCeConnection
            Dim cmdImgScaleCalibration As New SqlCeCommand
            Dim daImgScaleCalibration As New SqlCeDataAdapter
            Dim dsImgScaleCalibration As New DataSet
            Dim SN As String = ""
            Dim sequenceAnaList As New List(Of String)
            Dim xmlDoc = New XmlDocument()
            Dim node As XmlNode
            Dim nodes As XmlNodeList
            xmlDoc.Load(SequencePath)
            nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/Items/SequenceItem")
            For i = 0 To nodes.Count - 1
                If nodes(i).SelectSingleNode("Selected").InnerText.ToLower = "true" Then
                    sequenceAnaList.Add(nodes(i).SelectSingleNode("PatternSetupName").InnerText)
                End If
            Next
            nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup")

            For index = 0 To nodes.Count - 1
                If sequenceAnaList.Contains(nodes(index).SelectSingleNode("Name").InnerText) Then

                    node = nodes(index).SelectSingleNode("CameraSettingsList")
                    For Each childNode As XmlNode In node.ChildNodes
                        Dim lastChild As XmlNode = node.LastChild.Clone()
                        node.RemoveAll()
                        node.AppendChild(lastChild)
                    Next
                    SN = node.SelectSingleNode("CameraSettings/SerialNumber").InnerText
                End If
            Next
            Try
                dsImgScaleCalibration.Clear()
                conn = GetConnect(SN)
                cmdImgScaleCalibration = conn.CreateCommand
                cmdImgScaleCalibration.CommandText = "SELECT ImageScalingCalibrationID, ImageScalingCalibrationDesc FROM ImageScalingCalibration"
                daImgScaleCalibration.SelectCommand = cmdImgScaleCalibration
                daImgScaleCalibration.Fill(dsImgScaleCalibration, "ImageScalingCalibration")
                Dim newNoneRow = dsImgScaleCalibration.Tables("ImageScalingCalibration").NewRow()
                newNoneRow(0) = "0"
                newNoneRow(1) = "(None)"
                dsImgScaleCalibration.Tables("ImageScalingCalibration").Rows.InsertAt(newNoneRow, 0)
                Dim newFactoryRow = dsImgScaleCalibration.Tables("ImageScalingCalibration").NewRow()
                newFactoryRow(0) = "-1"
                newFactoryRow(1) = "Factory"
                dsImgScaleCalibration.Tables("ImageScalingCalibration").Rows.InsertAt(newFactoryRow, 0)
            Catch ex As Exception
                CommLogUpdateText("Error: " & ex.Source & ": " & ex.Message & "Connection Error !!")
                Exit Sub
            End Try

            For Each Row As DataRow In dsImgScaleCalibration.Tables("ImageScalingCalibration").Rows
                RefDict.Add(Row(0), Row(1))
            Next
        End Sub

        Public Sub GetFFCCalRef(SequencePath As String, ByRef RefDict As Dictionary(Of String, String))
            Dim conn As SqlCeConnection
            Dim cmdFlatFieldCalibration As New SqlCeCommand
            Dim daImgScaleCalibration As New SqlCeDataAdapter
            Dim dsFlatFieldCalibration As New DataSet
            Dim SN As String = ""
            Dim sequenceAnaList As New List(Of String)
            Dim xmlDoc = New XmlDocument()
            Dim node As XmlNode
            Dim nodes As XmlNodeList
            xmlDoc.Load(SequencePath)
            nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/Items/SequenceItem")
            For i = 0 To nodes.Count - 1
                If nodes(i).SelectSingleNode("Selected").InnerText.ToLower = "true" Then
                    sequenceAnaList.Add(nodes(i).SelectSingleNode("PatternSetupName").InnerText)
                End If
            Next
            nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup")

            For index = 0 To nodes.Count - 1
                If sequenceAnaList.Contains(nodes(index).SelectSingleNode("Name").InnerText) Then

                    node = nodes(index).SelectSingleNode("CameraSettingsList")
                    For Each childNode As XmlNode In node.ChildNodes
                        Dim lastChild As XmlNode = node.LastChild.Clone()
                        node.RemoveAll()
                        node.AppendChild(lastChild)
                    Next
                    SN = node.SelectSingleNode("CameraSettings/SerialNumber").InnerText
                End If
            Next
            Try
                dsFlatFieldCalibration.Clear()
                conn = GetConnect(SN)
                cmdFlatFieldCalibration = conn.CreateCommand
                cmdFlatFieldCalibration.CommandText = "SELECT CalibrationID, CalibrationDesc FROM FlatFieldCalibration"
                daImgScaleCalibration.SelectCommand = cmdFlatFieldCalibration
                daImgScaleCalibration.Fill(dsFlatFieldCalibration, "FlatFieldCalibration")
                Dim newNoneRow = dsFlatFieldCalibration.Tables("FlatFieldCalibration").NewRow()
                newNoneRow(0) = "0"
                newNoneRow(1) = "(None)"
                dsFlatFieldCalibration.Tables("FlatFieldCalibration").Rows.InsertAt(newNoneRow, 0)
                Dim newFactoryRow = dsFlatFieldCalibration.Tables("FlatFieldCalibration").NewRow()
                newFactoryRow(0) = "-1"
                newFactoryRow(1) = "Factory"
                dsFlatFieldCalibration.Tables("FlatFieldCalibration").Rows.InsertAt(newFactoryRow, 0)
            Catch ex As Exception
                CommLogUpdateText("Error: " & ex.Source & ": " & ex.Message & "Connection Error !!")
                Exit Sub
            End Try

            For Each Row As DataRow In dsFlatFieldCalibration.Tables("FlatFieldCalibration").Rows
                RefDict.Add(Row(0), Row(1))
            Next
        End Sub

        Public Function GetConnect(ByVal CameraSN)
            Dim conn As SqlCeConnection
            Dim calFile1 As String = Path.Combine("C:\Radiant Vision Systems Data\Camera Data\Calibration Files", CameraSN + "_CalibrationDB.calx")
            Dim calFile2 As String = Path.Combine("C:\Radiant Vision Systems Data\Camera Data\Calibration Files", "0" + CameraSN + "_CalibrationDB.calx")
            If File.Exists(calFile1) Then
                conn = New SqlCeConnection("Data Source=" + calFile1 + ";Max Database Size=4091")
            ElseIf File.Exists(calFile2) Then
                conn = New SqlCeConnection("Data Source=" + calFile2 + ";Max Database Size=4091")
            Else
                conn = New SqlCeConnection("Data Source=C:\Radiant Vision Systems Data\Camera Data\Calibration Files\PM Calibration Demo Camera.calx;Max Database Size=4091")
            End If

            Return conn
        End Function

        Private Sub compareAll()
            If Not needtoCheck Then
                Exit Sub
            End If
            CommLogUpdateText("Performing Master Functions Check !!!")

            getSequenceFilePath()
            CheckForMatchingSequenceParameters(runningSequencePath, masterSequencePath)

            CompareCalSettings(runningSequencePath, masterCalibrationPath)

            CompareAppDataFiles()

            CommLogUpdateText("Finished Master Functions Check !!!")

            needtoCheck = False

            Dim status As String = ""
            If Not parameterNG AndAlso Not calibrationNG AndAlso Not appdataNG Then
                status = "OK"
            ElseIf parameterNG Then
                status = "Parameter Missmatch"
            ElseIf calibrationNG Then
                status = "Calibration Missmatch"
            ElseIf appdataNG Then
                status = "Appdata Missmatch"
            End If
            File.WriteAllText(MasterStatusPath, status)
            CommLogUpdateText("Wrote Master Status " + Chr(34) + status + Chr(34) + " to : " + MasterStatusPath + " !!!")

        End Sub

        Private Sub getSequenceFilePath()
            Dim node1 As XmlNode
            Dim xmlDoc1 = New XmlDocument()

            xmlDoc1.Load("C:\Radiant Vision Systems Data\TrueTest\AppData\1.8\app.settings")
            node1 = xmlDoc1.DocumentElement.SelectSingleNode("/Settings/LastSequenceFile")
            runningSequencePath = node1.InnerText
            masterSequencePath = Path.Combine("C:\Radiant Vision Systems Data\TrueTest\Sequence\Master", Path.GetFileName(runningSequencePath))
            masterCalibrationPath = Path.Combine("C:\Radiant Vision Systems Data\TrueTest\Sequence\Calibration", Path.GetFileName(runningSequencePath))
            CommLogUpdateText("Running Sequence file path : " + runningSequencePath)
            CommLogUpdateText("Master sequence file path : " + masterSequencePath)
            CommLogUpdateText("Master calibraion file path : " + masterCalibrationPath)
        End Sub

        Private Sub SetControlEnabled(ByVal ctl As Control, ByVal enabled As Boolean)
            If ctl.InvokeRequired Then
                ctl.BeginInvoke(New Action(Of Control, Boolean)(AddressOf SetControlEnabled), ctl, enabled)
            Else
                ctl.Enabled = enabled
            End If
        End Sub

        Private Sub logchange1(ByVal source As Object, ByVal e As _
                        System.IO.FileSystemEventArgs)
            Dim Extensions() As String = {".seqxc"}
            If Extensions.Contains(Path.GetExtension(e.FullPath)) Then
                If e.ChangeType = IO.WatcherChangeTypes.Changed Then
                    CommLogUpdateText("----------File " & e.FullPath & " has been modified----------")

                End If
                If e.ChangeType = IO.WatcherChangeTypes.Created Then
                    CommLogUpdateText("----------File " & e.FullPath & " has been created----------")

                End If
                If e.ChangeType = IO.WatcherChangeTypes.Deleted Then
                    CommLogUpdateText("----------File " & e.FullPath & " has been deleted----------")

                End If
                CommLogUpdateText("Performing Master Sequence and Calibration Check !!!")

                getSequenceFilePath()
                CheckForMatchingSequenceParameters(runningSequencePath, masterSequencePath)

                CompareCalSettings(runningSequencePath, masterCalibrationPath)

                CommLogUpdateText("Finished Master Sequence and Calibration Check !!!")

                Dim status As String = ""
                If Not parameterNG AndAlso Not calibrationNG Then
                    status = "OK"
                ElseIf parameterNG Then
                    status = "Parameter Missmatch"
                ElseIf calibrationNG Then
                    status = "Calibration Missmatch"
                End If
                File.WriteAllText(MasterStatusPath, status)
                CommLogUpdateText("Wrote Master Status " + Chr(34) + status + Chr(34) + " to : " + MasterStatusPath + " !!!")

            End If

        End Sub

        Public Sub logrename1(ByVal source As Object, ByVal e As _
                            System.IO.RenamedEventArgs)
            Dim Extensions() As String = {".seqxc"}
            If Extensions.Contains(Path.GetExtension(e.FullPath)) Then
                CommLogUpdateText("----------File" & e.OldName & " has been renamed to " & e.Name & "----------")

                CommLogUpdateText("Performing Master Sequence and Calibration Check !!!")

                getSequenceFilePath()
                CheckForMatchingSequenceParameters(runningSequencePath, masterSequencePath)

                CompareCalSettings(runningSequencePath, masterCalibrationPath)

                CommLogUpdateText("Finished Master Sequence and Calibration Check !!!")

                Dim status As String = ""
                If Not parameterNG AndAlso Not calibrationNG Then
                    status = "OK"
                ElseIf parameterNG Then
                    status = "Parameter Missmatch"
                ElseIf calibrationNG Then
                    status = "Calibration Missmatch"
                End If
                File.WriteAllText(MasterStatusPath, status)
                CommLogUpdateText("Wrote Master Status " + Chr(34) + status + Chr(34) + " to : " + MasterStatusPath + " !!!")

            End If

        End Sub

        Private Sub logchange2(ByVal source As Object, ByVal e As _
                        System.IO.FileSystemEventArgs)
            Dim Extensions() As String = {".seqxc"}
            If Extensions.Contains(Path.GetExtension(e.FullPath)) Then
                If e.ChangeType = IO.WatcherChangeTypes.Changed Then
                    CommLogUpdateText("----------File " & e.FullPath & " has been modified----------")

                End If
                If e.ChangeType = IO.WatcherChangeTypes.Created Then
                    CommLogUpdateText("----------File " & e.FullPath & " has been created----------")

                End If
                If e.ChangeType = IO.WatcherChangeTypes.Deleted Then
                    CommLogUpdateText("----------File " & e.FullPath & " has been deleted----------")

                End If
                CommLogUpdateText("Performing Master Sequence and Calibration Check !!!")

                getSequenceFilePath()
                CheckForMatchingSequenceParameters(runningSequencePath, masterSequencePath)

                CompareCalSettings(runningSequencePath, masterCalibrationPath)

                CommLogUpdateText("Finished Master Sequence and Calibration Check !!!")

                Dim status As String = ""
                If Not parameterNG AndAlso Not calibrationNG Then
                    status = "OK"
                ElseIf parameterNG Then
                    status = "Parameter Missmatch"
                ElseIf calibrationNG Then
                    status = "Calibration Missmatch"
                End If
                File.WriteAllText(MasterStatusPath, status)
                CommLogUpdateText("Wrote Master Status " + Chr(34) + status + Chr(34) + " to : " + MasterStatusPath + " !!!")

            End If

        End Sub

        Public Sub logrename2(ByVal source As Object, ByVal e As _
                            System.IO.RenamedEventArgs)
            Dim Extensions() As String = {".seqxc"}
            If Extensions.Contains(Path.GetExtension(e.FullPath)) Then
                CommLogUpdateText("----------File" & e.OldName & " has been renamed to " & e.Name & "----------")

                CommLogUpdateText("Performing Master Sequence and Calibration Check !!!")

                getSequenceFilePath()
                CheckForMatchingSequenceParameters(runningSequencePath, masterSequencePath)

                CompareCalSettings(runningSequencePath, masterCalibrationPath)

                CommLogUpdateText("Finished Master Sequence and Calibration Check !!!")

                Dim status As String = ""
                If Not parameterNG AndAlso Not calibrationNG Then
                    status = "OK"
                ElseIf parameterNG Then
                    status = "Parameter Missmatch"
                ElseIf calibrationNG Then
                    status = "Calibration Missmatch"
                End If
                File.WriteAllText(MasterStatusPath, status)
                CommLogUpdateText("Wrote Master Status " + Chr(34) + status + Chr(34) + " to : " + MasterStatusPath + " !!!")

            End If

        End Sub

        Private Sub logchange3(ByVal source As Object, ByVal e As _
                        System.IO.FileSystemEventArgs)
            Dim Extensions() As String = {".seqxc"}
            If Extensions.Contains(Path.GetExtension(e.FullPath)) Then
                If e.ChangeType = IO.WatcherChangeTypes.Changed Then
                    CommLogUpdateText("----------File " & e.FullPath & " has been modified----------")

                End If
                If e.ChangeType = IO.WatcherChangeTypes.Created Then
                    CommLogUpdateText("----------File " & e.FullPath & " has been created----------")

                End If
                If e.ChangeType = IO.WatcherChangeTypes.Deleted Then
                    CommLogUpdateText("----------File " & e.FullPath & " has been deleted----------")

                End If
                CommLogUpdateText("Performing Master Sequence and Calibration Check !!!")

                getSequenceFilePath()
                CheckForMatchingSequenceParameters(runningSequencePath, masterSequencePath)

                CompareCalSettings(runningSequencePath, masterCalibrationPath)

                CommLogUpdateText("Finished Master Sequence and Calibration Check !!!")

                Dim status As String = ""
                If Not parameterNG AndAlso Not calibrationNG Then
                    status = "OK"
                ElseIf parameterNG Then
                    status = "Parameter Missmatch"
                ElseIf calibrationNG Then
                    status = "Calibration Missmatch"
                End If
                File.WriteAllText(MasterStatusPath, status)
                CommLogUpdateText("Wrote Master Status " + Chr(34) + status + Chr(34) + " to : " + MasterStatusPath + " !!!")

            End If

        End Sub

        Public Sub logrename3(ByVal source As Object, ByVal e As _
                            System.IO.RenamedEventArgs)
            Dim Extensions() As String = {".seqxc"}
            If Extensions.Contains(Path.GetExtension(e.FullPath)) Then
                CommLogUpdateText("----------File" & e.OldName & " has been renamed to " & e.Name & "----------")

                CommLogUpdateText("Performing Master Sequence and Calibration Check !!!")

                getSequenceFilePath()
                CheckForMatchingSequenceParameters(runningSequencePath, masterSequencePath)

                CompareCalSettings(runningSequencePath, masterCalibrationPath)

                CommLogUpdateText("Finished Master Sequence and Calibration Check !!!")

                Dim status As String = ""
                If Not parameterNG AndAlso Not calibrationNG Then
                    status = "OK"
                ElseIf parameterNG Then
                    status = "Parameter Missmatch"
                ElseIf calibrationNG Then
                    status = "Calibration Missmatch"
                End If
                File.WriteAllText(MasterStatusPath, status)
                CommLogUpdateText("Wrote Master Status " + Chr(34) + status + Chr(34) + " to : " + MasterStatusPath + " !!!")

            End If

        End Sub

        Private Sub logchange4(ByVal source As Object, ByVal e As _
                        System.IO.FileSystemEventArgs)
            Dim Extensions() As String = {".xml", ".csv"}
            If Extensions.Contains(Path.GetExtension(e.FullPath)) Or Path.GetFileNameWithoutExtension(e.FullPath).Contains("RegisterPixels") Then
                If e.ChangeType = IO.WatcherChangeTypes.Changed Then
                    CommLogUpdateText("----------File " & e.FullPath & " has been modified----------")

                End If
                If e.ChangeType = IO.WatcherChangeTypes.Created Then
                    CommLogUpdateText("----------File " & e.FullPath & " has been created----------")

                End If
                If e.ChangeType = IO.WatcherChangeTypes.Deleted Then
                    CommLogUpdateText("----------File " & e.FullPath & " has been deleted----------")

                End If
                CommLogUpdateText("Performing Master AppData Check !!!")

                CompareAppDataFiles()

                CommLogUpdateText("Finished Master AppData Check !!!")

                Dim status As String = ""
                If Not appdataNG Then
                    status = "OK"
                Else
                    status = "Appdata Missmatch"
                End If
                File.WriteAllText(MasterStatusPath, status)
                CommLogUpdateText("Wrote Master Status " + Chr(34) + status + Chr(34) + " to : " + MasterStatusPath + " !!!")

            End If
        End Sub

        Public Sub logrename4(ByVal source As Object, ByVal e As _
                            System.IO.RenamedEventArgs)
            Dim Extensions() As String = {".xml", ".csv"}
            If Extensions.Contains(Path.GetExtension(e.FullPath)) Or Path.GetFileNameWithoutExtension(e.FullPath).Contains("RegisterPixels") Then
                CommLogUpdateText("----------File" & e.OldName & " has been renamed to " & e.Name & "----------")

                CommLogUpdateText("Performing Master AppData Check !!!")

                CompareAppDataFiles()

                CommLogUpdateText("Finished Master AppData Check !!!")

                Dim status As String = ""
                If Not appdataNG Then
                    status = "OK"
                Else
                    status = "Appdata Missmatch"
                End If
                File.WriteAllText(MasterStatusPath, status)
                CommLogUpdateText("Wrote Master Status " + Chr(34) + status + Chr(34) + " to : " + MasterStatusPath + " !!!")

            End If
        End Sub

        Private Sub logchange5(ByVal source As Object, ByVal e As _
                        System.IO.FileSystemEventArgs)
            Dim Extensions() As String = {".xml", ".csv"}
            If Extensions.Contains(Path.GetExtension(e.FullPath)) Or Path.GetFileNameWithoutExtension(e.FullPath).Contains("RegisterPixels") Then
                If e.ChangeType = IO.WatcherChangeTypes.Changed Then
                    CommLogUpdateText("----------File " & e.FullPath & " has been modified----------")

                End If
                If e.ChangeType = IO.WatcherChangeTypes.Created Then
                    CommLogUpdateText("----------File " & e.FullPath & " has been created----------")

                End If
                If e.ChangeType = IO.WatcherChangeTypes.Deleted Then
                    CommLogUpdateText("----------File " & e.FullPath & " has been deleted----------")

                End If
                CommLogUpdateText("Performing Master AppData Check !!!")

                CompareAppDataFiles()

                CommLogUpdateText("Finished Master AppData Check !!!")

                Dim status As String = ""
                If Not appdataNG Then
                    status = "OK"
                Else
                    status = "Appdata Missmatch"
                End If
                File.WriteAllText(MasterStatusPath, status)
                CommLogUpdateText("Wrote Master Status " + Chr(34) + status + Chr(34) + " to : " + MasterStatusPath + " !!!")

            End If
        End Sub

        Public Sub logrename5(ByVal source As Object, ByVal e As _
                            System.IO.RenamedEventArgs)
            Dim Extensions() As String = {".xml", ".csv"}
            If Extensions.Contains(Path.GetExtension(e.FullPath)) Or Path.GetFileNameWithoutExtension(e.FullPath).Contains("RegisterPixels") Then
                CommLogUpdateText("----------File" & e.OldName & " has been renamed to " & e.Name & "----------")

                CommLogUpdateText("Performing Master AppData Check !!!")

                CompareAppDataFiles()

                CommLogUpdateText("Finished Master AppData Check !!!")

                Dim status As String = ""
                If Not appdataNG Then
                    status = "OK"
                Else
                    status = "Appdata Missmatch"
                End If
                File.WriteAllText(MasterStatusPath, status)
                CommLogUpdateText("Wrote Master Status " + Chr(34) + status + Chr(34) + " to : " + MasterStatusPath + " !!!")

            End If
        End Sub

        Private Sub ManualCheckToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ManualCheckToolStripMenuItem.Click
            CommLogUpdateText("Performing Master Functions Check !!!")

            getSequenceFilePath()
            CheckForMatchingSequenceParameters(runningSequencePath, masterSequencePath)

            CompareCalSettings(runningSequencePath, masterCalibrationPath)

            CompareAppDataFiles()

            CommLogUpdateText("Finished Master Functions Check !!!")

            Dim status As String = ""
            If Not parameterNG AndAlso Not calibrationNG AndAlso Not appdataNG Then
                status = "OK"
            ElseIf parameterNG Then
                status = "Parameter Missmatch"
            ElseIf calibrationNG Then
                status = "Calibration Missmatch"
            ElseIf appdataNG Then
                status = "Appdata Missmatch"
            End If
            File.WriteAllText(MasterStatusPath, status)
            CommLogUpdateText("Wrote Master Status " + Chr(34) + status + Chr(34) + " to : " + MasterStatusPath + " !!!")
        End Sub
    End Class
End Namespace
