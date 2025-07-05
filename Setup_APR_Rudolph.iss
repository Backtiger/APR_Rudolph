[Setup]
AppName=APR_Rudolph
AppVersion=1.0
DefaultDirName={pf}\APR_Rudolph
DefaultGroupName=APR_Rudolph
OutputDir=.
OutputBaseFilename=APR_Rudolph_Installer
Compression=lzma
SolidCompression=yes

[Files]
Source: "bin\\Debug\\net8.0-windows\\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs

[Icons]
Name: "{group}\APR_Rudolph"; Filename: "{app}\APR_Rudolph.exe"
Name: "{commondesktop}\APR_Rudolph"; Filename: "{app}\APR_Rudolph.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "바탕화면에 바로가기 만들기"; GroupDescription: "추가 아이콘:"; Flags: unchecked 