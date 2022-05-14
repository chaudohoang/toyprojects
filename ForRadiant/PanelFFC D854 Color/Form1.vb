Imports System.Diagnostics.Eventing
Imports System.IO
Imports System.Drawing
Imports System.Windows.Forms
Imports System.IO.Compression
Imports System.Reflection

Public Class PanelFFCD854Color

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

        'Dim B1, B2, B3, B4, B5, R1, R2, R3, R4, R5, G1, G2, G3, G4, G5, MB, MR, MG, DB, DR, DG As New List(Of List(Of Double))() 'create lists
        Dim G1, G2, G3, G4, G5, MG, DG As New List(Of List(Of Double))() 'create lists
        'Dim G1, G2, G3, G4, MG, DG As New List(Of List(Of Double))() 'create lists

        'load all txt file to the lists
        'R1 = LoadFile(redfilepath1.Text)
        'R2 = LoadFile(redfilepath2.Text)
        'R3 = LoadFile(redfilepath3.Text)
        'R4 = LoadFile(redfilepath4.Text)
        'R5 = LoadFile(redfilepath5.Text)
        G1 = LoadFile(greenfilepath1.Text)
        G2 = LoadFile(greenfilepath2.Text)
        G3 = LoadFile(greenfilepath3.Text)
        G4 = LoadFile(greenfilepath4.Text)
        G5 = LoadFile(greenfilepath5.Text)
        'B1 = LoadFile(bluefilepath1.Text)
        'B2 = LoadFile(bluefilepath2.Text)
        'B3 = LoadFile(bluefilepath3.Text)
        'B4 = LoadFile(bluefilepath4.Text)
        'B5 = LoadFile(bluefilepath5.Text)
        'MR = LoadFile(mredfilepath.Text)
        MG = LoadFile(mgreenfilepath.Text)
        'MB = LoadFile(mbluefilepath.Text)

        'calculate result list from the file
        'DR = Calculate(R1, R2, R3, R4, R5, MR)
        DG = Calculate(G1, G2, G3, G4, G5, MG)

        'DB = Calculate(B1, B2, B3, B4, B5, MB)

        'generate csv and xml
        'GenerateCSV(DR, outputpathbox.Text + "\" + modelname.Text + "_" +camerasn.Text + "_R.csv")
        GenerateCSV(DG, outputpathbox.Text + "\" + modelname.Text + "_" + camerasn.Text + ".csv")
        'GenerateCSV(DB, outputpathbox.Text + "\" + modelname.Text + "_" + camerasn.Text + "_B.csv")
        'GenerateXML(camerasn.Text, "R", outputpathbox.Text + "\" + modelname.Text + ".xml")
        GenerateXML(camerasn.Text, "G", outputpathbox.Text + "\" + modelname.Text + ".xml")
        'GenerateXML(camerasn.Text, "B", outputpathbox.Text + "\" + modelname.Text + ".xml")

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
                If tempvalue <> 0 Then
                    values.Add(tempvalue)
                End If
            Next
            If values.Count <> 0 Then
                records.Add(values)
            End If

        Next
        Return records
    End Function

    Private Function Calculate(list1 As List(Of List(Of Double)), list2 As List(Of List(Of Double)), list3 As List(Of List(Of Double)), list4 As List(Of List(Of Double)), list5 As List(Of List(Of Double)), mlist As List(Of List(Of Double))) As List(Of List(Of Double))
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

                tempvalue = mlist(row)(column) / (list1(row)(column) + list2(row)(column) + list3(row)(column) + list4(row)(column) + list5(row)(column))
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
                Else sw.Write(inputlist(row)(column).ToString("#.#######"))

                End If

                sw.Write(",")

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
                    'If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("R") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("1") Then
                    '    redfilepath1.Text = singleFile.FullName
                    'End If
                    'If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("R") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("2") Then
                    '    redfilepath2.Text = singleFile.FullName
                    'End If
                    'If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("R") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("3") Then
                    '    redfilepath3.Text = singleFile.FullName
                    'End If
                    'If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("R") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("4") Then
                    '    redfilepath4.Text = singleFile.FullName
                    'End If
                    'if path.getfilenamewithoutextension(singlefile.fullname).ToUpper.StartsWith("r") and path.getfilenamewithoutextension(singlefile.fullname).ToUpper.EndsWith("5") then
                    '    redfilepath5.text = singlefile.fullname
                    'end if
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("G") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("1") Then
                        greenfilepath1.Text = singleFile.FullName
                    End If
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("G") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("2") Then
                        greenfilepath2.Text = singleFile.FullName
                    End If
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("G") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("3") Then
                        greenfilepath3.Text = singleFile.FullName
                    End If
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("G") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("4") Then
                        greenfilepath4.Text = singleFile.FullName
                    End If
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("G") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("5") Then
                        greenfilepath5.Text = singleFile.FullName
                    End If
                    'If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("B") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("1") Then
                    '    bluefilepath1.Text = singleFile.FullName
                    'End If
                    'If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("B") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("2") Then
                    '    bluefilepath2.Text = singleFile.FullName
                    'End If
                    'If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("B") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("3") Then
                    '    bluefilepath3.Text = singleFile.FullName
                    'End If
                    'If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("B") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("4") Then
                    '    bluefilepath4.Text = singleFile.FullName
                    'End If
                    'If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("B") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("5") Then
                    '    bluefilepath5.Text = singleFile.FullName
                    'End If
                    'If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("M") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("R") Then
                    '    mredfilepath.Text = singleFile.FullName
                    'End If
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("M") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("G") Then
                        mgreenfilepath.Text = singleFile.FullName
                    End If
                    'If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("M") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("B") Then
                    '    mbluefilepath.Text = singleFile.FullName
                    'End If

                Next

            End If
        End Using






    End Sub

    Private Sub GenerateXML(ByVal camerasn As String, ByVal color As String, ByVal outputfile As String) 'this function is to generate the xml based on the template below
        Dim xmlstring As String = txtXmlTemplate.Text
        xmlstring = xmlstring.Replace("camerasn", camerasn)
        xmlstring = xmlstring.Replace("model", modelname.Text)
        xmlstring = xmlstring.Replace("color", color)
        Dim xmlfile As New System.IO.StreamWriter(outputfile)

        xmlfile.Write(xmlstring)
        xmlfile.Close()

    End Sub

    'Private Sub btnChooseRedFile1_Click(sender As Object, e As EventArgs)
    '    Using frm = New OpenFileDialog
    '        If frm.ShowDialog(Me) = DialogResult.OK Then
    '            redfilepath1.Text = frm.FileName
    '        End If
    '    End Using
    'End Sub

    'Private Sub btnChooseRedFile2_Click(sender As Object, e As EventArgs)
    '    Using frm = New OpenFileDialog
    '        If frm.ShowDialog(Me) = DialogResult.OK Then
    '            redfilepath2.Text = frm.FileName
    '        End If
    '    End Using
    'End Sub

    'Private Sub btnChooseRedFile3_Click(sender As Object, e As EventArgs)
    '    Using frm = New OpenFileDialog
    '        If frm.ShowDialog(Me) = DialogResult.OK Then
    '            redfilepath3.Text = frm.FileName
    '        End If
    '    End Using
    'End Sub

    'Private Sub btnChooseRedFile4_Click(sender As Object, e As EventArgs)
    '    Using frm = New OpenFileDialog
    '        If frm.ShowDialog(Me) = DialogResult.OK Then
    '            redfilepath4.Text = frm.FileName
    '        End If
    '    End Using
    'End Sub

    'Private Sub btnChooseRedFile5_Click(sender As Object, e As EventArgs)
    '    Using frm = New OpenFileDialog
    '        If frm.ShowDialog(Me) = DialogResult.OK Then
    '            redfilepath5.Text = frm.FileName
    '        End If
    '    End Using
    'End Sub

    Private Sub btnChooseGreenFile1_Click(sender As Object, e As EventArgs) Handles btnChooseGreenFile1.Click
        Using frm = New OpenFileDialog
            If frm.ShowDialog(Me) = DialogResult.OK Then
                greenfilepath1.Text = frm.FileName
            End If
        End Using
    End Sub

    Private Sub btnChooseGreenFile2_Click(sender As Object, e As EventArgs) Handles btnChooseGreenFile2.Click
        Using frm = New OpenFileDialog
            If frm.ShowDialog(Me) = DialogResult.OK Then
                greenfilepath2.Text = frm.FileName
            End If
        End Using
    End Sub

    Private Sub btnChooseGreenFile3_Click(sender As Object, e As EventArgs) Handles btnChooseGreenFile3.Click
        Using frm = New OpenFileDialog
            If frm.ShowDialog(Me) = DialogResult.OK Then
                greenfilepath3.Text = frm.FileName
            End If
        End Using
    End Sub

    Private Sub btnChooseGreenFile4_Click(sender As Object, e As EventArgs) Handles btnChooseGreenFile4.Click
        Using frm = New OpenFileDialog
            If frm.ShowDialog(Me) = DialogResult.OK Then
                greenfilepath4.Text = frm.FileName
            End If
        End Using
    End Sub

    Private Sub btnChooseGreenFile5_Click(sender As Object, e As EventArgs) Handles btnChooseGreenFile5.Click
        Using frm = New OpenFileDialog
            If frm.ShowDialog(Me) = DialogResult.OK Then
                greenfilepath5.Text = frm.FileName
            End If
        End Using
    End Sub

    'Private Sub btnChooseBlueFile1_Click(sender As Object, e As EventArgs)
    '    Using frm = New OpenFileDialog
    '        If frm.ShowDialog(Me) = DialogResult.OK Then
    '            bluefilepath1.Text = frm.FileName
    '        End If
    '    End Using
    'End Sub

    'Private Sub btnChooseBlueFile2_Click(sender As Object, e As EventArgs)
    '    Using frm = New OpenFileDialog
    '        If frm.ShowDialog(Me) = DialogResult.OK Then
    '            bluefilepath2.Text = frm.FileName
    '        End If
    '    End Using
    'End Sub

    'Private Sub btnChooseBlueFile3_Click(sender As Object, e As EventArgs)
    '    Using frm = New OpenFileDialog
    '        If frm.ShowDialog(Me) = DialogResult.OK Then
    '            bluefilepath3.Text = frm.FileName
    '        End If
    '    End Using
    'End Sub

    'Private Sub btnChooseBlueFile4_Click(sender As Object, e As EventArgs)
    '    Using frm = New OpenFileDialog
    '        If frm.ShowDialog(Me) = DialogResult.OK Then
    '            bluefilepath4.Text = frm.FileName
    '        End If
    '    End Using
    'End Sub

    'Private Sub btnChooseBlueFile5_Click(sender As Object, e As EventArgs)
    '    Using frm = New OpenFileDialog
    '        If frm.ShowDialog(Me) = DialogResult.OK Then
    '            bluefilepath5.Text = frm.FileName
    '        End If
    '    End Using
    'End Sub

    'Private Sub btnChooseMotherRedFile_Click(sender As Object, e As EventArgs)
    '    Using frm = New OpenFileDialog
    '        If frm.ShowDialog(Me) = DialogResult.OK Then
    '            mredfilepath.Text = frm.FileName
    '        End If
    '    End Using
    'End Sub

    Private Sub btnChooseMotherGreenFile_Click(sender As Object, e As EventArgs) Handles btnChooseMotherGreenFile.Click
        Using frm = New OpenFileDialog
            If frm.ShowDialog(Me) = DialogResult.OK Then
                mgreenfilepath.Text = frm.FileName
            End If
        End Using
    End Sub

    'Private Sub btnChooseMotherBlueFile_Click(sender As Object, e As EventArgs) Handles btnChooseOuputFolder.Click
    '    Using frm = New OpenFileDialog
    '        If frm.ShowDialog(Me) = DialogResult.OK Then
    '            mbluefilepath.Text = frm.FileName
    '        End If
    '    End Using
    'End Sub

    Private Sub inputpathbox_TextChanged(sender As Object, e As EventArgs) Handles inputpathbox.TextChanged
        outputpathbox.Text = inputpathbox.Text + "\Result"

        Dim dir As New DirectoryInfo(inputpathbox.Text)
        Dim allFiles As IO.FileInfo() = dir.GetFiles()
        Dim singleFile As IO.FileInfo

        'this long loop is just to read all the file in the chosen folder, if the file name start with R and end with 1, the program will consider the file as red measurement of panel 1, and go on
        For Each singleFile In allFiles
            'If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("R") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("1") Then
            '    redfilepath1.Text = singleFile.FullName
            'End If
            'If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("R") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("2") Then
            '    redfilepath2.Text = singleFile.FullName
            'End If
            'If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("R") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("3") Then
            '    redfilepath3.Text = singleFile.FullName
            'End If
            'If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("R") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("4") Then
            '    redfilepath4.Text = singleFile.FullName
            'End If
            'If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("R") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("5") Then
            '    redfilepath5.Text = singleFile.FullName
            'End If
            If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("G") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("1") Then
                greenfilepath1.Text = singleFile.FullName
            End If
            If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("G") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("2") Then
                greenfilepath2.Text = singleFile.FullName
            End If
            If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("G") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("3") Then
                greenfilepath3.Text = singleFile.FullName
            End If
            If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("G") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("4") Then
                greenfilepath4.Text = singleFile.FullName
            End If
            If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("G") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("5") Then
                greenfilepath5.Text = singleFile.FullName
            End If
            'If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("B") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("1") Then
            '    bluefilepath1.Text = singleFile.FullName
            'End If
            'If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("B") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("2") Then
            '    bluefilepath2.Text = singleFile.FullName
            'End If
            'If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("B") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("3") Then
            '    bluefilepath3.Text = singleFile.FullName
            'End If
            'If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("B") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("4") Then
            '    bluefilepath4.Text = singleFile.FullName
            'End If
            'If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("B") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("5") Then
            '    bluefilepath5.Text = singleFile.FullName
            'End If
            'If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("M") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("R") Then
            '    mredfilepath.Text = singleFile.FullName
            'End If
            If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("M") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("G") Then
                mgreenfilepath.Text = singleFile.FullName
            End If
            'If Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.StartsWith("M") And Path.GetFileNameWithoutExtension(singleFile.FullName).ToUpper.EndsWith("B") Then
            '    mbluefilepath.Text = singleFile.FullName
            'End If

        Next
    End Sub

    Private Sub PanelFFCGRAY_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        SetVersionInfo()
        Dim rootpath As String = Assembly.GetEntryAssembly().Location
        Dim dir = Directory.GetParent(Directory.GetParent(rootpath).ToString()).ToString() + "\XmlTemplate"
        Dim files As New List(Of String)
        For Each file As String In System.IO.Directory.GetFiles(dir)
            If IO.Path.GetExtension(file) = ".xml" Then
                files.Add(System.IO.Path.GetFileNameWithoutExtension(file))
            End If
        Next
        files.Distinct.ToList()
        cbXmlTemplate.Items.Clear()
        For Each file As String In files
            cbXmlTemplate.Items.Add(file)
        Next
    End Sub

    Private Sub cbXmlTemplate_TextChanged(sender As Object, e As EventArgs) Handles cbXmlTemplate.TextChanged
        Dim rootpath As String = Assembly.GetEntryAssembly().Location
        Dim dir = Directory.GetParent(Directory.GetParent(rootpath).ToString()).ToString() + "\XmlTemplate"
        Dim fileReader As String
        Dim filePath As String = Path.GetFullPath(Path.Combine(dir, cbXmlTemplate.Text + ".xml"))
        If File.Exists(filePath) Then
            fileReader = My.Computer.FileSystem.ReadAllText(filePath)
            If fileReader <> "" Then
                txtXmlTemplate.Text = ""
                txtXmlTemplate.Text = fileReader
            End If
        End If
    End Sub

    Private Sub btnSaveXmlTemplate_Click(sender As Object, e As EventArgs) Handles btnSaveXmlTemplate.Click
        Dim rootpath As String = Assembly.GetEntryAssembly().Location
        Dim dir = Directory.GetParent(Directory.GetParent(rootpath).ToString()).ToString() + "\XmlTemplate"
        Dim filePath As String = Path.GetFullPath(Path.Combine(dir, cbXmlTemplate.Text + ".xml"))
        Dim text As String = txtXmlTemplate.Text
        File.WriteAllText(filePath, text)
    End Sub

    Private Sub cbXmlTemplate_DropDown(sender As Object, e As EventArgs) Handles cbXmlTemplate.DropDown
        Dim rootpath As String = Assembly.GetEntryAssembly().Location
        Dim dir = Directory.GetParent(Directory.GetParent(rootpath).ToString()).ToString() + "\XmlTemplate"
        Dim files As New List(Of String)
        For Each file As String In System.IO.Directory.GetFiles(dir)
            If IO.Path.GetExtension(file) = ".xml" Then
                files.Add(System.IO.Path.GetFileNameWithoutExtension(file))
            End If
        Next
        files.Distinct.ToList()
        cbXmlTemplate.Items.Clear()
        For Each file As String In files
            cbXmlTemplate.Items.Add(file)
        Next
    End Sub

    Private Sub btnDelXmlTemplate_Click(sender As Object, e As EventArgs) Handles btnDelXmlTemplate.Click
        Dim rootpath As String = Assembly.GetEntryAssembly().Location
        Dim dir = Directory.GetParent(Directory.GetParent(rootpath).ToString()).ToString() + "\XmlTemplate"
        Dim filepath As String = Path.GetFullPath(Path.Combine(dir, cbXmlTemplate.Text + ".xml"))
        If IO.File.Exists(filepath) Then
            IO.File.Delete(filepath)
        End If
        Dim files As New List(Of String)
        For Each file As String In System.IO.Directory.GetFiles(dir)
            If IO.Path.GetExtension(file) = ".xml" Then
                files.Add(System.IO.Path.GetFileNameWithoutExtension(file))
            End If
        Next
        files.Distinct.ToList()
        cbXmlTemplate.Items.Clear()
        For Each file As String In files
            cbXmlTemplate.Items.Add(file)
        Next
        If cbXmlTemplate.Items.Count > 0 Then
            cbXmlTemplate.SelectedIndex = 0
        Else
            cbXmlTemplate.Text = ""
        End If

    End Sub

    Private Sub btnDefaultTemplate_Click(sender As Object, e As EventArgs) Handles btnDefaultTemplate.Click
        Dim rootpath As String = Assembly.GetEntryAssembly().Location
        Dim dir = Directory.GetParent(Directory.GetParent(rootpath).ToString()).ToString() + "\XmlTemplate"
        Dim filepath As String = Path.GetFullPath(Path.Combine(dir, "default.zip"))
        Using archive As ZipArchive = ZipFile.OpenRead(filepath)
            For Each entry As ZipArchiveEntry In archive.Entries
                If Not File.Exists(Path.Combine(dir, entry.FullName)) Then
                    entry.ExtractToFile(Path.Combine(dir, entry.FullName), False)
                End If
            Next
        End Using
    End Sub

    Private Sub cbXmlTemplate_DropDownClosed(sender As Object, e As EventArgs) Handles cbXmlTemplate.DropDownClosed
        Dim rootpath As String = Assembly.GetEntryAssembly().Location
        Dim dir = Directory.GetParent(Directory.GetParent(rootpath).ToString()).ToString() + "\XmlTemplate"
        Dim fileReader As String
        Dim filePath As String = Path.GetFullPath(Path.Combine(dir, cbXmlTemplate.Text + ".xml"))
        If File.Exists(filePath) Then
            fileReader = My.Computer.FileSystem.ReadAllText(filePath)
            If fileReader <> "" Then
                txtXmlTemplate.Text = ""
                txtXmlTemplate.Text = fileReader
            End If
        End If
    End Sub
End Class


