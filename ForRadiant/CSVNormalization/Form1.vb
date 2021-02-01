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

        'Chau modified this part to avoid empty file path error
        Dim Files() As String = InputTextBox.Text.Replace(vbCrLf, ",").Split(
                     New String() {","}, StringSplitOptions.RemoveEmptyEntries
                 ).Where(
                     Function(s) Not String.IsNullOrWhiteSpace(s)
                 ).ToArray()

        If Files.Count < 0 Then
            MessageBox.Show("input folder does not contain csv files")
            RunButton.Enabled = True
            Exit Sub
        End If



        For Each inputPath As String In Files

            'read the header 
            Dim numRows As Integer = 0
            Dim numCols As Integer = 0
            Try
                Using sr As New IO.StreamReader(inputPath)
                    Dim Data() As String = sr.ReadLine.Split(CChar(","))
                    numCols = Data(0)
                    numRows = Data(1)
                End Using
            Catch ex As Exception
                MessageBox.Show("Error reading CSV header: " & ex.Message)
            End Try


            ''read the data
            Dim InputImage(numCols - 1, numRows - 1) As Single
            Try
                Using sr As New IO.StreamReader(inputPath)
                    Dim trash() As String = sr.ReadLine.Split(CChar(",")) ' skip the header
                    Dim row As Integer = 0
                    Do While Not sr.EndOfStream
                        Dim Data() As String = sr.ReadLine.Split(CChar(","))
                        For col As Integer = 0 To numCols - 1
                            InputImage(col, row) = Data(col).Trim
                        Next
                        row += 1
                    Loop

                End Using
            Catch ex As Exception
                MessageBox.Show("Error reading CSV data: " & ex.Message)
                RunButton.Enabled = True
                Exit Sub
            End Try

            Dim detWidth As Integer = numCols * 0.1
            Dim detHeight As Integer = numRows * 0.05
            Dim detWidthHalf As Integer = detWidth / 2
            Dim detHeightHalf As Integer = detHeight / 2
            Dim detCenterX As Integer = numCols / 2
            Dim detCenterY As Integer = numRows / 2
            Dim detStartX As Integer = detCenterX - detWidthHalf
            Dim detEndX As Integer = detCenterX + detWidthHalf
            Dim detStartY As Integer = detCenterY - detHeightHalf
            Dim detEndY As Integer = detCenterY + detHeightHalf

            Dim Average As Single = 0
            For col As Integer = detStartX To detEndX
                For row As Integer = detStartY To detEndY
                    Average += InputImage(col, row)
                Next
            Next
            Average /= ((detEndX - detStartX + 1) * (detEndY - detStartY + 1))

            Dim OutputImage(numCols - 1, numRows - 1) As Single
            For col As Integer = 0 To numCols - 1
                For row As Integer = 0 To numRows - 1
                    OutputImage(col, row) = InputImage(col, row) / Average
                Next
            Next

            Dim OuputPath As String = OutputTextBox.Text & "\" & IO.Path.GetFileNameWithoutExtension(inputPath) & Now.ToString("yyyyMMddhhmmss") & ".csv"

            Try
                Using sw As IO.StreamWriter = New IO.StreamWriter(IO.File.Open(OuputPath, IO.FileMode.Create))

                    'Chau added
                    'write first line to CSV file to keep the same format
                    sw.Write(numCols)
                    sw.Write(",")
                    sw.Write(numRows)
                    For column As Integer = 2 To numCols - 1
                        sw.Write("")
                        sw.Write(",")
                    Next
                    sw.WriteLine()

                    For row As Integer = 0 To numRows - 1

                        Dim sb As New StringBuilder()

                        For col As Integer = 0 To numCols - 1
                            sb.Append(String.Format("{0},", OutputImage(col, row)))
                        Next

                        'Remove the last comma from each line
                        sb.Length -= 1
                        sw.WriteLine(sb.ToString())
                    Next
                End Using
            Catch ex As Exception
                MessageBox.Show("Error writing CSV: " & ex.Message)
                RunButton.Enabled = True
                Exit Sub
            End Try


        Next

        RunButton.Enabled = True

    End Sub

    'Chau added
    'Added ability to drag and drop files to input box
    Private Sub InputTextBox_DragEnter(sender As Object, e As DragEventArgs) Handles InputTextBox.DragEnter
        e.Effect = DragDropEffects.All
        Dim files() As String = e.Data.GetData(DataFormats.FileDrop)
        For Each path In files
            InputTextBox.Text += path + vbCrLf
        Next
    End Sub
End Class
