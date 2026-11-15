; Akhil Face ID installer (Inno Setup 6).
; Production Windows 11 installation package for Akhil Face ID.
; Bundles the native biometric engine, ONNX runtime, pre-trained offline models,
; WPF control panel, system tray integration, and optional Credential Provider.

#define AppName "Akhil Face ID"
#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif
#define Root ".."
#define Build Root + "\build"
#define AppBin Root + "\app\WinFace\bin\publish"
#define UpdBin Root + "\app\WinFaceUpdater\bin\publish"
#define Clsid "{{C188DC15-E41E-4CCF-9DA9-8238E1D0BBDF}"
#define ClsidPlain "{C188DC15-E41E-4CCF-9DA9-8238E1D0BBDF}"

[Setup]
AppId={{9B7E412A-5120-4F8A-A5E8-3C9A621D58B4}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher=Akhil Face ID
AppPublisherURL=
AppSupportURL=
DefaultDirName={autopf}\Akhil Face ID
DisableProgramGroupPage=yes
DisableDirPage=auto
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.18362
OutputDir={#Root}\dist
OutputBaseFilename=AkhilFaceID-Setup-x64
SetupIconFile={#Root}\app\WinFace\akhilfaceid.ico
UninstallDisplayIcon={app}\AkhilFaceID.exe
UninstallDisplayName={#AppName}
WizardStyle=modern
WizardImageFile={#Root}\assets\wizard.bmp
WizardSmallImageFile={#Root}\assets\wizard_small.bmp
LicenseFile={#Root}\PRIVACY.md
Compression=lzma2/ultra64
SolidCompression=yes
CloseApplications=yes
RestartApplications=yes
VersionInfoVersion={#AppVersion}
VersionInfoDescription=Akhil Face ID Windows 11 Setup

[Tasks]
Name: desktopicon; Description: "Create a Desktop shortcut"; Flags: unchecked
Name: autostart;   Description: "Launch Akhil Face ID automatically on Windows sign-in"; Flags: unchecked

[Files]
; Main Application Binaries
Source: "{#AppBin}\AkhilFaceID.exe";                DestDir: "{app}"; Flags: ignoreversion
Source: "{#AppBin}\AkhilFaceID.dll";                DestDir: "{app}"; Flags: ignoreversion
Source: "{#AppBin}\AkhilFaceID.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#AppBin}\AkhilFaceID.deps.json";          DestDir: "{app}"; Flags: ignoreversion

; Legacy Compatibility Aliases
Source: "{#AppBin}\WinFace.exe";                    DestDir: "{app}"; Flags: ignoreversion
Source: "{#AppBin}\WinFace.dll";                    DestDir: "{app}"; Flags: ignoreversion
Source: "{#AppBin}\WinFace.runtimeconfig.json";     DestDir: "{app}"; Flags: ignoreversion
Source: "{#AppBin}\WinFace.deps.json";              DestDir: "{app}"; Flags: ignoreversion

; Native C++ Engine Components
Source: "{#Build}\WinFaceCP.dll";                   DestDir: "{app}"; Flags: ignoreversion
Source: "{#Build}\fgsetup.exe";                     DestDir: "{app}"; Flags: ignoreversion
Source: "{#Build}\fgcredtest.exe";                  DestDir: "{app}"; Flags: ignoreversion
Source: "{#Build}\fgsound.exe";                     DestDir: "{app}"; Flags: ignoreversion
Source: "{#Build}\onnxruntime.dll";                 DestDir: "{app}"; Flags: ignoreversion

; Offline Pre-Trained Neural Network Models (Apache 2.0 & InsightFace)
Source: "{#Root}\models\runtime\face_detector.onnx";           DestDir: "{app}\models"; Flags: ignoreversion
Source: "{#Root}\models\runtime\face_landmarks_detector.onnx"; DestDir: "{app}\models"; Flags: ignoreversion
Source: "{#Root}\models\runtime\fas_v1se_s4.0.onnx";           DestDir: "{app}\models"; Flags: ignoreversion
Source: "{#Root}\models\runtime\fas_v2_s2.7.onnx";             DestDir: "{app}\models"; Flags: ignoreversion
Source: "{#Root}\models\runtime\canonical_face.bin";           DestDir: "{app}\models"; Flags: ignoreversion
Source: "{#Root}\models\runtime\w600k_r50.onnx";               DestDir: "{app}\models"; Flags: ignoreversion

; Application Assets & Recovery Docs
Source: "{#Root}\assets\sfx_unlock.wav";            DestDir: "{app}\assets"; Flags: ignoreversion
Source: "{#Root}\assets\tile.bmp";                  DestDir: "{app}\assets"; Flags: ignoreversion
Source: "{#Root}\assets\akhilfaceid.png";           DestDir: "{app}\assets"; Flags: ignoreversion
Source: "{#Root}\assets\akhilfaceid.ico";           DestDir: "{app}\assets"; Flags: ignoreversion
Source: "{#Root}\install\RECOVERY.md";              DestDir: "{app}"; Flags: ignoreversion
Source: "{#Root}\PRIVACY.md";                       DestDir: "{app}"; Flags: ignoreversion

; Background Updater
Source: "{#UpdBin}\WinFaceUpdater.exe";                DestDir: "{app}"; Flags: ignoreversion
Source: "{#UpdBin}\WinFaceUpdater.dll";                DestDir: "{app}"; Flags: ignoreversion
Source: "{#UpdBin}\WinFaceUpdater.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#UpdBin}\WinFaceUpdater.deps.json";          DestDir: "{app}"; Flags: ignoreversion

[Dirs]
Name: "{commonappdata}\WinFace"; Permissions: system-full admins-full users-readexec; Flags: uninsneveruninstall

[Registry]
; COM Server & Credential Provider Registration (Safe: Disabled by default)
Root: HKLM64; Subkey: "SOFTWARE\Classes\CLSID\{#Clsid}"; ValueType: string; ValueName: ""; ValueData: "Akhil Face ID"; Flags: uninsdeletekey
Root: HKLM64; Subkey: "SOFTWARE\Classes\CLSID\{#Clsid}\InprocServer32"; ValueType: string; ValueName: ""; ValueData: "{app}\WinFaceCP.dll"
Root: HKLM64; Subkey: "SOFTWARE\Classes\CLSID\{#Clsid}\InprocServer32"; ValueType: string; ValueName: "ThreadingModel"; ValueData: "Apartment"
Root: HKLM64; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\Credential Providers\{#Clsid}"; ValueType: string; ValueName: ""; ValueData: "Akhil Face ID"; Flags: uninsdeletekey

; Disabled by default until user completes guided setup
Root: HKLM64; Subkey: "SOFTWARE\WinFace"; ValueType: dword; ValueName: "Enabled"; ValueData: "0"; Flags: createvalueifdoesntexist uninsdeletekey

; Enable password authentication alongside PIN for Microsoft accounts (ensures hands-free unlock works smoothly)
Root: HKLM64; Subkey: "SOFTWARE\Microsoft\Windows NT\CurrentVersion\PasswordLess\Device"; ValueType: dword; ValueName: "DevicePasswordLessBuildVersion"; ValueData: "0"

; User Startup Option
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "AkhilFaceID"; ValueData: """{app}\AkhilFaceID.exe"" --minimized"; Tasks: autostart; Flags: uninsdeletevalue

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\AkhilFaceID.exe"; Comment: "Akhil Face ID - AI Facial Recognition"
Name: "{autodesktop}\{#AppName}";  Filename: "{app}\AkhilFaceID.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\AkhilFaceID.exe"; Description: "Launch Akhil Face ID"; Flags: postinstall nowait skipifsilent shellexec

[UninstallDelete]
Type: files; Name: "{app}\WinFaceCP.old.*.dll"
Type: files; Name: "{app}\onnxruntime.old.*.dll"
Type: dirifempty; Name: "{app}\models"
Type: dirifempty; Name: "{app}\assets"
Type: dirifempty; Name: "{app}"

[Messages]
WizardLicense=Privacy Policy &amp; Biometric Security Guarantee
LicenseLabel=Please review the privacy policy and security terms before proceeding.
LicenseLabel3=All biometric processing is performed 100% locally on your PC. No camera data is ever uploaded.
LicenseAccepted=I have read and &agree to the terms and privacy policy
LicenseNotAccepted=I do &not agree
FinishedLabel=Akhil Face ID is installed successfully.%n%nLaunch Akhil Face ID to enroll your face profile. Your standard Windows PIN and password will always remain available.

[Code]
var
  SacPage: TWizardPage;
  SacStatus: TNewStaticText;
  TermsPage: TWizardPage;
  TermsAccept: TNewCheckBox;

function SacState: Integer;
var
  V: Cardinal;
begin
  Result := -1;
  if RegQueryDWordValue(HKEY_LOCAL_MACHINE, 'SYSTEM\CurrentControlSet\Control\CI\Policy',
                        'VerifiedAndReputablePolicyState', V) then
    Result := Integer(V);
end;

procedure ShowSac;
begin
  case SacState of
    1: SacStatus.Caption := 'Smart App Control is ON. It blocks unsigned Credential Provider DLLs.' + #13#10#13#10 +
         'Please open Windows Security > App & browser control > Smart App Control settings, choose Off or Evaluation, then click "Check again".';
    2: SacStatus.Caption := 'Smart App Control is in Evaluation mode. Face ID will function normally.' + #13#10#13#10 +
         'You may proceed with installation.';
  else
    SacStatus.Caption := 'Smart App Control is off. Hardware authentication is fully supported.';
  end;
end;

procedure OpenSecurity(Sender: TObject);
var
  Code: Integer;
begin
  ShellExec('open', 'windowsdefender://appbrowser/', '', '', SW_SHOWNORMAL, ewNoWait, Code);
end;

procedure CheckAgain(Sender: TObject);
begin
  ShowSac;
end;

procedure TermsClicked(Sender: TObject);
begin
  WizardForm.NextButton.Enabled := TermsAccept.Checked;
end;

procedure InitializeWizard;
var
  B1, B2: TNewButton;
  Memo: TNewMemo;
begin
  SacPage := CreateCustomPage(wpWelcome, 'Windows Security Verification', 'Smart App Control status check');
  SacStatus := TNewStaticText.Create(SacPage);
  SacStatus.Parent := SacPage.Surface;
  SacStatus.AutoSize := False;
  SacStatus.WordWrap := True;
  SacStatus.Width := SacPage.SurfaceWidth;
  SacStatus.Height := ScaleY(120);
  
  B1 := TNewButton.Create(SacPage);
  B1.Parent := SacPage.Surface;
  B1.Caption := 'Open Windows Security';
  B1.Width := ScaleX(170);
  B1.Height := ScaleY(26);
  B1.Top := SacStatus.Top + SacStatus.Height + ScaleY(8);
  B1.OnClick := @OpenSecurity;

  B2 := TNewButton.Create(SacPage);
  B2.Parent := SacPage.Surface;
  B2.Caption := 'Check again';
  B2.Width := ScaleX(110);
  B2.Height := ScaleY(26);
  B2.Top := B1.Top;
  B2.Left := B1.Left + B1.Width + ScaleX(8);
  B2.OnClick := @CheckAgain;

  TermsPage := CreateCustomPage(SacPage.ID, 'InsightFace Model Terms', 'Recognition Model Non-Commercial License Agreement');
  Memo := TNewMemo.Create(TermsPage);
  Memo.Parent := TermsPage.Surface;
  Memo.Width := TermsPage.SurfaceWidth;
  Memo.Height := ScaleY(160);
  Memo.ReadOnly := True;
  Memo.ScrollBars := ssVertical;
  Memo.Text :=
    'Akhil Face ID uses the deep insight ArcFace recognition model (w600k_r50.onnx).' + #13#10#13#10 +
    'The model is provided for non-commercial personal / research use under the InsightFace license.' + #13#10#13#10 +
    'All inference runs entirely on-device on your laptop.' + #13#10#13#10 +
    'By checking the box below, you agree to use the recognition model for personal use.';

  TermsAccept := TNewCheckBox.Create(TermsPage);
  TermsAccept.Parent := TermsPage.Surface;
  TermsAccept.Top := Memo.Top + Memo.Height + ScaleY(10);
  TermsAccept.Width := TermsPage.SurfaceWidth;
  TermsAccept.Caption := 'I accept the InsightFace non-commercial license for on-device recognition';
  TermsAccept.Checked := True;
  TermsAccept.OnClick := @TermsClicked;
end;

function ShouldSkipPage(PageID: Integer): Boolean;
begin
  Result := False;
  if PageID = SacPage.ID then Result := SacState <= 0;
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  ResultCode: Integer;
begin
  Result := '';
  Exec('taskkill.exe', '/F /IM AkhilFaceID.exe', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Exec('taskkill.exe', '/F /IM WinFace.exe', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Exec('taskkill.exe', '/F /IM WinFaceUpdater.exe', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Exec('taskkill.exe', '/F /IM fgsetup.exe', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Sleep(500);
end;

procedure CurPageChanged(CurPageID: Integer);
begin
  if CurPageID = SacPage.ID then ShowSac;
  if CurPageID = TermsPage.ID then WizardForm.NextButton.Enabled := TermsAccept.Checked;
end;
