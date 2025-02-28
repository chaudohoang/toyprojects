program DelphiCompiler2;

{$APPTYPE CONSOLE}

uses
  SysUtils, Classes, Windows;

function CleanPath(const Path: string): string;
begin
  Result := Trim(Path);
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
  Result := -1;
  ZeroMemory(@StartupInfo, SizeOf(TStartupInfo));
  ZeroMemory(@ProcessInfo, SizeOf(TProcessInformation));
  StartupInfo.cb := SizeOf(TStartupInfo);
  StartupInfo.dwFlags := STARTF_USESHOWWINDOW;
  StartupInfo.wShowWindow := SW_SHOWNORMAL;

  StrPCopy(CommandBuffer, Command);

  if CreateProcess(nil, CommandBuffer, nil, nil, False, 0, nil, nil, StartupInfo, ProcessInfo) then
  begin
    WaitForSingleObject(ProcessInfo.hProcess, INFINITE);
    GetExitCodeProcess(ProcessInfo.hProcess, ExitCode);
    CloseHandle(ProcessInfo.hProcess);
    CloseHandle(ProcessInfo.hThread);
    Result := ExitCode;
  end;
end;

procedure CompileSource(var SourceFile: string);
var
  SourceFolder, OutputFolder, OutputFile, Command: string;
  ExitCode: Integer;
begin
  SourceFolder := ExtractFilePath(SourceFile);
  if SourceFolder = '' then
    SourceFolder := GetCurrentDir;

  OutputFolder := IncludeTrailingPathDelimiter(SourceFolder) + 'Output';
  if not DirectoryExists(OutputFolder) then
  begin
    Writeln('Creating output folder: ', OutputFolder);
    if not ForceDirectories(OutputFolder) then
    begin
      Writeln('Error: Failed to create output folder: ', OutputFolder);
      Exit;
    end;
  end;

  OutputFile := IncludeTrailingPathDelimiter(OutputFolder) + ExtractFileName(ChangeFileExt(SourceFile, '.exe'));

  SourceFile := CleanPath(SourceFile);
  OutputFile := CleanPath(OutputFile);

  Writeln('DEBUG: SourceFolder = ', SourceFolder);
  Writeln('DEBUG: OutputFolder = ', OutputFolder);
  Writeln('DEBUG: Cleaned SourceFile = ', SourceFile);
  Writeln('DEBUG: Cleaned OutputFile = ', OutputFile);
  Writeln('Compiling: ', SourceFile);
  Writeln('Output: ', OutputFile);

  // Use `fpc.exe` instead of `dcc32.exe`
  Command := Format('fpc -Mdelphi "%s" -o"%s"', [SourceFile, OutputFile]);


  ExitCode := ExecuteProcess(Command);

  if ExitCode <> 0 then
  begin
    Writeln('Error: Compilation failed with exit code ', ExitCode);
    Readln;
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
    Writeln('Usage: DelphiCompiler2.exe <filename.pas>');
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

