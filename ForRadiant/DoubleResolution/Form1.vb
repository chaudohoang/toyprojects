Imports System.Text
Imports System.Reflection
Imports System.IO

Public Class Form1

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
    Private Sub RunButton_Click(sender As Object, e As EventArgs) Handles RunButton.Click
        RunButton.Enabled = False

        If inputTextBox.Text = "" Then
            MessageBox.Show("input files are empty")
            RunButton.Enabled = True
            Exit Sub
        End If
        If outputTextBox.Text = "" Then
            MessageBox.Show("output folder is empty")
            RunButton.Enabled = True
            Exit Sub
        End If
        If Not IO.Directory.Exists(outputTextBox.Text) Then
            Try
                IO.Directory.CreateDirectory(outputTextBox.Text)
            Catch ex As Exception
                MessageBox.Show("error making output folder")
                RunButton.Enabled = True
                Exit Sub
            End Try
        End If

        Dim inputfile As String = inputTextBox.Text

        Dim newrow As Integer
        Dim newcolumn As Integer
        Dim outputfile As String
        Dim header(7) As String

        newrow = File.ReadAllLines(inputfile).Length - 8
        newcolumn = (File.ReadAllLines(inputfile)(8).Split(New String() {ControlChars.Tab}, StringSplitOptions.None).Count - 1) * 2
        Dim newImage(newcolumn - 1, newrow - 1) As Single
        Try
            Using fs As New IO.StreamReader(inputfile)

                header(0) = fs.ReadLine()
                header(1) = fs.ReadLine()
                header(2) = fs.ReadLine()
                header(3) = fs.ReadLine()
                header(4) = fs.ReadLine()
                header(5) = fs.ReadLine()
                header(6) = fs.ReadLine()
                header(7) = fs.ReadLine()

                Dim row As Integer = 0
                Do While Not fs.EndOfStream

                    Dim Data() As String = fs.ReadLine.Split(New String() {ControlChars.Tab}, StringSplitOptions.None)

                    For col As Integer = 0 To Data.Length - 2

                        newImage(col * 2, row) += Data(col)
                        newImage(col * 2 + 1, row) += Data(col)

                    Next
                    row += 1
                Loop

            End Using
        Catch ex As Exception
            MessageBox.Show("Error reading input files: " & ex.Message)
        End Try

        outputfile = outputTextBox.Text & "\" & IO.Path.GetFileName(inputfile) & ".txt"

        Try
            Using sw As IO.StreamWriter = New IO.StreamWriter(IO.File.Open(outputfile, IO.FileMode.Create))

                'write header
                For row As Integer = 0 To header.Length - 1
                    sw.WriteLine(header(row))
                Next

                For row As Integer = 0 To newrow - 1

                    Dim sb As New StringBuilder()

                    For col As Integer = 0 To newcolumn - 1
                        sb.Append(String.Format("{0}" + vbTab, newImage(col, row)))
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
        Dim file As String = e.Data.GetData(DataFormats.FileDrop)(0)

        inputTextBox.Text = file

        outputTextBox.Text = IO.Path.GetDirectoryName(file) & "\" & "Result"

    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        SetVersionInfo()
    End Sub
End Class
