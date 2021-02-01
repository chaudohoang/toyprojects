Imports System.Text

Public Class Form1
    Private Sub RunButton_Click(sender As Object, e As EventArgs) Handles RunButton.Click
        RunButton.Enabled = False

        If InputTextBox.Text = "" Then
            MessageBox.Show("input files are empty")
            RunButton.Enabled = True
            Exit Sub
        End If
        If OutputTextBox.Text = "" Then
            MessageBox.Show("output folder is empty")
            RunButton.Enabled = True
            Exit Sub
        End If
        If Not IO.Directory.Exists(OutputTextBox.Text) Then
            Try
                IO.Directory.CreateDirectory(OutputTextBox.Text)
            Catch ex As Exception
                MessageBox.Show("error making output folder")
                RunButton.Enabled = True
                Exit Sub
            End Try
        End If

        Dim Files() As String = InputTextBox.Text.Replace(vbCrLf, ",").Split(
                     New String() {","}, StringSplitOptions.RemoveEmptyEntries
                 ).Where(
                     Function(s) Not String.IsNullOrWhiteSpace(s)
                 ).ToArray()

        If Files.Count < 0 Then
            MessageBox.Show("input folder does not contain files")
            RunButton.Enabled = True
            Exit Sub
        End If

        Dim Mother(1169, 2531) As Single
        Dim header(7) As String
        For Each inputfile As String In Files

            Try
                Using fs As New IO.StreamReader(inputfile)
                    'skip 8 header lines, save it if input file is mother green
                    If IO.Path.GetFileNameWithoutExtension(inputfile) = "MOTHER G" Then
                        header(0) = fs.ReadLine()
                        header(1) = fs.ReadLine()
                        header(2) = fs.ReadLine()
                        header(3) = fs.ReadLine()
                        header(4) = fs.ReadLine()
                        header(5) = fs.ReadLine()
                        header(6) = fs.ReadLine()
                        header(7) = fs.ReadLine()
                    Else
                        fs.ReadLine()
                        fs.ReadLine()
                        fs.ReadLine()
                        fs.ReadLine()
                        fs.ReadLine()
                        fs.ReadLine()
                        fs.ReadLine()
                        fs.ReadLine()

                    End If

                    Dim row As Integer = 0
                    Do While Not fs.EndOfStream

                        Dim Data() As String = fs.ReadLine.Split(New String() {ControlChars.Tab}, StringSplitOptions.None)
                        For col As Integer = 0 To Data.Length - 2
                            If IO.Path.GetFileNameWithoutExtension(inputfile) = "MOTHER R" Or IO.Path.GetFileNameWithoutExtension(inputfile) = "MOTHER B" Then
                                Mother(col * 2, row) += Data(col)
                                Mother(col * 2 + 1, row) += Data(col)
                            Else
                                Mother(col, row) += Data(col)
                            End If
                        Next
                        row += 1
                    Loop


                End Using
            Catch ex As Exception
                MessageBox.Show("Error reading input files: " & ex.Message)
            End Try


        Next
        Dim outputfile As String = outputTextBox.Text & "\" & "MOTHER new G.txt"

        Try
            Using sw As IO.StreamWriter = New IO.StreamWriter(IO.File.Open(outputfile, IO.FileMode.Create))

                'write header
                For row As Integer = 0 To header.Length - 1
                    sw.WriteLine(header(row))
                Next

                For row As Integer = 0 To 2531

                    Dim sb As New StringBuilder()

                    For col As Integer = 0 To 1169
                        sb.Append(String.Format("{0}" + vbTab, Mother(col, row)))
                    Next

                    'Remove the last tab from each line
                    sb.Length -= 1
                    sw.WriteLine(sb.ToString())
                Next
            End Using
        Catch ex As Exception
            MessageBox.Show("Error writing output file: " & ex.Message)
            Exit Sub
        End Try
        RunButton.Enabled = True
    End Sub

    Private Sub InputTextBox_DragEnter(sender As Object, e As DragEventArgs) Handles inputTextBox.DragEnter
        e.Effect = DragDropEffects.All
        Dim files() As String = e.Data.GetData(DataFormats.FileDrop)
        For Each path In files
            inputTextBox.Text += path + vbCrLf
        Next
        outputTextBox.Text = IO.Path.GetDirectoryName(files(1))

    End Sub

End Class
