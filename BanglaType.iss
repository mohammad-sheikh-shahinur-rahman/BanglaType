#define MyAppName "BanglaType"
#define MyAppVersion "1.0.6"
#define MyAppPublisher "Mohammad Sheikh Shahinur Rahman"
#define MyAppExeName "BanglaType.exe"

[Setup]
AppId={{E63A8A01-30AB-41C9-92A2-551428AB4B49}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL=https://amadersomaj.com
AppSupportURL=https://amadersomaj.com
AppUpdatesURL=https://amadersomaj.com
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
PrivilegesRequired=admin

; Build output
OutputDir=Output
OutputBaseFilename=BanglaTypeInstaller-{#MyAppVersion}
SetupIconFile=banglatype.ico

; Compression
Compression=lzma2
SolidCompression=yes
WizardStyle=modern

; Architecture (পরিবর্তন করুন যদি x86 অ্যাপ হয়)
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

; Optional
UninstallDisplayIcon={app}\{#MyAppExeName}
SetupLogging=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; \
    GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "startup"; Description: "Run BanglaType at Windows startup"; \
    GroupDescription: "Startup"; Flags: unchecked

[Files]
Source: "bin\Release\BanglaType.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\BanglaType.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\BanglaType.xml"; DestDir: "{app}"; Flags: ignoreversion

Source: "bin\Release\Resources\*"; DestDir: "{app}\Resources"; \
    Flags: ignoreversion recursesubdirs createallsubdirs

Source: "data\*"; DestDir: "{app}\data"; \
    Flags: ignoreversion recursesubdirs createallsubdirs

; Visual C++ Redistributables
Source: "bin\Redist\vc_redist.x86.exe"; DestDir: "{tmp}"; \
    Flags: deleteafterinstall

Source: "bin\Redist\vc_redist.x64.exe"; DestDir: "{tmp}"; \
    Flags: deleteafterinstall; Check: Is64BitInstallMode

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; \
    Tasks: desktopicon
Name: "{userstartup}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; \
    Tasks: startup

[Run]
Filename: "{tmp}\vc_redist.x86.exe"; \
    Parameters: "/quiet /norestart"; \
    StatusMsg: "Installing Visual C++ Redistributable (x86)..."; \
    Check: NotVCRedistx86Installed

Filename: "{tmp}\vc_redist.x64.exe"; \
    Parameters: "/quiet /norestart"; \
    StatusMsg: "Installing Visual C++ Redistributable (x64)..."; \
    Check: Is64BitInstallMode and NotVCRedistx64Installed

Filename: "{app}\{#MyAppExeName}"; \
    Description: "Launch {#MyAppName}"; \
    Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}\data"
Type: filesandordirs; Name: "{userappdata}\BanglaType"

[Code]

function NotVCRedistx86Installed: Boolean;
var
  Installed: Cardinal;
begin
  Result := True;
  if RegQueryDWordValue(
       HKLM,
       'SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x86',
       'Installed',
       Installed) then
    Result := Installed <> 1;
end;

function NotVCRedistx64Installed: Boolean;
var
  Installed: Cardinal;
begin
  Result := True;
  if RegQueryDWordValue(
       HKLM,
       'SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64',
       'Installed',
       Installed) then
    Result := Installed <> 1;
end;