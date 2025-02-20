Imports System.IO
Imports System.Diagnostics

Module Module1
    Sub Main(args As String())
        ' Ensure an input file is provided
        If args.Length = 0 Then
            Console.WriteLine("Usage: RunTestOmit.exe input_file.txt")
            Console.ReadLine()
            Return
        End If

        Dim inputFile As String = args(0)

        ' Ensure file exists
        If Not File.Exists(inputFile) Then
            Console.WriteLine("Error: File not found - " & inputFile)
            Console.ReadLine()
            Return
        End If

        ' Define working directories
        Dim workingDirs As String() = {
            "C:\Program Files\Radiant Vision Systems\TrueTest 1.8\ScratchDustDetect_NY",
            "C:\Program Files\Radiant Vision Systems\TrueTest 1.8 debug Mobile\ScratchDustDetect_NY",
            "D:\Batch\ScratchNY_Custom\Exe Files"
        }

        ' Prompt user for working directory without requiring Enter
        Dim selectedDir As String = ""
        Do
            Console.WriteLine("Select a working directory:")
            Console.WriteLine("[1] " & workingDirs(0))
            Console.WriteLine("[2] " & workingDirs(1))
            Console.WriteLine("[3] " & workingDirs(2))
            Console.Write("Press 1, 2, or 3: ")

            Dim key As ConsoleKeyInfo = Console.ReadKey(True)
            Select Case key.KeyChar
                Case "1"c, "2"c, "3"c
                    selectedDir = workingDirs(CInt(key.KeyChar.ToString()) - 1)
                Case Else
                    Console.WriteLine(vbCrLf & "Invalid choice. Try again.")
            End Select
        Loop While selectedDir = ""

        Console.WriteLine(vbCrLf & "Selected working directory: " & selectedDir)

        Dim exePath As String
        Dim arguments As New List(Of String)

        ' Read all lines from file
        Do
            Dim lines As String() = File.ReadAllLines(inputFile)

            ' Ensure there is at least one line (the executable)
            If lines.Length = 0 Then
                Console.WriteLine("Error: Input file is empty")
                Console.ReadLine()
                Return
            End If

            ' Extract executable and arguments
            exePath = lines(0).Trim()
            arguments.Clear()

            For i As Integer = 1 To lines.Length - 1
                Dim arg As String = lines(i).Trim()

                ' Check if the argument is a folder OR if it ends with "\"
                If Directory.Exists(arg) OrElse arg.EndsWith("\") Then
                    ' Ensure the directory exists
                    If Not Directory.Exists(arg) Then
                        Try
                            Directory.CreateDirectory(arg)
                            Console.WriteLine("Created folder: " & arg)
                        Catch ex As Exception
                            Console.WriteLine("Error creating folder: " & arg & " - " & ex.Message)
                        End Try
                    End If
                End If

                ' Wrap argument in quotes if it contains spaces
                If arg.Contains(" ") Then
                    arg = Chr(34) & arg & Chr(34)
                End If

                arguments.Add(arg)
            Next

            ' Create process start info
            Dim startInfo As New ProcessStartInfo With {
                .FileName = exePath,
                .Arguments = String.Join(" ", arguments),
                .WorkingDirectory = selectedDir,
                .UseShellExecute = False
            }

            ' Run the process
            Dim exitCode As Integer = -1
            Try
                Console.WriteLine("Running in directory: " & selectedDir)
                Console.WriteLine("Command: " & exePath & " " & String.Join(" ", arguments))
                Dim process As Process = Process.Start(startInfo)
                process.WaitForExit()
                exitCode = process.ExitCode
            Catch ex As Exception
                Console.WriteLine("Error running process: " & ex.Message)
            End Try

            ' Print the exit code
            Console.WriteLine("Exit Code: " & exitCode)
            If exitCode = 0 Then
                Console.WriteLine("Succeeded")
            Else
                Console.WriteLine("Error: Exit code " & exitCode)
            End If

            ' Ask the user if they want to run it again using 1/2 instead of Y/N
            Dim retry As Boolean = False
            Do
                Console.WriteLine(vbCrLf & "Select an option:")
                Console.WriteLine("[1] Run the executable again with the same input")
                Console.WriteLine("[2] Exit")
                Console.Write("Press 1 or 2: ")

                Dim retryKey As ConsoleKeyInfo
                Do
                    retryKey = Console.ReadKey(True)
                Loop While retryKey.Key = ConsoleKey.Enter ' Ignore Enter key

                Select Case retryKey.KeyChar
                    Case "1"c
                        retry = True
                    Case "2"c
                        Return ' Exit immediately
                    Case Else
                        Console.WriteLine(vbCrLf & "Invalid choice. Try again.")
                        Continue Do
                End Select
                Exit Do
            Loop


            Console.WriteLine(vbCrLf & "Running again...")
        Loop
    End Sub
End Module
