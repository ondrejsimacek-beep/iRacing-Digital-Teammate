#ifndef MyAppVersion
  #error MyAppVersion must be provided by build-installer.ps1
#endif

#define MyAppName "iRacing Digital Teammate"
#define MyAppPublisher "Digital Downforce Sim Racing"
#define MyAppExeName "iRacing Digital Teammate.exe"
#define MyAppUrl "https://github.com/ondrejsimacek-beep/iRacing-Teammate"

[Setup]
AppId={{7C194F19-7362-4DB8-8AB1-7CF09BC78D50}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppUrl}
AppSupportURL={#MyAppUrl}/issues
AppUpdatesURL={#MyAppUrl}/releases/latest
DefaultDirName={localappdata}\Programs\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
OutputDir=dist
OutputBaseFilename=iRacing-Digital-Teammate-Setup-v{#MyAppVersion}
SetupIconFile=obj\dds-teammate.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
RestartApplications=no
VersionInfoVersion={#MyAppVersion}.0
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription={#MyAppName} Setup
VersionInfoProductName={#MyAppName}
VersionInfoProductVersion={#MyAppVersion}

[Files]
Source: "dist\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "dist\README.md"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{userprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"
Name: "{userprograms}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"

[InstallDelete]
Type: files; Name: "{userprograms}\iRacing Teammate.lnk"

[UninstallDelete]
Type: files; Name: "{userstartup}\iRacing Digital Teammate.lnk"
Type: files; Name: "{userstartup}\iRacing Teammate.lnk"

[Run]
Filename: "{app}\{#MyAppExeName}"; Parameters: "--minimized"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent
