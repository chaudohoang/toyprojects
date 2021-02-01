Imports System.Text

Public Class Form1
    Private Sub RunButton_Click(sender As Object, e As EventArgs) Handles RunButton.Click
        RunButton.Enabled = False

        If InputTextBox.Text = "" Then
            MessageBox.Show("input files are empty")
            RunButton.Enabled = True

            Exit Sub
        End If


        Dim Files() As String = inputTextBox.Text.Replace(vbCrLf, ",").Split(
                     New String() {","}, StringSplitOptions.RemoveEmptyEntries
                 ).Where(
                     Function(s) Not String.IsNullOrWhiteSpace(s)
                 ).ToArray()




        If Files.Count < 0 Then
            MessageBox.Show("input folder does not contain files")
            RunButton.Enabled = True
            Exit Sub
        End If

        Dim NewRResMatch(1169, 2531) As Single
        Dim NewG(1169, 2531) As Single
        Dim NewBResMatch(1169, 2531) As Single
        Dim NewR(584, 2531) As Single
        Dim NewB(584, 2531) As Single
        Dim Average(1169, 2531) As Single
        Dim headerR As String
        Dim headerG As String
        Dim headerB As String
        For Each inputfile As String In Files

            Try
                Using fs As New IO.StreamReader(inputfile)
                    'skip 8 header lines, save it if input file is mother green
                    If IO.Path.GetFileNameWithoutExtension(inputfile).EndsWith("R") Then
                        headerR = fs.ReadLine()
                        Dim row As Integer = 0
                        Do While Not fs.EndOfStream

                            Dim Data() As String = fs.ReadLine.Split(New String() {","}, StringSplitOptions.None)
                            For col As Integer = 0 To Data.Length - 2

                                NewRResMatch(col * 2, row) = Data(col)
                                NewRResMatch(col * 2 + 1, row) = Data(col)
                            Next
                            row += 1
                        Loop
                    ElseIf IO.Path.GetFileNameWithoutExtension(inputfile).EndsWith("G") Then
                        headerG = fs.ReadLine()
                        Dim row As Integer = 0
                        Do While Not fs.EndOfStream

                            Dim Data() As String = fs.ReadLine.Split(New String() {","}, StringSplitOptions.None)
                            For col As Integer = 0 To Data.Length - 2

                                NewG(col, row) = Data(col)
                            Next
                            row += 1
                        Loop

                    Else
                        headerB = fs.ReadLine()
                        Dim row As Integer = 0
                        Do While Not fs.EndOfStream

                            Dim Data() As String = fs.ReadLine.Split(New String() {","}, StringSplitOptions.None)
                            For col As Integer = 0 To Data.Length - 2

                                NewBResMatch(col * 2, row) = Data(col)
                                NewBResMatch(col * 2 + 1, row) = Data(col)
                            Next
                            row += 1
                        Loop

                    End If

                End Using
            Catch ex As Exception
                MessageBox.Show("Error reading input files: " & ex.Message)
            End Try


        Next

        For row As Integer = 0 To 2531
            For col As Integer = 0 To 1169

                Average(col, row) = (NewRResMatch(col, row) + NewG(col, row) + NewBResMatch(col, row)) / 3

            Next


        Next

        For row As Integer = 0 To 2531
            For col As Integer = 0 To 584

                NewR(col, row) = (Average(col * 2, row) + Average(col * 2 + 1, row)) / 2

            Next
        Next

        For row As Integer = 0 To 2531
            For col As Integer = 0 To 584

                NewB(col, row) = (Average(col * 2, row) + Average(col * 2 + 1, row)) / 2

            Next
        Next

        For Each inputfile As String In Files

            Dim OutputFolder = IO.Path.GetDirectoryName(inputfile) + "/Backup_CSV_Average/"
            My.Computer.FileSystem.CopyFile(
                inputfile, OutputFolder + IO.Path.GetFileNameWithoutExtension(inputfile + ".csv"))
            My.Computer.FileSystem.DeleteFile(inputfile)

            If IO.Path.GetFileNameWithoutExtension(inputfile).EndsWith("R") Then
                Try
                    Using sw As IO.StreamWriter = New IO.StreamWriter(IO.File.Open(inputfile, IO.FileMode.Create))

                        'write header

                        'write first line to CSV file to keep the same format
                        sw.Write("585")
                        sw.Write(",")
                        sw.Write("2532")
                        For column As Integer = 2 To 584
                            sw.Write("")
                            sw.Write(",")
                        Next
                        sw.WriteLine()


                        For row As Integer = 0 To 2531

                            Dim sb As New StringBuilder()

                            For col As Integer = 0 To 584
                                sb.Append(String.Format("{0}" + ",", NewR(col, row)))
                            Next

                            'Remove the last char from each line
                            sb.Length -= 1
                            sw.WriteLine(sb.ToString())
                        Next
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error writing output file: " & ex.Message)
                    Exit Sub
                End Try
            ElseIf IO.Path.GetFileNameWithoutExtension(inputfile).EndsWith("G") Then
                Try
                    Using sw As IO.StreamWriter = New IO.StreamWriter(IO.File.Open(inputfile, IO.FileMode.Create))

                        'write header

                        'write first line to CSV file to keep the same format
                        sw.Write("1170")
                        sw.Write(",")
                        sw.Write("2532")
                        For column As Integer = 2 To 1169
                            sw.Write("")
                            sw.Write(",")
                        Next
                        sw.WriteLine()


                        For row As Integer = 0 To 2531

                            Dim sb As New StringBuilder()

                            For col As Integer = 0 To 1169
                                sb.Append(String.Format("{0}" + ",", Average(col, row)))
                            Next

                            'Remove the last char from each line
                            sb.Length -= 1
                            sw.WriteLine(sb.ToString())
                        Next
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error writing output file: " & ex.Message)
                    Exit Sub
                End Try
            Else
                Try
                    Using sw As IO.StreamWriter = New IO.StreamWriter(IO.File.Open(inputfile, IO.FileMode.Create))

                        'write header

                        'write first line to CSV file to keep the same format
                        sw.Write("585")
                        sw.Write(",")
                        sw.Write("2532")
                        For column As Integer = 2 To 584
                            sw.Write("")
                            sw.Write(",")
                        Next
                        sw.WriteLine()


                        For row As Integer = 0 To 2531

                            Dim sb As New StringBuilder()

                            For col As Integer = 0 To 584
                                sb.Append(String.Format("{0}" + ",", NewB(col, row)))
                            Next

                            'Remove the last char from each line
                            sb.Length -= 1
                            sw.WriteLine(sb.ToString())
                        Next
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error writing output file: " & ex.Message)
                    Exit Sub
                End Try
            End If
        Next




        RunButton.Enabled = True
    End Sub

    Private Sub InputTextBox_DragEnter(sender As Object, e As DragEventArgs) Handles inputTextBox.DragEnter
        e.Effect = DragDropEffects.All
        Dim files() As String = e.Data.GetData(DataFormats.FileDrop)
        For Each path In files
            inputTextBox.Text += path + vbCrLf
        Next


    End Sub

End Class
