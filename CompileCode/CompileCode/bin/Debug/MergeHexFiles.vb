Imports System
Imports System.IO

Namespace Program
    Friend Class Program
        Public Shared Sub Main(ByVal args As String())

            If args.Length = 2 Then
                MergeHexFiles(args(0), args(1), "out.hex")
            End If


        End Sub

        Public Shared Sub MergeHexFiles(FileIn1 As String, FileIn2 As String, FileOut As String)
            Dim StringData1 As String = ""
            Dim fileReady1 As Boolean = False
            Dim StringData2 As String = ""
            Dim fileReady2 As Boolean = False
            Do
                Try
                    Dim br As New IO.StreamReader(IO.File.Open(FileIn1, IO.FileMode.Open))
                    If br IsNot Nothing Then
                        fileReady1 = True
                    End If

                    StringData1 = br.ReadToEnd
                    br.Close()

                Catch ex As Exception

                End Try

            Loop While fileReady1 = False

            Do
                Try
                    Dim br As New IO.StreamReader(IO.File.Open(FileIn2, IO.FileMode.Open))
                    If br IsNot Nothing Then
                        fileReady2 = True
                    End If

                    StringData2 = br.ReadToEnd
                    br.Close()

                Catch ex As Exception

                End Try

            Loop While fileReady2 = False

            Try
                File.WriteAllText(FileOut, StringData1 + StringData2)
            Catch ex As Exception

            End Try


        End Sub
    End Class
End Namespace
