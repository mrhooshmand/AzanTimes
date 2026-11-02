; AzanTimes setup. Unicode script, UTF-8 with BOM.

#define MyAppName "AzanTimes"
#define MyAppExeName "AzanTimes.exe"
#define MyAppVersion "1.0.0"

[Setup]
AppId={{8F3A1C64-2B7E-4D19-9A55-6C0E4F2A8B17}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher=AzanTimes
DefaultDirName={localappdata}\AzanTimes
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
OutputDir=..\dist
OutputBaseFilename=AzanTimes-Setup
SetupIconFile=..\src\AzanTimes\Assets\app.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
CloseApplications=no

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"

[Messages]
SetupAppRunningError=AzanTimes در حال اجراست.%n%nبرنامه را ببندید و تأیید را بزنید، یا انصراف دهید.
ButtonBack=< قبلی
ButtonNext=بعدی >
ButtonInstall=نصب
ButtonCancel=انصراف
ButtonFinish=پایان
ButtonBrowse=انتخاب...
WelcomeLabel1=نصب AzanTimes
WelcomeLabel2=اوقات شرعی شهرهای ایران روی ویجت دسکتاپ و با پاپ‌آپ نشان داده می‌شود.%n%nنیازی به نصب جداگانه دات‌نت یا کتابخانه دیگری نیست.%n%nدر مرحله بعد شهر را انتخاب کنید. بعد از نصب هم از تنظیمات می‌توانید شهر را عوض کنید.
ClickNext=برای ادامه بعدی را بزنید.
WizardSelectDir=محل نصب
SelectDirDesc=AzanTimes کجا نصب شود؟
SelectDirLabel3=برنامه در این پوشه نصب می‌شود.
SelectDirBrowseLabel=برای ادامه بعدی را بزنید. برای تغییر پوشه، انتخاب را بزنید.
WizardReady=آماده نصب
ReadyLabel1=نصب AzanTimes شروع می‌شود.
ReadyLabel2a=نصب را بزنید. برای تغییر شهر یا پوشه، قبلی را بزنید.
FinishedHeadingLabel=نصب تمام شد
FinishedLabel=AzanTimes نصب شد. می‌توانید برنامه را اجرا کنید تا اوقات شرعی شهر انتخاب‌شده نشان داده شود.
FinishedLabelNoIcons=AzanTimes نصب شد.
ClickFinish=برای بستن پایان را بزنید.

[Files]
Source: "..\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "cities.txt"; DestDir: "{tmp}"; Flags: dontcopy

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "AzanTimes"; ValueData: """{app}\{#MyAppExeName}"""; Flags: uninsdeletevalue

[Icons]
Name: "{autoprograms}\AzanTimes"; Filename: "{app}\{#MyAppExeName}"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "اجرای AzanTimes"; Flags: nowait postinstall skipifsilent

[Code]
var
  CityPage: TWizardPage;
  CityCombo: TNewComboBox;
  CityIds: TStringList;

procedure InitializeWizard;
var
  Lines: TArrayOfString;
  I, P: Integer;
  Line, Id, Name, CityFile: String;
begin
  CityPage := CreateCustomPage(wpSelectDir,
    'انتخاب شهر',
    'اوقات شرعی برای این شهر محاسبه می‌شود. بعد از نصب هم از منوی کنار ساعت، تنظیمات، می‌توانید شهر را عوض کنید.');

  CityCombo := TNewComboBox.Create(CityPage);
  CityCombo.Parent := CityPage.Surface;
  CityCombo.Left := 0;
  CityCombo.Top := 16;
  CityCombo.Width := CityPage.SurfaceWidth;
  CityCombo.Style := csDropDownList;

  CityIds := TStringList.Create;
  ExtractTemporaryFile('cities.txt');
  CityFile := ExpandConstant('{tmp}\cities.txt');
  if LoadStringsFromFile(CityFile, Lines) then
  begin
    for I := 0 to GetArrayLength(Lines) - 1 do
    begin
      Line := Trim(Lines[I]);
      if Line = '' then
        Continue;
      P := Pos('|', Line);
      if P <= 1 then
        Continue;
      Id := Copy(Line, 1, P - 1);
      Name := Copy(Line, P + 1, MaxInt);
      CityIds.Add(Id);
      CityCombo.Items.Add(Name);
      if CompareText(Id, 'tehran') = 0 then
        CityCombo.ItemIndex := CityCombo.Items.Count - 1;
    end;
  end;
  if CityCombo.ItemIndex < 0 then
    CityCombo.ItemIndex := 0;
end;

procedure SaveSelectedCity;
var
  Dir, Id: String;
begin
  if (CityIds = nil) or (CityCombo.ItemIndex < 0) then
    Exit;
  Id := CityIds[CityCombo.ItemIndex];
  Dir := ExpandConstant('{userappdata}\AzanTimes');
  ForceDirectories(Dir);
  SaveStringToFile(Dir + '\selected-city.txt', Id, False);
  if not FileExists(Dir + '\settings.json') then
    SaveStringToFile(Dir + '\settings.json',
      '{' + #13#10 + '  "cityId": "' + Id + '"' + #13#10 + '}' + #13#10, False);
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  ResultCode: Integer;
begin
  Exec(ExpandConstant('{sys}\taskkill.exe'), '/IM AzanTimes.exe /F', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Result := '';
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
    SaveSelectedCity;
end;

procedure DeinitializeSetup;
begin
  if CityIds <> nil then
  begin
    CityIds.Free;
    CityIds := nil;
  end;
end;
