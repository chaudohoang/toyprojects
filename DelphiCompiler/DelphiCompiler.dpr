program DelphiCompiler;

{$APPTYPE CONSOLE}

uses
  System.SysUtils, System.IOUtils, Winapi.Windows, Winapi.ShellAPI;

function CleanPath(const Path: string): string;
begin
  Result := Path;

  // Remove surrounding quotes if they exist
  if (Length(Result) > 1) and (Result[1] = '"') and (Result[Length(Result)] = '"') then
    Result := Copy(Result, 2, Length(Result) - 2);
end;



function ExecuteProcess(Command: string): Integer;
var
  StartupInfo: TStartupInfo;
  ProcessInfo: TProcessInformation;
  ExitCode: DWORD;
  CommandBuffer: array[0..MAX_PATH] of Char;
begin
  Result := -1; // Default to error
  ZeroMemory(@StartupInfo, SizeOf(TStartupInfo));
  ZeroMemory(@ProcessInfo, SizeOf(TProcessInformation));
  StartupInfo.cb := SizeOf(TStartupInfo);
  StartupInfo.dwFlags := STARTF_USESHOWWINDOW;
  StartupInfo.wShowWindow := SW_SHOWNORMAL;

  StrPCopy(CommandBuffer, Command);

  if CreateProcess(nil, CommandBuffer, nil, nil, False, 0, nil, nil, StartupInfo, ProcessInfo) then
  begin
    // Wait for the process to complete
    WaitForSingleObject(ProcessInfo.hProcess, INFINITE);
    GetExitCodeProcess(ProcessInfo.hProcess, ExitCode);
    CloseHandle(ProcessInfo.hProcess);
    CloseHandle(ProcessInfo.hThread);

    Result := ExitCode; // Return the actual exit code of the process
  end;
end;


procedure CompileSource(var SourceFile: string);
var
  SourceFolder, OutputFolder, OutputFile, Command: string;
  ExitCode: Integer;
begin
  // Extract the folder of the source file
  SourceFolder := ExtractFilePath(SourceFile);

  // Ensure SourceFolder is not empty
  if SourceFolder = '' then
    SourceFolder := GetCurrentDir;

  // Define the "Output" folder within the same directory
  OutputFolder := IncludeTrailingPathDelimiter(SourceFolder) + 'Output';

  // Ensure the folder exists, create it if necessary
  if not DirectoryExists(OutputFolder) then
  begin
    Writeln('Creating output folder: ', OutputFolder);
    if not ForceDirectories(OutputFolder) then
    begin
      Writeln('Error: Failed to create output folder: ', OutputFolder);
      Exit;
    end;
  end;

  // Define output EXE path inside the "Output" folder
  OutputFile := IncludeTrailingPathDelimiter(OutputFolder) + ExtractFileName(ChangeFileExt(SourceFile, '.exe'));

  // Clean paths to remove unwanted quotes
  SourceFile := CleanPath(SourceFile);
  OutputFile := CleanPath(OutputFile);

   // Debugging output
  Writeln('DEBUG: SourceFolder = ', SourceFolder);
  Writeln('DEBUG: OutputFolder = ', OutputFolder);
  Writeln('DEBUG: Cleaned SourceFile = ', SourceFile);
  Writeln('DEBUG: Cleaned OutputFile = ', OutputFile);
  Writeln('Compiling: ', SourceFile);
  Writeln('Output: ', OutputFile);

  // 🔹 Fix: Properly format the command to prevent quote issues
  Command := Format('dcc32.exe "%s" -E"%s"', [SourceFile, OutputFolder]);

  ExitCode := ExecuteProcess(Command);

  if ExitCode <> 0 then
  begin
    Writeln('Error: Compilation failed with exit code ', ExitCode);
    Readln; // Keep console open
  end
  else
    Writeln('Compilation completed successfully.');
end;



procedure ProcessCommandLine;
var
  I: Integer;
  IsDragDrop: Boolean;
  SourceFile: string;
begin
  IsDragDrop := (ParamCount = 1) and FileExists(ParamStr(1));

  Writeln('DEBUG: ParamCount = ', ParamCount);
  Writeln('DEBUG: ParamStr(1) = ', ParamStr(1));
  Writeln('DEBUG: IsDragDrop = ', BoolToStr(IsDragDrop, True));

  if ParamCount = 0 then
  begin
    Writeln('Usage: DelphiCompiler.exe <filename.pas>');
    Writeln('Drag and drop a .pas file onto this window to compile.');
    Readln;
    Exit;
  end;

  for I := 1 to ParamCount do
  begin
    SourceFile := ParamStr(I);
    if FileExists(SourceFile) then
      CompileSource(SourceFile)
    else
      Writeln('Error: File not found - ', SourceFile);
  end;

  Writeln('Done.');

  // Keep console open if drag-and-drop was used
  if IsDragDrop then
  begin
    Writeln;
    Writeln('Press Enter to exit...');
    Readln;
  end;
end;



begin
  ProcessCommandLine;
end.

