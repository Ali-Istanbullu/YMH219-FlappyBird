using AliBird.Core.Abstractions;
using AliBird.Core.Configuration;
using AliBird.Core.Gameplay;
using AliBird.Core.Models;
using AliBird.Core.Presentation;
using AliBird.Core.Services;
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
//  Tek sorumlulugu (K61): uygulamayi ayaga kaldirmak.
//   1) Genel hata yakalayiciyi devreye almak (isi GenelHataYakalayici'da)
//   2) Tek kopya kontrolu (isi TekKopyaKoruyucu'da)
//   3) Siniflari birbirine baglamak: DI container SADECE burada kurulur (M6a, R2)
//   4) Pencereyi acmak, kapanista rekoru kaydettirmek, acilis/kapanisi loglamak (K52, K63)
//  Diger hicbir sinif container'i bilmez; hepsi ihtiyaclarini constructor'dan alir.
// ================================================================================
internal static class Program
{
    // Tek kopya kilidinin adi. "Local\" = sadece bu kullanicinin oturumu.
    private const string TekKopyaKilidi = @"Local\AliBird_TekKopya";
    private const string Baslik = "AliBird";

    [STAThread]
    private static void Main()
    {
        // 1) Genel hata yakalayici: HER SEYDEN ONCE (R6). DI'dan once gerektigi icin burada olusturulur.
        var genelHataYakalayici = new GenelHataYakalayici();
        genelHataYakalayici.Kur();

        // 2) WinForms'un standart ayarlari (yuksek DPI, gorsel stiller).
        ApplicationConfiguration.Initialize();

        // 3) Tek kopya (K17, M25b).
        using var tekKopya = new TekKopyaKoruyucu(TekKopyaKilidi);
        if (!tekKopya.IlkKopyaMi)
        {
            MessageBox.Show("AliBird zaten açık.", Baslik, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // 4) Siniflari bagla. Hata olursa oyun acilamaz (M24 #9).
        ServiceProvider servisler;
        try
        {
            servisler = ServisleriKur();
            genelHataYakalayici.BildiriciyiAta(servisler.GetRequiredService<IHataBildirici>());
        }
        catch (Exception hata)
        {
            BaslangicHatasiniBildir(hata);
            return;
        }

        // 5) Oyunu calistir. "using": cikista butun servisler (log, ses, veritabani...) duzgun kapanir.
        using (servisler)
        {
            IGunlukcu gunlukcu = servisler.GetRequiredService<IGunlukcu>();
            try
            {
                gunlukcu.Bilgi($"AliBird {Application.ProductVersion} açıldı.");   // K63

                AnaForm form = servisler.GetRequiredService<AnaForm>();
                OyunYoneticisi yonetici = servisler.GetRequiredService<OyunYoneticisi>();
                PencereOlaylariniBagla(form, yonetici, servisler.GetRequiredService<IHataBildirici>());

                // Pencere kapanana kadar burada kalir.
                Application.Run(form);
            }
            catch (Exception hata)
            {
                BaslangicHatasiniBildir(hata);
            }
            finally
            {
                gunlukcu.Bilgi("AliBird kapandı.");   // K63 (logger servislerle birlikte en son kapanir)
            }
        }
    }

    // ================= DI KURULUMU (N6: hepsi Singleton) =================
    private static ServiceProvider ServisleriKur()
    {
        var servisler = new ServiceCollection();

        // ---- Core: oyun ----
        servisler.AddSingleton(new OyunAyarlari());                                // D1b
        servisler.AddSingleton<ICarpismaDenetcisi, CarpismaDenetcisi>();          // durum tutmaz (CK8, K65)
        servisler.AddSingleton<IEngelUretici, EngelUretici>();                     // K65
        servisler.AddSingleton<IOyunMotoruFabrikasi, OyunMotoruFabrikasi>();       // her oyunda yeni motor (K19, K60)

        // ---- Core: servisler (K61, Adim 8c) ----
        servisler.AddSingleton<IRekorYoneticisi, RekorYoneticisi>();
        servisler.AddSingleton<ISesYoneticisi, SesYoneticisi>();
        servisler.AddSingleton<IKareHataSayaci, KareHataSayaci>();
        servisler.AddSingleton<IKareSuresiOlcer, KareSuresiOlcer>();               // Adim 9, CK53
        servisler.AddSingleton<IKareCarpaniHesaplayici, KareCarpaniHesaplayici>(); // K68 delta time
        servisler.AddSingleton<OyunYoneticisi>();                                  // presenter

        // ---- Infrastructure ----
        servisler.AddSingleton<IUygulamaYollari>(_ => new UygulamaYollari(UygulamaYollari.VarsayilanKokKlasor()));
        servisler.AddSingleton<IGunlukcu, SerilogGunlukcu>();                      // K15
        servisler.AddSingleton<IHataBildirici, HataBildirici>();                   // K14
        servisler.AddSingleton<IBozukVeritabaniKurtarici, BozukVeritabaniKurtarici>();   // K61
        servisler.AddSingleton<IPuanDeposu, SqlitePuanDeposu>();                   // K10
        servisler.AddSingleton<ISesUreteci, SesUreteci>();                         // K13
        servisler.AddSingleton<ISesSaglayiciUretici, DiziSesSaglayiciUretici>();  // K65
        servisler.AddSingleton<ISesCalar, NAudioSesCalar>();
        servisler.AddSingleton<ISaat, SistemSaati>();
        servisler.AddSingleton<IKronometre, SistemKronometresi>();                 // CK53: monoton sayac
        servisler.AddSingleton<IRastgele, SistemRastgele>();

        // ---- WinForms ----
        servisler.AddSingleton(Tema.Gece());                                       // G1b
        servisler.AddSingleton<IOlcekHesaplayici, OlcekHesaplayici>();            // K65
        servisler.AddSingleton<CizimKaynaklari>();                                 // GDI+ kaynak paketi
        servisler.AddSingleton<IYaziCizici, YaziCizici>();
        // Cizim katmanlari: KAYIT SIRASI = CIZIM SIRASI (arkadan one). Yeni katman = yeni satir (O).
        servisler.AddSingleton<IKatmanCizici, SahneCizici>();
        servisler.AddSingleton<IKatmanCizici, BilgiCizici>();
        servisler.AddSingleton<IKatmanCizici, DurumKatmaniCizici>();
        servisler.AddSingleton<IOyunCizici, OyunCizici>();
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
    private static void PencereOlaylariniBagla(AnaForm form, OyunYoneticisi yonetici, IHataBildirici hataBildirici)
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
                hataBildirici.Bildir(hata, Gorunurluk.EkranVeGunluk, "Oyun başlatılamadı.");
            }
        };

        // K52 b: kapanirken ilk istegi iptal et, rekor kaydini bekle (en fazla 2 sn, CK6), sonra kapat (CK39).
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
                hataBildirici.Bildir(hata, Gorunurluk.SadeceGunluk, string.Empty);
            }

            kapanisaHazir = true;
            form.Close();
        };
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
