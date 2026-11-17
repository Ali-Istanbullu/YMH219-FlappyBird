using AliBird.Core.Abstractions;
using AliBird.Core.Configuration;
using AliBird.Core.Gameplay;
using AliBird.Core.Models;
using AliBird.Core.Presentation;
using AliBird.Infrastructure.Audio;
using AliBird.Infrastructure.Errors;
using AliBird.Infrastructure.FileSystem;
using AliBird.Infrastructure.Logging;
using AliBird.Infrastructure.Persistence;
using AliBird.Infrastructure.Platform;
using AliBird.WinForms.Forms;
using AliBird.WinForms.Platform;
using AliBird.WinForms.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace AliBird.WinForms;

// ================================================================================
//  UYGULAMANIN BASLANGIC NOKTASI ve "KOMPOZISYON KOKU" (Composition Root)
//
//  Projedeki TEK yer burasi ki siniflar birbirine baglanir (M6a, K19, R2):
//  DI container (Microsoft.Extensions.DependencyInjection) sadece burada kurulur.
//  Diger hicbir sinif container'i bilmez; hepsi ihtiyaclarini constructor'dan alir.
//
//  Ayrica burada:
//   - Tek kopya kontrolu (K17)
//   - Genel hata yakalayicilar: gozden kacan bir hata bile uygulamayi COKERTMESIN (R6)
//   - Pencere kapanirken rekorun kaydedilmesi (K52 b)
// ================================================================================
internal static class Program
{
    // Tek kopya kilidinin adi. "Local\" = sadece bu kullanicinin oturumu.
    private const string TekKopyaKilidi = @"Local\AliBird_TekKopya";
    private const string Baslik = "AliBird";

    // Genel hata yakalayicilarin kullandigi bildirici. DI kurulana kadar null'dir.
    private static IHataBildirici? _hataBildirici;

    // CK40: Genel yakalayicinin mesaj kutusu en fazla bir kez acilir; sonrakiler sadece loga gider.
    private static bool _genelHataGosterildi;

    [STAThread]
    private static void Main()
    {
        // 1) Genel hata yakalayicilar: HER SEYDEN ONCE kurulur (R6, M24 #8).
        GenelHataYakalayicilariniKur();

        // 2) WinForms'un standart ayarlari (yuksek DPI, gorsel stiller). Sablondan gelir.
        ApplicationConfiguration.Initialize();

        // 3) Tek kopya (K17, M25b): oyun zaten aciksa haber ver ve kapan.
        using var tekKopya = new TekKopyaKoruyucu(TekKopyaKilidi);
        if (!tekKopya.IlkKopyaMi)
        {
            MessageBox.Show("AliBird zaten açık.", Baslik, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // 4) Siniflari birbirine bagla. Burada hata olursa oyun acilamaz (M24 #9).
        ServiceProvider servisler;
        try
        {
            servisler = ServisleriKur();
            _hataBildirici = servisler.GetRequiredService<IHataBildirici>();
        }
        catch (Exception hata)
        {
            BaslangicHatasiniBildir(hata);
            return;
        }

        // 5) Oyunu calistir. "using": cikista butun servisler (log, ses, veritabani...) duzgun kapanir.
        using (servisler)
        {
            try
            {
                AnaForm form = servisler.GetRequiredService<AnaForm>();
                OyunYoneticisi yonetici = servisler.GetRequiredService<OyunYoneticisi>();

                PencereOlaylariniBagla(form, yonetici);

                // Pencere kapanana kadar burada kalir.
                Application.Run(form);
            }
            catch (Exception hata)
            {
                BaslangicHatasiniBildir(hata);
            }
        }
    }

    // ================= DI KURULUMU (N6: hepsi Singleton) =================
    private static ServiceProvider ServisleriKur()
    {
        var servisler = new ServiceCollection();

        // ---- Core ----
        servisler.AddSingleton(new OyunAyarlari());                                // D1b
        servisler.AddSingleton<CarpismaDenetcisi>();                               // durum tutmaz, paylasilir (CK8)
        servisler.AddSingleton<IOyunMotoruFabrikasi, OyunMotoruFabrikasi>();       // her oyunda yeni motor (K19)
        servisler.AddSingleton<OyunYoneticisi>();                                  // presenter

        // ---- Infrastructure ----
        servisler.AddSingleton<IUygulamaYollari>(_ => new UygulamaYollari(UygulamaYollari.VarsayilanKokKlasor()));
        servisler.AddSingleton<IGunlukcu, SerilogGunlukcu>();                      // K15
        servisler.AddSingleton<IHataBildirici, HataBildirici>();                   // K14
        servisler.AddSingleton<IPuanDeposu, SqlitePuanDeposu>();                   // K10
        servisler.AddSingleton<ISesUreteci, SesUreteci>();                         // K13
        servisler.AddSingleton<ISesCalar, NAudioSesCalar>();
        servisler.AddSingleton<ISaat, SistemSaati>();
        servisler.AddSingleton<IRastgele, SistemRastgele>();

        // ---- WinForms ----
        servisler.AddSingleton(Tema.Gece());                                       // G1b
        servisler.AddSingleton<OlcekHesaplayici>();
        servisler.AddSingleton<OyunCizici>();
        servisler.AddSingleton<IZamanlayici, FormsZamanlayici>();                  // G8
        servisler.AddSingleton<IKullaniciBildirici, MesajKutusuBildirici>();
        servisler.AddSingleton<AnaForm>();
        // Presenter IOyunGorunumu ister; ona AYNI AnaForm ornegi verilir.
        servisler.AddSingleton<IOyunGorunumu>(sp => sp.GetRequiredService<AnaForm>());

        // ValidateOnBuild: eksik ya da hatali bir kayit varsa oyun acilirken hemen anlasilir.
        return servisler.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
    }

    // ================= PENCERE OLAYLARI =================
    private static void PencereOlaylariniBagla(AnaForm form, OyunYoneticisi yonetici)
    {
        // Pencere ekranda gorununce oyunu baslat (rekoru oku, zamanlayiciyi calistir).
        form.Shown += async (_, _) =>
        {
            try
            {
                await yonetici.BaslatAsync();
            }
            catch (Exception hata)
            {
                _hataBildirici?.Bildir(hata, Gorunurluk.EkranVeGunluk, "Oyun başlatılamadı.");
            }
        };

        // K52 b: pencere kapanirken oyun suruyorsa rekor kaydedilsin.
        // Kayit async oldugu icin ilk kapanma istegini iptal edip kaydi bekleriz (en fazla 2 sn, CK6),
        // sonra pencereyi tekrar kapatiriz.
        bool kapanisaHazir = false;
        form.FormClosing += async (_, e) =>
        {
            if (kapanisaHazir)
            {
                return;
            }

            e.Cancel = true;
            try
            {
                await yonetici.KapatAsync();
            }
            catch (Exception hata)
            {
                _hataBildirici?.Bildir(hata, Gorunurluk.SadeceGunluk, string.Empty);
            }

            kapanisaHazir = true;
            form.Close();
        };
    }

    // ================= GENEL HATA YAKALAYICILAR (R6) =================
    private static void GenelHataYakalayicilariniKur()
    {
        // Arayuz is parcacigindaki yakalanmamis hatalar Application.ThreadException'a gelsin;
        // boylece uygulama kapanmaz, kullanici bilgilendirilir ve oyun devam eder.
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => GenelHatayiBildir(e.Exception);

        // Arka plan is parcaciklarindaki hatalar. .NET bu durumda uygulamayi kapatir;
        // biz en azindan loga yazariz (bkz. CK41).
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            GenelHatayiBildir(e.ExceptionObject as Exception ?? new Exception("Bilinmeyen hata"));

        // Beklenmeyen (await edilmemis) Task hatalari: loga yaz ve "goruldu" isaretle ki surec kapanmasin.
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            GenelHatayiBildir(e.Exception);
            e.SetObserved();
        };
    }

    private static void GenelHatayiBildir(Exception hata)
    {
        try
        {
            if (_hataBildirici is null)
            {
                // DI henuz kurulmadi: dogrudan mesaj kutusu.
                MessageBox.Show("Beklenmeyen bir hata oluştu.", Baslik, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // CK40: ilk genel hata ekranda gosterilir, sonrakiler sadece loga gider (mesaj kutusu yagmuru olmasin).
            Gorunurluk gorunurluk = _genelHataGosterildi ? Gorunurluk.SadeceGunluk : Gorunurluk.EkranVeGunluk;
            _genelHataGosterildi = true;
            _hataBildirici.Bildir(hata, gorunurluk,
                "Beklenmeyen bir hata oluştu. Oyun çalışmaya devam ediyor; sorun sürerse oyunu yeniden başlatın.");
        }
        catch
        {
            // R6: son savunma hattinda da exception disari cikmaz.
        }
    }

    // M24 #9: oyun hic acilamadi. Kullaniciya soyle ve mumkunse loga yaz.
    private static void BaslangicHatasiniBildir(Exception hata)
    {
        try
        {
            MessageBox.Show("AliBird başlatılamadı. Ayrıntılar log dosyasında.", Baslik,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch
        {
            // R6
        }

        try
        {
            // DI kurulamamis olabilir; log icin gecici bir gunlukcu (kompozisyon kokunun yedek yolu).
            using var gunlukcu = new SerilogGunlukcu(new UygulamaYollari(UygulamaYollari.VarsayilanKokKlasor()));
            gunlukcu.Hata(hata, "Uygulama başlatılamadı.");
        }
        catch
        {
            // R6
        }
    }
}
