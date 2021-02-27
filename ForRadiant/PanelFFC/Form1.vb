Imports System.Diagnostics.Eventing
Imports System.IO
Imports System.Drawing
Imports System.Windows.Forms

Public Class PanelFFC

    Private Sub btnGenerate_Click(sender As Object, e As EventArgs) Handles btnGenerate.Click 'button Generate function

        btnGenerate.Enabled = False
        ' Create result folder
        Dim di As DirectoryInfo = New DirectoryInfo(rgboutputpathbox.Text)
        di.Create()

        Dim B1, B2, B3, B4, B5, R1, R2, R3, R4, R5, G1, G2, G3, G4, G5, MB, MR, MG, DB, DR, DG As New List(Of List(Of Double))() 'create lists
        'load all txt file to the lists
        R1 = LoadFile(rgbredfilepath1.Text)
        R2 = LoadFile(rgbredfilepath2.Text)
        R3 = LoadFile(rgbredfilepath3.Text)
        R4 = LoadFile(rgbredfilepath4.Text)
        R5 = LoadFile(rgbredfilepath5.Text)
        G1 = LoadFile(rgbgreenfilepath1.Text)
        G2 = LoadFile(rgbgreenfilepath2.Text)
        G3 = LoadFile(rgbgreenfilepath3.Text)
        G4 = LoadFile(rgbgreenfilepath4.Text)
        G5 = LoadFile(rgbgreenfilepath5.Text)
        B1 = LoadFile(rgbbluefilepath1.Text)
        B2 = LoadFile(rgbbluefilepath2.Text)
        B3 = LoadFile(rgbbluefilepath3.Text)
        B4 = LoadFile(rgbbluefilepath4.Text)
        B5 = LoadFile(rgbbluefilepath5.Text)
        MR = LoadFile(rgbmredfilepath.Text)
        MG = LoadFile(rgbmgreenfilepath.Text)
        MB = LoadFile(rgbmbluefilepath.Text)

        'calculate result list from the file
        DR = Calculate(R1, R2, R3, R4, R5, MR)
        DG = Calculate(G1, G2, G3, G4, G5, MG)
        DB = Calculate(B1, B2, B3, B4, B5, MB)

        'generate csv and xml
        If txtType.Text = "RGB" Then
            GenerateCSV(DR, rgboutputpathbox.Text + "\" + rgbmodel.Text + "_" + rgbcamerasn.Text + "_R.csv")
            GenerateCSV(DG, rgboutputpathbox.Text + "\" + rgbmodel.Text + "_" + rgbcamerasn.Text + "_G.csv")
            GenerateCSV(DB, rgboutputpathbox.Text + "\" + rgbmodel.Text + "_" + rgbcamerasn.Text + "_B.csv")
            GenerateXML(rgbmodel.Text, rgbcamerasn.Text, "R", rgboutputpathbox.Text + "\" + rgbmodel.Text + "_1.xml")
            GenerateXML(rgbmodel.Text, rgbcamerasn.Text, "G", rgboutputpathbox.Text + "\" + rgbmodel.Text + "_2.xml")
            GenerateXML(rgbmodel.Text, rgbcamerasn.Text, "B", rgboutputpathbox.Text + "\" + rgbmodel.Text + "_3.xml")
        Else
            GenerateCSV(DG, rgboutputpathbox.Text + "\" + graymodel.Text + "_" + rgbcamerasn.Text + ".csv")
            GenerateXML(graymodel.Text, rgbcamerasn.Text, rgboutputpathbox.Text + "\" + graymodel.Text + ".xml")
        End If


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

    Private Function Calculate(ByVal list1 As List(Of List(Of Double)), ByVal list2 As List(Of List(Of Double)), ByVal list3 As List(Of List(Of Double)), ByVal list4 As List(Of List(Of Double)), ByVal list5 As List(Of List(Of Double)), ByVal mlist As List(Of List(Of Double))) As List(Of List(Of Double))
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

    Private Sub rgbbtnChooseInputFolder_Click(sender As Object, e As EventArgs) Handles rgbbtnChooseInputFolder.Click  'function to choose folder which contains all the files to load

        Using frm = New FolderBrowserDialog
            If frm.ShowDialog(Me) = DialogResult.OK Then


                rgbinputpathbox.Text = frm.SelectedPath
                rgboutputpathbox.Text = rgbinputpathbox.Text + "\Result"

                Dim dir As New DirectoryInfo(rgbinputpathbox.Text)
                Dim allFiles As IO.FileInfo() = dir.GetFiles()
                Dim singleFile As IO.FileInfo

                'this long loop is just to read all the file in the chosen folder, if the file name start with R and end with 1, the program will consider the file as red measurement of panel 1, and go on
                For Each singleFile In allFiles
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("R") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("1") Then
                        rgbredfilepath1.Text = singleFile.FullName
                    End If
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("R") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("2") Then
                        rgbredfilepath2.Text = singleFile.FullName
                    End If
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("R") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("3") Then
                        rgbredfilepath3.Text = singleFile.FullName
                    End If
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("R") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("4") Then
                        rgbredfilepath4.Text = singleFile.FullName
                    End If
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("R") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("5") Then
                        rgbredfilepath5.Text = singleFile.FullName
                    End If
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("G") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("1") Then
                        rgbgreenfilepath1.Text = singleFile.FullName
                    End If
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("G") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("2") Then
                        rgbgreenfilepath2.Text = singleFile.FullName
                    End If
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("G") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("3") Then
                        rgbgreenfilepath3.Text = singleFile.FullName
                    End If
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("G") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("4") Then
                        rgbgreenfilepath4.Text = singleFile.FullName
                    End If
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("G") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("5") Then
                        rgbgreenfilepath5.Text = singleFile.FullName
                    End If
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("B") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("1") Then
                        rgbbluefilepath1.Text = singleFile.FullName
                    End If
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("B") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("2") Then
                        rgbbluefilepath2.Text = singleFile.FullName
                    End If
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("B") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("3") Then
                        rgbbluefilepath3.Text = singleFile.FullName
                    End If
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("B") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("4") Then
                        rgbbluefilepath4.Text = singleFile.FullName
                    End If
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("B") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("5") Then
                        rgbbluefilepath5.Text = singleFile.FullName
                    End If
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("M") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("R") Then
                        rgbmredfilepath.Text = singleFile.FullName
                    End If
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("M") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("G") Then
                        rgbmgreenfilepath.Text = singleFile.FullName
                    End If
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("M") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("B") Then
                        rgbmbluefilepath.Text = singleFile.FullName
                    End If

                Next

            End If
        End Using






    End Sub

    Private Sub GenerateXML(ByVal model As String, ByVal camerasn As String, ByVal color As String, ByVal outputfile As String)

        Dim xmlfile As New System.IO.StreamWriter(outputfile)

        xmlfile.WriteLine("<?xml version=""1.0"" encoding=""utf-8""?>")
        xmlfile.WriteLine("<LensCosineCorrection xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">")
        xmlfile.WriteLine("  <SerialNumber>" + camerasn + "</SerialNumber>")
        xmlfile.WriteLine("  <NormalFileName>" + model + "_" + camerasn + "_" + color + ".csv</NormalFileName>")
        xmlfile.WriteLine("  <NormalFileSmoothFilter>")
        xmlfile.WriteLine("    <Width>300</Width>")
        xmlfile.WriteLine("    <Height>300</Height>")
        xmlfile.WriteLine("  </NormalFileSmoothFilter>")
        xmlfile.WriteLine("  <CosineCorrection>0, 0</CosineCorrection>")
        xmlfile.WriteLine("  <VerticalGradient>0.5, 1</VerticalGradient>")
        xmlfile.WriteLine("  <HorizontalGradient>0, 0</HorizontalGradient>")
        xmlfile.WriteLine("  <Vignetting>850p, 0, 1.015</Vignetting>")
        xmlfile.WriteLine("  <BorderAttenuation>0,0,0,0,0,0,0,0</BorderAttenuation>")
        xmlfile.WriteLine("</LensCosineCorrection>")
        xmlfile.Close()

    End Sub

    Private Sub GenerateXML(ByVal model As String, ByVal camerasn As String, ByVal outputfile As String)

        Dim xmlfile As New System.IO.StreamWriter(outputfile)

        xmlfile.WriteLine("<?xml version=""1.0"" encoding=""utf-8""?>")
        xmlfile.WriteLine("<LensCosineCorrection xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">")
        xmlfile.WriteLine("  <SerialNumber>" + camerasn + "</SerialNumber>")
        xmlfile.WriteLine("  <NormalFileName>" + model + "_" + camerasn + ".csv</NormalFileName>")
        xmlfile.WriteLine("  <NormalFileSmoothFilter>")
        xmlfile.WriteLine("    <Width>300</Width>")
        xmlfile.WriteLine("    <Height>300</Height>")
        xmlfile.WriteLine("  </NormalFileSmoothFilter>")
        xmlfile.WriteLine("  <CosineCorrection>0, 0</CosineCorrection>")
        xmlfile.WriteLine("  <VerticalGradient>0.5, 1</VerticalGradient>")
        xmlfile.WriteLine("  <HorizontalGradient>0, 0</HorizontalGradient>")
        xmlfile.WriteLine("  <Vignetting>850p, 0, 1.015</Vignetting>")
        xmlfile.WriteLine("  <BorderAttenuation>0,0,0,0,0,0,0,0</BorderAttenuation>")
        xmlfile.WriteLine("</LensCosineCorrection>")
        xmlfile.Close()

    End Sub

    Private Sub rgbbtnChooseRedFile1_Click(sender As Object, e As EventArgs) Handles rgbbtnChooseRedFile1.Click
        Using frm = New OpenFileDialog
            If frm.ShowDialog(Me) = DialogResult.OK Then
                rgbredfilepath1.Text = frm.FileName
            End If
        End Using
    End Sub

    Private Sub rgbbtnChooseRedFile2_Click(sender As Object, e As EventArgs) Handles rgbbtnChooseRedFile2.Click
        Using frm = New OpenFileDialog
            If frm.ShowDialog(Me) = DialogResult.OK Then
                rgbredfilepath2.Text = frm.FileName
            End If
        End Using
    End Sub

    Private Sub rgbbtnChooseRedFile3_Click(sender As Object, e As EventArgs) Handles rgbbtnChooseRedFile3.Click
        Using frm = New OpenFileDialog
            If frm.ShowDialog(Me) = DialogResult.OK Then
                rgbredfilepath3.Text = frm.FileName
            End If
        End Using
    End Sub

    Private Sub rgbbtnChooseRedFile4_Click(sender As Object, e As EventArgs) Handles rgbbtnChooseRedFile4.Click
        Using frm = New OpenFileDialog
            If frm.ShowDialog(Me) = DialogResult.OK Then
                rgbredfilepath4.Text = frm.FileName
            End If
        End Using
    End Sub

    Private Sub rgbbtnChooseRedFile5_Click(sender As Object, e As EventArgs) Handles rgbbtnChooseRedFile5.Click
        Using frm = New OpenFileDialog
            If frm.ShowDialog(Me) = DialogResult.OK Then
                rgbredfilepath5.Text = frm.FileName
            End If
        End Using
    End Sub

    Private Sub rgbbtnChooseGreenFile1_Click(sender As Object, e As EventArgs) Handles rgbbtnChooseGreenFile1.Click
        Using frm = New OpenFileDialog
            If frm.ShowDialog(Me) = DialogResult.OK Then
                rgbgreenfilepath1.Text = frm.FileName
            End If
        End Using
    End Sub

    Private Sub rgbbtnChooseGreenFile2_Click(sender As Object, e As EventArgs) Handles rgbbtnChooseGreenFile2.Click
        Using frm = New OpenFileDialog
            If frm.ShowDialog(Me) = DialogResult.OK Then
                rgbgreenfilepath2.Text = frm.FileName
            End If
        End Using
    End Sub

    Private Sub rgbbtnChooseGreenFile3_Click(sender As Object, e As EventArgs) Handles rgbbtnChooseGreenFile3.Click
        Using frm = New OpenFileDialog
            If frm.ShowDialog(Me) = DialogResult.OK Then
                rgbgreenfilepath3.Text = frm.FileName
            End If
        End Using
    End Sub

    Private Sub rgbbtnChooseGreenFile4_Click(sender As Object, e As EventArgs) Handles rgbbtnChooseGreenFile4.Click
        Using frm = New OpenFileDialog
            If frm.ShowDialog(Me) = DialogResult.OK Then
                rgbgreenfilepath4.Text = frm.FileName
            End If
        End Using
    End Sub

    Private Sub rgbbtnChooseGreenFile5_Click(sender As Object, e As EventArgs) Handles rgbbtnChooseGreenFile5.Click
        Using frm = New OpenFileDialog
            If frm.ShowDialog(Me) = DialogResult.OK Then
                rgbgreenfilepath5.Text = frm.FileName
            End If
        End Using
    End Sub

    Private Sub rgbbtnChooseBlueFile1_Click(sender As Object, e As EventArgs) Handles rgbbtnChooseBlueFile1.Click
        Using frm = New OpenFileDialog
            If frm.ShowDialog(Me) = DialogResult.OK Then
                rgbbluefilepath1.Text = frm.FileName
            End If
        End Using
    End Sub

    Private Sub rgbbtnChooseBlueFile2_Click(sender As Object, e As EventArgs) Handles rgbbtnChooseBlueFile2.Click
        Using frm = New OpenFileDialog
            If frm.ShowDialog(Me) = DialogResult.OK Then
                rgbbluefilepath2.Text = frm.FileName
            End If
        End Using
    End Sub

    Private Sub rgbbtnChooseBlueFile3_Click(sender As Object, e As EventArgs) Handles rgbbtnChooseBlueFile3.Click
        Using frm = New OpenFileDialog
            If frm.ShowDialog(Me) = DialogResult.OK Then
                rgbbluefilepath3.Text = frm.FileName
            End If
        End Using
    End Sub

    Private Sub rgbbtnChooseBlueFile4_Click(sender As Object, e As EventArgs) Handles rgbbtnChooseBlueFile4.Click
        Using frm = New OpenFileDialog
            If frm.ShowDialog(Me) = DialogResult.OK Then
                rgbbluefilepath4.Text = frm.FileName
            End If
        End Using
    End Sub

    Private Sub rgbbtnChooseBlueFile5_Click(sender As Object, e As EventArgs) Handles rgbbtnChooseBlueFile5.Click
        Using frm = New OpenFileDialog
            If frm.ShowDialog(Me) = DialogResult.OK Then
                rgbbluefilepath5.Text = frm.FileName
            End If
        End Using
    End Sub

    Private Sub rgbbtnChooseMotherRedFile_Click(sender As Object, e As EventArgs) Handles rgbbtnChooseMotherRedFile.Click
        Using frm = New OpenFileDialog
            If frm.ShowDialog(Me) = DialogResult.OK Then
                rgbmredfilepath.Text = frm.FileName
            End If
        End Using
    End Sub

    Private Sub rgbbtnChooseMotherGreenFile_Click(sender As Object, e As EventArgs) Handles rgbbtnChooseMotherGreenFile.Click
        Using frm = New OpenFileDialog
            If frm.ShowDialog(Me) = DialogResult.OK Then
                rgbmgreenfilepath.Text = frm.FileName
            End If
        End Using
    End Sub

    Private Sub rgbbtnChooseMotherBlueFile_Click(sender As Object, e As EventArgs) Handles rgbbtnChooseOuputFolder.Click, rgbbtnChooseMotherBlueFile.Click, Button1.Click
        Using frm = New OpenFileDialog
            If frm.ShowDialog(Me) = DialogResult.OK Then
                rgbmbluefilepath.Text = frm.FileName
            End If
        End Using
    End Sub

    Private Sub rgbinputpathbox_TextChanged(sender As Object, e As EventArgs) Handles rgbinputpathbox.TextChanged
        rgboutputpathbox.Text = rgbinputpathbox.Text + "\Result"

        Dim dir As New DirectoryInfo(rgbinputpathbox.Text)
        Dim allFiles As IO.FileInfo() = dir.GetFiles()
        Dim singleFile As IO.FileInfo

        'this long loop is just to read all the file in the chosen folder, if the file name start with R and end with 1, the program will consider the file as red measurement of panel 1, and go on
        For Each singleFile In allFiles
            If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("R") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("1") Then
                rgbredfilepath1.Text = singleFile.FullName
            End If
            If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("R") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("2") Then
                rgbredfilepath2.Text = singleFile.FullName
            End If
            If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("R") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("3") Then
                rgbredfilepath3.Text = singleFile.FullName
            End If
            If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("R") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("4") Then
                rgbredfilepath4.Text = singleFile.FullName
            End If
            If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("R") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("5") Then
                rgbredfilepath5.Text = singleFile.FullName
            End If
            If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("G") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("1") Then
                rgbgreenfilepath1.Text = singleFile.FullName
            End If
            If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("G") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("2") Then
                rgbgreenfilepath2.Text = singleFile.FullName
            End If
            If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("G") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("3") Then
                rgbgreenfilepath3.Text = singleFile.FullName
            End If
            If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("G") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("4") Then
                rgbgreenfilepath4.Text = singleFile.FullName
            End If
            If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("G") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("5") Then
                rgbgreenfilepath5.Text = singleFile.FullName
            End If
            If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("B") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("1") Then
                rgbbluefilepath1.Text = singleFile.FullName
            End If
            If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("B") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("2") Then
                rgbbluefilepath2.Text = singleFile.FullName
            End If
            If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("B") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("3") Then
                rgbbluefilepath3.Text = singleFile.FullName
            End If
            If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("B") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("4") Then
                rgbbluefilepath4.Text = singleFile.FullName
            End If
            If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("B") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("5") Then
                rgbbluefilepath5.Text = singleFile.FullName
            End If
            If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("M") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("R") Then
                rgbmredfilepath.Text = singleFile.FullName
            End If
            If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("M") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("G") Then
                rgbmgreenfilepath.Text = singleFile.FullName
            End If
            If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("M") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("B") Then
                rgbmbluefilepath.Text = singleFile.FullName
            End If

        Next
    End Sub

    Private Sub graybtnChooseInputFolder_Click(sender As Object, e As EventArgs) Handles graybtnChooseInputFolder.Click
        Using frm = New FolderBrowserDialog
            If frm.ShowDialog(Me) = DialogResult.OK Then


                grayinputpathbox.Text = frm.SelectedPath
                grayoutputpathbox.Text = grayinputpathbox.Text + "\Result"

                Dim dir As New DirectoryInfo(grayinputpathbox.Text)
                Dim allFiles As IO.FileInfo() = dir.GetFiles()
                Dim singleFile As IO.FileInfo

                'this long loop is just to read all the file in the chosen folder, if the file name start with R and end with 1, the program will consider the file as red measurement of panel 1, and go on
                For Each singleFile In allFiles

                    If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("G") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("1") Then
                        graygreenfilepath1.Text = singleFile.FullName
                    End If
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("G") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("2") Then
                        graygreenfilepath2.Text = singleFile.FullName
                    End If
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("G") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("3") Then
                        graygreenfilepath3.Text = singleFile.FullName
                    End If
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("G") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("4") Then
                        graygreenfilepath4.Text = singleFile.FullName
                    End If
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("G") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("5") Then
                        rgbgreenfilepath5.Text = singleFile.FullName
                    End If


                    If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("M") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("G") Then
                        graymgreenfilepath.Text = singleFile.FullName
                    End If


                Next

            End If
        End Using

    End Sub

    Private Sub grayinputpathbox_TextChanged(sender As Object, e As EventArgs) Handles grayinputpathbox.TextChanged
        grayoutputpathbox.Text = grayinputpathbox.Text + "\Result"

        Dim dir As New DirectoryInfo(grayinputpathbox.Text)
        Dim allFiles As IO.FileInfo() = dir.GetFiles()
        Dim singleFile As IO.FileInfo

        'this long loop is just to read all the file in the chosen folder, if the file name start with R and end with 1, the program will consider the file as red measurement of panel 1, and go on
        For Each singleFile In allFiles

            If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("G") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("1") Then
                graygreenfilepath1.Text = singleFile.FullName
            End If
            If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("G") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("2") Then
                graygreenfilepath2.Text = singleFile.FullName
            End If
            If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("G") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("3") Then
                graygreenfilepath3.Text = singleFile.FullName
            End If
            If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("G") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("4") Then
                graygreenfilepath4.Text = singleFile.FullName
            End If
            If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("G") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("5") Then
                graygreenfilepath5.Text = singleFile.FullName
            End If


            If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("M") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("G") Then
                graymgreenfilepath.Text = singleFile.FullName
            End If


        Next
    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click

        FileGrid.Rows.Clear()
        Dim dir As New DirectoryInfo(txtinputpath.Text)
        Dim allFiles As IO.FileInfo() = dir.GetFiles()
        Dim singleFile As IO.FileInfo
        Dim rowlength As New Integer
        Dim columnlength As New Integer
        'rowlength = list1.Count
        'columnlength = list1(0).Count
        'Dim records As New List(Of List(Of Double))() 'define 2D list

        ''this loop is to calculate the value in every row and then add the row to the result list
        'For row As Integer = 0 To rowlength - 1
        '    Dim values As New List(Of Double)() ' define row list
        '    Dim tempvalue As New Double
        '    For column As Integer = 0 To columnlength - 1

        '        tempvalue = mlist(row)(column) / (list1(row)(column) + list2(row)(column) + list3(row)(column) + list4(row)(column) + list5(row)(column))
        '        values.Add(tempvalue)
        '    Next

        '    records.Add(values)
        'Next

        If txtpanelcount.Text IsNot "" Then
            If txtType.Text = "RGB" Then

                For count As Integer = 1 To txtpanelcount.Text
                    For Each singleFile In allFiles
                        If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("R") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith(count.ToString()) Then
                            FileGrid.Rows.Add("Red File " & count, singleFile.FullName)
                        End If
                    Next
                Next
                For count As Integer = 1 To txtpanelcount.Text
                    For Each singleFile In allFiles
                        If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("G") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith(count.ToString()) Then
                            FileGrid.Rows.Add("Green File " & count, singleFile.FullName)
                        End If
                    Next
                Next
                For count As Integer = 1 To txtpanelcount.Text
                    For Each singleFile In allFiles
                        If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("B") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith(count.ToString()) Then
                            FileGrid.Rows.Add("Blue File " & count, singleFile.FullName)
                        End If
                    Next
                Next

                For Each singleFile In allFiles
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("M") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("R") Then
                        FileGrid.Rows.Add("Mother Red File ", singleFile.FullName)
                    End If
                Next


                For Each singleFile In allFiles
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("M") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("G") Then
                        FileGrid.Rows.Add("Mother Green File ", singleFile.FullName)
                    End If
                Next


                For Each singleFile In allFiles
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("M") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("B") Then
                        FileGrid.Rows.Add("Mother Blue File ", singleFile.FullName)
                    End If
                Next


            Else

                For count As Integer = 1 To txtpanelcount.Text
                    For Each singleFile In allFiles
                        If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("G") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith(count.ToString()) Then
                            FileGrid.Rows.Add("Gray File " & count, singleFile.FullName)
                        End If
                    Next
                Next

                For Each singleFile In allFiles
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("M") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("G") Then
                        FileGrid.Rows.Add("Mother Gray File ", singleFile.FullName)
                    End If
                Next


            End If
        End If

    End Sub

    Private Sub txtinputpath_TextChanged(sender As Object, e As EventArgs) Handles txtinputpath.TextChanged
        txtoutputpath.Text = txtinputpath.Text + "\Result"
    End Sub

    Private Sub btnRun_Click(sender As Object, e As EventArgs) Handles btnRun.Click
        btnRun.Enabled = False
        Dim di As DirectoryInfo = New DirectoryInfo(txtoutputpath.Text)
        di.Create()
        btnRun.Enabled = True
    End Sub
End Class


