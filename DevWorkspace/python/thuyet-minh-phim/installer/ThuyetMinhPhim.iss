; Bộ cài "Thuyết minh phim" (Inno Setup 6). Build bằng installer\build.ps1, không chạy trực tiếp.
#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif
#define AppName "Thuyết minh phim"
#define AppExe "ThuyetMinhPhim.exe"
#define Root ".."

[Setup]
AppId={{6C1C3E3B-8E4B-4F8A-9F3B-4C2A7D5E1A10}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher=thuyet-minh-phim
DefaultDirName={autopf}\ThuyetMinhPhim
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
; Mặc định cài cho mọi người dùng (cần quyền admin để cài thêm giọng Microsoft An);
; người dùng có thể chọn "chỉ cho tôi" nếu không có quyền admin.
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog commandline
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.17763
OutputDir={#Root}\dist\setup
OutputBaseFilename=ThuyetMinhPhim-Setup-{#AppVersion}
SetupIconFile={#Root}\installer\icon.ico
UninstallDisplayIcon={app}\{#AppExe}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes

[Languages]
Name: "vi"; MessagesFile: "Vietnamese.isl"

[CustomMessages]
vi.VoiceTask=Cài giọng đọc offline Microsoft An (tiếng Việt) của Windows — cần Internet, có thể mất vài phút
vi.VoiceStatus=Đang cài giọng Microsoft An (tiếng Việt) từ Windows Update, vui lòng chờ...
vi.VoiceGroup=Giọng đọc:
vi.LaunchApp=Mở Thuyết minh phim

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "installvoice"; Description: "{cm:VoiceTask}"; GroupDescription: "{cm:VoiceGroup}"; Check: NeedVoice

[Files]
Source: "{#Root}\build\app\ThuyetMinhPhim\*"; Excludes: "portable.txt,\data"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#Root}\README.md"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Run]
Filename: "{sys}\WindowsPowerShell\v1.0\powershell.exe"; \
  Parameters: "-NoProfile -ExecutionPolicy Bypass -Command ""Add-WindowsCapability -Online -Name 'Language.TextToSpeech~~~vi-VN~0.0.1.0'"""; \
  StatusMsg: "{cm:VoiceStatus}"; Flags: runhidden waituntilterminated; Tasks: installvoice
Filename: "{app}\{#AppExe}"; Description: "{cm:LaunchApp}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Cache giọng đọc có thể nặng vài trăm MB; giữ lại config.json
Type: filesandordirs; Name: "{userappdata}\ThuyetMinhPhim\cache"
Type: filesandordirs; Name: "{userappdata}\ThuyetMinhPhim\debug"
; Mô hình giọng VieNeu app tự tải (~520 MB)
Type: filesandordirs; Name: "{userappdata}\ThuyetMinhPhim\models"

[Code]
function NeedVoice: Boolean;
begin
  { Chỉ hỏi khi đang cài với quyền admin và máy chưa có giọng An }
  Result := IsAdminInstallMode and
    not RegKeyExists(HKLM64, 'SOFTWARE\Microsoft\Speech_OneCore\Voices\Tokens\MSTTS_V110_viVN_An');
end;
