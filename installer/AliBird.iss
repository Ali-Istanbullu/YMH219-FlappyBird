; ================================================================================
;  AliBird kurulum betigi (Inno Setup) - Adim 11, K18, R3
;
;  Once uygulamayi yayinla (repo kokunden):
;    dotnet publish src/AliBird.WinForms -c Release -r win-x64 --self-contained false -o publish/AliBird
;  Sonra bu dosyayi Inno Setup ile derle (ISCC.exe installer\AliBird.iss).
;  Cikti: installer\Output\AliBird-Kurulum-1.0.0.exe  (repoya girmez, GitHub Releases'e yuklenir, K1)
;
;  Requirement (R3): .NET 10 Desktop Runtime (x64) pakete GOMULMEZ (kurulum kucuk kalir).
;  Kurulum runtime'i kontrol eder; yoksa Microsoft'tan indirir ve sessizce kurar.
;  Indirme ya da kurulum basarisiz olursa hata gosterilir ve kurulum ilerlemez.
; ================================================================================

#define UygulamaAdi "AliBird"
#define UygulamaSurumu "1.0.0"
#define Yayinci "Ali İstanbullu"
#define ExeAdi "AliBird.exe"
#define YayinKlasoru "..\publish\AliBird"

; Microsoft'un "her zaman son .NET 10 Desktop Runtime x64" kalici baglantisi (aka.ms/dotnet/<kanal>/...)
#define RuntimeUrl "https://aka.ms/dotnet/10.0/windowsdesktop-runtime-win-x64.exe"
#define RuntimeDosyasi "windowsdesktop-runtime-win-x64.exe"

[Setup]
AppId={{0446C720-E1C9-4AB8-8982-CDF17EAA94B9}
AppName={#UygulamaAdi}
AppVersion={#UygulamaSurumu}
AppPublisher={#Yayinci}
; K18: Program Files\AliBird, yonetici yetkisi gerekir
DefaultDirName={autopf}\{#UygulamaAdi}
DefaultGroupName={#UygulamaAdi}
PrivilegesRequired=admin
; K2: sadece Windows x64
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=Output
OutputBaseFilename={#UygulamaAdi}-Kurulum-{#UygulamaSurumu}
SetupIconFile=..\assets\alibird.ico
UninstallDisplayIcon={app}\{#ExeAdi}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
; Kaldirma AppData'ya (%LocalAppData%\AliBird) DOKUNMAZ: rekor ve loglar kalir,
; yeniden kurulunca rekor geri gelir (Ali'nin istegi).

[Languages]
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"

[Tasks]
; K18: kisayollar istege bagli
Name: "masaustu"; Description: "Masaüstüne kısayol oluştur"; GroupDescription: "Kısayollar:"
Name: "baslatmenusu"; Description: "Başlat menüsüne kısayol oluştur"; GroupDescription: "Kısayollar:"

[Files]
Source: "{#YayinKlasoru}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autodesktop}\{#UygulamaAdi}"; Filename: "{app}\{#ExeAdi}"; Tasks: masaustu
Name: "{group}\{#UygulamaAdi}"; Filename: "{app}\{#ExeAdi}"; Tasks: baslatmenusu
Name: "{group}\{#UygulamaAdi} Kaldır"; Filename: "{uninstallexe}"; Tasks: baslatmenusu

[Run]
Filename: "{app}\{#ExeAdi}"; Description: "AliBird'ü başlat"; Flags: nowait postinstall skipifsilent

[Code]
var
  IndirmeSayfasi: TDownloadWizardPage;   // indirilen dosya {tmp}'ye gelir, kurulum bitince silinir

// .NET 10 Desktop Runtime (x64) kurulu mu? Microsoft'un kurdugu klasorde 10.x surumu aranir.
function RuntimeKuruluMu: Boolean;
var
  Bulunan: TFindRec;
begin
  Result := FindFirst(ExpandConstant('{commonpf64}\dotnet\shared\Microsoft.WindowsDesktop.App\10.*'), Bulunan);
  if Result then
    FindClose(Bulunan);
end;

procedure InitializeWizard;
begin
  IndirmeSayfasi := CreateDownloadPage('.NET 10 Desktop Runtime indiriliyor',
    'AliBird için gerekli .NET 10 Desktop Runtime indiriliyor, lütfen bekleyin.', nil);
end;

// Runtime'i sessizce kurar. Basarisizsa False ve hata mesaji.
function RuntimeKur(var Hata: String): Boolean;
var
  Sayfa: TOutputMarqueeProgressWizardPage;
  CikisKodu: Integer;
begin
  Result := False;
  Sayfa := CreateOutputMarqueeProgressPage('.NET 10 Desktop Runtime kuruluyor',
    'Bu birkaç dakika sürebilir, lütfen bekleyin.');
  Sayfa.Show;
  try
    Sayfa.Animate;
    if not Exec(ExpandConstant('{tmp}\{#RuntimeDosyasi}'), '/install /quiet /norestart', '',
                SW_HIDE, ewWaitUntilTerminated, CikisKodu) then
    begin
      Hata := 'Runtime kurulum programı çalıştırılamadı: ' + SysErrorMessage(CikisKodu);
      Exit;
    end;
    // 0 = basarili, 3010/1641 = basarili ama yeniden baslatma gerekli
    if (CikisKodu <> 0) and (CikisKodu <> 3010) and (CikisKodu <> 1641) then
    begin
      Hata := 'Runtime kurulumu başarısız oldu (çıkış kodu ' + IntToStr(CikisKodu) + ').';
      Exit;
    end;
    if not RuntimeKuruluMu then
    begin
      Hata := 'Runtime kuruldu görünüyor ama bulunamadı.';
      Exit;
    end;
    Result := True;
  finally
    Sayfa.Hide;
  end;
end;

// "Kur" dugmesine basilinca: runtime yoksa indir + kur. Hata olursa kurulum ilerlemez (K18).
function NextButtonClick(CurPageID: Integer): Boolean;
var
  Hata: String;
begin
  Result := True;
  if (CurPageID <> wpReady) or RuntimeKuruluMu then
    Exit;

  IndirmeSayfasi.Clear;
  IndirmeSayfasi.Add('{#RuntimeUrl}', '{#RuntimeDosyasi}', '');
  IndirmeSayfasi.Show;
  try
    try
      IndirmeSayfasi.Download;
    except
      if not IndirmeSayfasi.AbortedByUser then
        SuppressibleMsgBox('.NET 10 Desktop Runtime indirilemedi. İnternet bağlantınızı kontrol edip tekrar deneyin.' + #13#10#13#10 +
          AddPeriod(GetExceptionMessage), mbCriticalError, MB_OK, IDOK);
      Result := False;
      Exit;
    end;
  finally
    IndirmeSayfasi.Hide;
  end;

  if not RuntimeKur(Hata) then
  begin
    SuppressibleMsgBox(Hata + #13#10#13#10 + 'AliBird kurulamadı.', mbCriticalError, MB_OK, IDOK);
    Result := False;
  end;
end;
