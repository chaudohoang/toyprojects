Imports System.Diagnostics.Eventing
Imports System.IO
Imports System.Drawing
Imports System.Windows.Forms
Imports System.Reflection

Public Class PanelFFCX10801panel
    Private Sub SetVersionInfo()

        Dim ass As System.Reflection.Assembly = System.Reflection.Assembly.GetExecutingAssembly()
        Dim ver As System.Version = ass.GetName().Version
        Dim startDate As DateTime = New Date(2000, 1, 1)
        Dim diffDays As Integer = ver.Build
        Dim computedDate As DateTime = startDate.AddDays(diffDays)
        Dim lastBuilt As String = computedDate.ToShortDateString()
        'Me.Text = (Me.Text & " " & ver.Major & "." & ver.Minor & "." & ver.Build & "." & ver.Revision & " (" & lastBuilt & ")")
        Me.Text = (Me.Text & " " & ver.Major & "." & ver.Minor & "." & ver.Build & "." & ver.Revision)

    End Sub

    Private Sub btnGenerate_Click(sender As Object, e As EventArgs) Handles btnGenerate.Click 'button Generate function
        btnGenerate.Enabled = False
        ' Create result folder
        Dim di As DirectoryInfo = New DirectoryInfo(outputpathbox.Text)
        di.Create()

        Dim B1, R1, G1, MB, MR, MG, DB, DR, DG As New List(Of List(Of Double))() 'create lists
        'load all txt file to the lists
        R1 = LoadFile(redfilepath1.Text)
        G1 = LoadFile(greenfilepath1.Text)
        B1 = LoadFile(bluefilepath1.Text)
        MR = LoadFile(mredfilepath.Text)
        MG = LoadFile(mgreenfilepath.Text)
        MB = LoadFile(mbluefilepath.Text)

        'calculate result list from the file
        DR = Calculate(R1, MR)
        DG = Calculate(G1, MG)
        DB = Calculate(B1, MB)

        'generate csv and xml
        GenerateCSV(DR, outputpathbox.Text + "\X1080_" + camerasn.Text + "_R.csv")
        GenerateCSV(DG, outputpathbox.Text + "\X1080_" + camerasn.Text + "_G.csv")
        GenerateCSV(DB, outputpathbox.Text + "\X1080_" + camerasn.Text + "_B.csv")
        GenerateXML(camerasn.Text, "R", outputpathbox.Text + "\X1080_1.xml")
        GenerateXML(camerasn.Text, "G", outputpathbox.Text + "\X1080_2.xml")
        GenerateXML(camerasn.Text, "B", outputpathbox.Text + "\X1080_3.xml")

        'just create Done! text with random color when finish, just like butterfly tool
        Static m_Rnd As New Random
        Dim tempcolor As Color
        tempcolor = Label20.ForeColor
        Do While Label20.ForeColor = tempcolor
            Label20.ForeColor = Color.FromArgb(255, m_Rnd.Next(0, 255), m_Rnd.Next(0, 255), m_Rnd.Next(0, 255))
        Loop

        Label20.Text = "Done !"
        btnGenerate.Enabled = True

    End Sub

    Private Function LoadFile(ByVal filePath As String) As List(Of List(Of Double)) 'load file function, load text file into 2 dimensional list
        Dim tempvalue As Double
        Dim records As New List(Of List(Of Double))()
        For Each line As String In File.ReadAllLines(filePath)
            Dim values As New List(Of Double)()
            For Each field As String In line.Split(New String() {ControlChars.Tab}, StringSplitOptions.None)
                Double.TryParse(field, tempvalue)
                'If tempvalue <> 0 Then
                values.Add(tempvalue)
                'End If
            Next
            If values.Count <> 0 Then
                records.Add(values)
            End If

        Next
        Return records
    End Function

    Private Function Calculate(ByVal list1 As List(Of List(Of Double)), ByVal mlist As List(Of List(Of Double))) As List(Of List(Of Double))
        'this function is to calculate the result by sum 5 lists and devide it to mother list

        Dim rowlength As New Integer
        Dim columnlength As New Integer
        rowlength = list1.Count
        columnlength = list1(0).Count
        Dim records As New List(Of List(Of Double))() 'define 2D list

        'this loop is to calculate the value in every row and then add the row to the result list
        For row As Integer = 0 To rowlength - 1
            Dim values As New List(Of Double)() ' define row list
            Dim tempvalue As New Double
            For column As Integer = 0 To columnlength - 1

                tempvalue = mlist(row)(column) / list1(row)(column)
                values.Add(tempvalue)
            Next

            records.Add(values)
        Next

        Return records
    End Function

    Private Sub GenerateCSV(ByVal inputlist As List(Of List(Of Double)), ByVal outputfile As String) 'function to write CSV file from the list

        Dim rowlength As New Integer
        Dim columnlength As New Integer
        Dim sw As New StreamWriter(outputfile)
        'count how many row and column from the input 2D list
        rowlength = inputlist.Count
        columnlength = inputlist(0).Count

        'write first line to CSV file
        sw.Write(columnlength)
        sw.Write(",")
        sw.Write(rowlength)
        For column As Integer = 2 To columnlength - 1
            sw.Write("")
            sw.Write(",")
        Next
        sw.WriteLine()

        'write all remaining data to CSV file
        For row As Integer = 0 To rowlength - 1

            For column As Integer = 0 To columnlength - 1

                If inputlist(row)(column) > 1 Then
                    sw.Write(inputlist(row)(column).ToString("#.######"))
                Else sw.Write(inputlist(row)(column).ToString("0.#######"))

                End If

                If column < columnlength - 1 Then
                    sw.Write(",")
                End If

            Next
            sw.WriteLine()
        Next
        sw.Close()
    End Sub

    Private Sub btnChooseInputFolder_Click(sender As Object, e As EventArgs) Handles btnChooseInputFolder.Click 'function to choose folder which contains all the files to load

        Using frm = New FolderBrowserDialog
            If frm.ShowDialog(Me) = DialogResult.OK Then


                inputpathbox.Text = frm.SelectedPath
                outputpathbox.Text = inputpathbox.Text + "\Result"

                Dim dir As New DirectoryInfo(inputpathbox.Text)
                Dim allFiles As IO.FileInfo() = dir.GetFiles()
                Dim singleFile As IO.FileInfo

                'this long loop is just to read all the file in the chosen folder, if the file name start with R and end with 1, the program will consider the file as red measurement of panel 1, and go on
                For Each singleFile In allFiles
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("R") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("1") Then
                        redfilepath1.Text = singleFile.FullName
                    End If
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("G") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("1") Then
                        greenfilepath1.Text = singleFile.FullName
                    End If
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("B") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("1") Then
                        bluefilepath1.Text = singleFile.FullName
                    End If
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("M") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("R") Then
                        mredfilepath.Text = singleFile.FullName
                    End If
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("M") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("G") Then
                        mgreenfilepath.Text = singleFile.FullName
                    End If
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("M") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("B") Then
                        mbluefilepath.Text = singleFile.FullName
                    End If

                Next

            End If
        End Using

    End Sub

    Private Sub GenerateXML(ByVal camerasn As String, ByVal color As String, ByVal outputfile As String) 'this function is to generate the xml based on the template below

        Dim xmlfile As New System.IO.StreamWriter(outputfile)

        xmlfile.WriteLine("<?xml version=""1.0"" encoding=""utf-8""?>")
        xmlfile.WriteLine("<LensCosineCorrection xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">")
        xmlfile.WriteLine("  <SerialNumber>" + camerasn + "</SerialNumber>")
        xmlfile.WriteLine("  <NormalFileName>X1080_" + camerasn + "_" + color + ".csv</NormalFileName>")
        xmlfile.WriteLine("  <NormalFileSmoothFilter>")
        xmlfile.WriteLine("    <Width>300</Width>")
        xmlfile.WriteLine("    <Height>300</Height>")
        xmlfile.WriteLine("  </NormalFileSmoothFilter>")
        xmlfile.WriteLine("  <CosineCorrection>0, 0</CosineCorrection>")
        xmlfile.WriteLine("  <BorderAttenuation>0,0,0,0,0,0,0,0</BorderAttenuation>")
        xmlfile.WriteLine("  <HorizontalGradient>0,0,0</HorizontalGradient>")
        xmlfile.WriteLine("  <VerticalGradient>0,0,0</VerticalGradient>")
        xmlfile.WriteLine("</LensCosineCorrection>")
        xmlfile.Close()

    End Sub

    Private Sub btnChooseRedFile1_Click(sender As Object, e As EventArgs) Handles btnChooseRedFile1.Click
        Using frm = New OpenFileDialog
            If frm.ShowDialog(Me) = DialogResult.OK Then
                redfilepath1.Text = frm.FileName
            End If
        End Using
    End Sub
    Private Sub btnChooseGreenFile1_Click(sender As Object, e As EventArgs) Handles btnChooseGreenFile1.Click
        Using frm = New OpenFileDialog
            If frm.ShowDialog(Me) = DialogResult.OK Then
                greenfilepath1.Text = frm.FileName
            End If
        End Using
    End Sub
    Private Sub btnChooseBlueFile1_Click(sender As Object, e As EventArgs) Handles btnChooseBlueFile1.Click
        Using frm = New OpenFileDialog
            If frm.ShowDialog(Me) = DialogResult.OK Then
                bluefilepath1.Text = frm.FileName
            End If
        End Using
    End Sub
    Private Sub btnChooseMotherRedFile_Click(sender As Object, e As EventArgs) Handles btnChooseMotherRedFile.Click
        Using frm = New OpenFileDialog
            If frm.ShowDialog(Me) = DialogResult.OK Then
                mredfilepath.Text = frm.FileName
            End If
        End Using
    End Sub
    Private Sub btnChooseMotherGreenFile_Click(sender As Object, e As EventArgs) Handles btnChooseMotherGreenFile.Click
        Using frm = New OpenFileDialog
            If frm.ShowDialog(Me) = DialogResult.OK Then
                mgreenfilepath.Text = frm.FileName
            End If
        End Using
    End Sub
    Private Sub btnChooseMotherBlueFile_Click(sender As Object, e As EventArgs) Handles btnChooseMotherBlueFile.Click, btnChooseOuputFolder.Click
        Using frm = New OpenFileDialog
            If frm.ShowDialog(Me) = DialogResult.OK Then
                mbluefilepath.Text = frm.FileName
            End If
        End Using
    End Sub
    Private Sub inputpathbox_TextChanged(sender As Object, e As EventArgs) Handles inputpathbox.TextChanged
        outputpathbox.Text = inputpathbox.Text + "\Result"

        Dim dir As New DirectoryInfo(inputpathbox.Text)
        Dim allFiles As IO.FileInfo() = dir.GetFiles()
        Dim singleFile As IO.FileInfo

        'this long loop is just to read all the file in the chosen folder, if the file name start with R and end with 1, the program will consider the file as red measurement of panel 1, and go on
        For Each singleFile In allFiles
            If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("R") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("1") Then
                redfilepath1.Text = singleFile.FullName
            End If
            If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("G") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("1") Then
                greenfilepath1.Text = singleFile.FullName
            End If
            If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("B") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("1") Then
                bluefilepath1.Text = singleFile.FullName
            End If
            If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("M") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("R") Then
                mredfilepath.Text = singleFile.FullName
            End If
            If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("M") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("G") Then
                mgreenfilepath.Text = singleFile.FullName
            End If
            If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("M") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("B") Then
                mbluefilepath.Text = singleFile.FullName
            End If

        Next
    End Sub

    Private Sub PanelFFCX10801panel_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        SetVersionInfo()
    End Sub
End Class


