Imports System.Diagnostics.Eventing
Imports System.IO
Imports System.Drawing
Imports System.Windows.Forms

Public Class PanelFFC

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


    Private Sub btnRun_Click(sender As Object, e As EventArgs) Handles btnRun.Click
        btnRun.Enabled = False
        Dim di As DirectoryInfo = New DirectoryInfo(txtoutputpath.Text)
        di.Create()

        Dim dir As New DirectoryInfo(txtinputpath.Text)
        Dim allFiles As IO.FileInfo() = dir.GetFiles()
        Dim singleFile As IO.FileInfo

        Dim listRed As New List(Of List(Of List(Of Double)))
        Dim listGreen As New List(Of List(Of List(Of Double)))
        Dim listBlue As New List(Of List(Of List(Of Double)))
        Dim listGray As New List(Of List(Of List(Of Double)))
        Dim motherRed As New List(Of List(Of Double))
        Dim motherGreen As New List(Of List(Of Double))
        Dim motherBlue As New List(Of List(Of Double))
        Dim motherGray As New List(Of List(Of Double))
        Dim resultRed As New List(Of List(Of Double))
        Dim resultGreen As New List(Of List(Of Double))
        Dim resultBlue As New List(Of List(Of Double))
        Dim resultGray As New List(Of List(Of Double))

        If txtpanelcount.Text IsNot "" Then
            If txtType.Text = "RGB" Then 'if rgb

                For Each singleFile In allFiles


                    If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("M") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("R") Then
                        motherRed = LoadFile(singleFile.FullName)

                    End If
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("M") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("G") Then
                        motherGreen = LoadFile(singleFile.FullName)

                    End If
                    If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("M") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("B") Then
                        motherBlue = LoadFile(singleFile.FullName)

                    End If
                Next

                For count As Integer = 1 To txtpanelcount.Text
                    For Each singleFile In allFiles
                        If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("R") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith(count.ToString()) Then
                            listRed.Add(LoadFile(singleFile.FullName))

                        End If
                        If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("G") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith(count.ToString()) Then
                            listGreen.Add(LoadFile(singleFile.FullName))

                        End If
                        If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("B") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith(count.ToString()) Then
                            listBlue.Add(LoadFile(singleFile.FullName))

                        End If


                    Next
                Next

                Dim redrowlength As New Integer
                Dim redcolumnlength As New Integer
                Dim greenrowlength As New Integer
                Dim greencolumnlength As New Integer
                Dim bluerowlength As New Integer
                Dim bluecolumnlength As New Integer
                redrowlength = listRed(0).Count
                redcolumnlength = listRed(0)(0).Count
                greenrowlength = listGreen(0).Count
                greencolumnlength = listGreen(0)(0).Count
                bluerowlength = listBlue(0).Count
                bluecolumnlength = listBlue(0)(0).Count



                For row As Integer = 0 To redrowlength - 1
                    Dim values As New List(Of Double)() ' define row list
                    Dim tempvalue As New Double
                    For column As Integer = 0 To redcolumnlength - 1
                        For count As Integer = 0 To txtpanelcount.Text - 1
                            tempvalue += listRed(count)(row)(column)
                        Next
                        tempvalue = motherRed(row)(column) / tempvalue
                        values.Add(tempvalue)

                    Next

                    resultRed.Add(values)
                Next
                For row As Integer = 0 To greenrowlength - 1
                    Dim values As New List(Of Double)() ' define row list
                    Dim tempvalue As New Double
                    For column As Integer = 0 To greencolumnlength - 1
                        For count As Integer = 0 To txtpanelcount.Text - 1
                            tempvalue += listGreen(count)(row)(column)
                        Next
                        tempvalue = motherGreen(row)(column) / tempvalue
                        values.Add(tempvalue)

                    Next

                    resultGreen.Add(values)
                Next
                For row As Integer = 0 To bluerowlength - 1
                    Dim values As New List(Of Double)() ' define row list
                    Dim tempvalue As New Double
                    For column As Integer = 0 To bluecolumnlength - 1
                        For count As Integer = 0 To txtpanelcount.Text - 1
                            tempvalue += listBlue(count)(row)(column)
                        Next
                        tempvalue = motherBlue(row)(column) / tempvalue
                        values.Add(tempvalue)

                    Next

                    resultBlue.Add(values)
                Next
                GenerateCSV(resultRed, txtoutputpath.Text + "\" + txtCsvPrefix.Text + "_" + txtCameraSN.Text + "_" + txtCSVSubfix.Text.Split(",")(0) + ".csv")
                GenerateXML(txtXMLPrefix.Text, txtCameraSN.Text, txtCSVSubfix.Text.Split(",")(0), txtoutputpath.Text + "\" + txtXMLPrefix.Text + "_" + txtXMLSubfix.Text.Split(",")(0) + ".xml")
                GenerateCSV(resultGreen, txtoutputpath.Text + "\" + txtCsvPrefix.Text + "_" + txtCameraSN.Text + "_" + txtCSVSubfix.Text.Split(",")(1) + ".csv")
                GenerateXML(txtXMLPrefix.Text, txtCameraSN.Text, txtCSVSubfix.Text.Split(",")(1), txtoutputpath.Text + "\" + txtXMLPrefix.Text + "_" + txtXMLSubfix.Text.Split(",")(1) + ".xml")
                GenerateCSV(resultBlue, txtoutputpath.Text + "\" + txtCsvPrefix.Text + "_" + txtCameraSN.Text + "_" + txtCSVSubfix.Text.Split(",")(2) + ".csv")
                GenerateXML(txtXMLPrefix.Text, txtCameraSN.Text, txtCSVSubfix.Text.Split(",")(2), txtoutputpath.Text + "\" + txtXMLPrefix.Text + "_" + txtXMLSubfix.Text.Split(",")(2) + ".xml")
            Else 'if gray


                For Each singleFile In allFiles

                    If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("M") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith("G") Then
                        motherGray = LoadFile(singleFile.FullName)

                    End If
                Next
                For count As Integer = 1 To txtpanelcount.Text
                    For Each singleFile In allFiles
                        If Path.GetFileNameWithoutExtension(singleFile.FullName).StartsWith("G") And Path.GetFileNameWithoutExtension(singleFile.FullName).EndsWith(count.ToString()) Then
                            listGray.Add(LoadFile(singleFile.FullName))

                        End If

                    Next
                Next

                Dim grayrowlength As New Integer
                Dim grayncolumnlength As New Integer
                grayrowlength = listGray(0).Count
                grayncolumnlength = listGray(0)(0).Count
                For row As Integer = 0 To grayrowlength - 1
                    Dim values As New List(Of Double)() ' define row list
                    Dim tempvalue As New Double
                    For column As Integer = 0 To grayncolumnlength - 1
                        For count As Integer = 0 To txtpanelcount.Text - 1
                            tempvalue += listGray(count)(row)(column)
                        Next
                        tempvalue = motherGray(row)(column) / tempvalue
                        values.Add(tempvalue)

                    Next

                    resultGray.Add(values)
                Next
                'For filecount As Integer = 0 To txtpanelcount.Text - 1
                '    For row As Integer = 0 To grayrowlength - 1
                '        Dim values As New List(Of Double)()
                '        Dim tempvalue As New Double
                '        For column As Integer = 0 To grayncolumnlength - 1
                '            tempvalue += listGray(filecount)(row)(column)
                '        Next
                '        tempvalue = motherGray(row)(column) / tempvalue
                '        values.Add(tempvalue)
                '    Next

                'Next
                GenerateCSV(resultGray, txtoutputpath.Text + "\" + txtCsvPrefix.Text + "_" + txtCameraSN.Text + ".csv")
                GenerateXML(txtXMLPrefix.Text, txtCameraSN.Text, txtoutputpath.Text + "\" + txtXMLPrefix.Text + ".xml")
            End If


        End If
        Static m_Rnd As New Random
        Dim tempcolor As Color
        tempcolor = Label20.ForeColor
        Do While Label20.ForeColor = tempcolor
            Label20.ForeColor = Color.FromArgb(255, m_Rnd.Next(0, 255), m_Rnd.Next(0, 255), m_Rnd.Next(0, 255))
        Loop

        Label20.Text = "Done !"
        btnRun.Enabled = True
    End Sub

    Private Sub txtinputpath_TextChanged(sender As Object, e As EventArgs) Handles txtinputpath.TextChanged
        txtoutputpath.Text = txtinputpath.Text + "\Result"
    End Sub

End Class


