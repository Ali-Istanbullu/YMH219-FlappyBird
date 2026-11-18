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
//  Tek sorumlulugu: uygulamayi ayaga kaldirmak.
//   1) Genel hata yakalayiciyi devreye almak (isi GenelHataYakalayici'da)
//   2) Tek kopya kontrolu (isi TekKopyaKoruyucu'da)
//   3) Siniflari birbirine baglamak: DI container SADECE burada kurulur
//   4) Pencereyi acmak, kapanista rekoru kaydettirmek, acilis/kapanisi loglamak
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
        // 1) Genel hata yakalayici: HER SEYDEN ONCE. DI'dan once gerektigi icin burada olusturulur.
        var genelHataYakalayici = new GenelHataYakalayici();
        genelHataYakalayici.Kur();

        // 2) WinForms'un standart ayarlari (yuksek DPI, gorsel stiller).
        ApplicationConfiguration.Initialize();

        // 3) Tek kopya.
        using var tekKopya = new TekKopyaKoruyucu(TekKopyaKilidi);
        if (!tekKopya.IlkKopyaMi)
        {
            MessageBox.Show("AliBird zaten açık.", Baslik, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // 4) Siniflari bagla. Hata olursa oyun acilamaz.
        ServiceProvider servisler;
        try
        {
            servisler = ServisleriKur(UygulamaYollari.VarsayilanKokKlasor());
            genelHataYakalayici.BildiriciyiAta(servisler.GetRequiredService<IHataBildirici>());
        }
        catch (Exception hata) when (!DerlemeModu.HataAyiklamaMi)   // Debug'da debugger dursun
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
                gunlukcu.Bilgi($"AliBird {Application.ProductVersion} açıldı.");

                AnaForm form = servisler.GetRequiredService<AnaForm>();
                OyunYoneticisi yonetici = servisler.GetRequiredService<OyunYoneticisi>();
                PencereOlaylariniBagla(form, yonetici);

                // Pencere kapanana kadar burada kalir.
                Application.Run(form);
            }
            catch (Exception hata) when (!DerlemeModu.HataAyiklamaMi)
            {
                BaslangicHatasiniBildir(hata);
            }
            finally
            {
                gunlukcu.Bilgi("AliBird kapandı.");   // logger servislerle birlikte en son kapanir
            }
        }
    }

    // ================= DI KURULUMU (hepsi Singleton) =================
    // internal: smoke testleri ayni kayitlarla container'i kurabilsin (InternalsVisibleTo).
    // kokKlasor: veri ve log klasoru; testte gecici bir klasor verilir.
    internal static ServiceProvider ServisleriKur(string kokKlasor)
    {
        var servisler = new ServiceCollection();

        // ---- Core: oyun ----
        servisler.AddSingleton(new OyunAyarlari());
        servisler.AddSingleton<ICarpismaDenetcisi, CarpismaDenetcisi>();          // durum tutmaz
        servisler.AddSingleton<IEngelUretici, EngelUretici>();
        servisler.AddSingleton<IOyunMotoruFabrikasi, OyunMotoruFabrikasi>();       // her oyunda yeni motor

        // ---- Core: servisler ----
        servisler.AddSingleton<IRekorYoneticisi, RekorYoneticisi>();
        servisler.AddSingleton<ISesYoneticisi, SesYoneticisi>();
        servisler.AddSingleton<IKareSuresiOlcer, KareSuresiOlcer>();
        servisler.AddSingleton<IKareCarpaniHesaplayici, KareCarpaniHesaplayici>(); // delta time
        servisler.AddSingleton<OyunYoneticisi>();                                  // presenter

        // ---- Infrastructure ----
        servisler.AddSingleton<IUygulamaYollari>(_ => new UygulamaYollari(kokKlasor));
        servisler.AddSingleton<IGunlukcu, SerilogGunlukcu>();
        servisler.AddSingleton<IHataBildirici, HataBildirici>();
        servisler.AddSingleton<IBozukVeritabaniKurtarici, BozukVeritabaniKurtarici>();
        servisler.AddSingleton<IPuanDeposu, SqlitePuanDeposu>();
        servisler.AddSingleton<ISesUreteci, SesUreteci>();
        servisler.AddSingleton<ISesSaglayiciUretici, DiziSesSaglayiciUretici>();
        servisler.AddSingleton<ISesCalar, NAudioSesCalar>();
        servisler.AddSingleton<ISaat, SistemSaati>();
        servisler.AddSingleton<IKronometre, SistemKronometresi>();                 // monoton sayac
        servisler.AddSingleton<IRastgele, SistemRastgele>();

        // ---- WinForms ----
        servisler.AddSingleton(Tema.Gece());
        servisler.AddSingleton<IOlcekHesaplayici, OlcekHesaplayici>();
        servisler.AddSingleton<CizimKaynaklari>();                                 // GDI+ kaynak paketi
        servisler.AddSingleton<IYaziCizici, YaziCizici>();
        servisler.AddSingleton<IKusCizici, KusCizici>();
        // Cizim katmanlari: KAYIT SIRASI = CIZIM SIRASI (arkadan one). Yeni katman = yeni satir.
        servisler.AddSingleton<IKatmanCizici, SahneCizici>();
        servisler.AddSingleton<IKatmanCizici, BilgiCizici>();
        servisler.AddSingleton<IKatmanCizici, DurumKatmaniCizici>();
        servisler.AddSingleton<IOyunCizici, OyunCizici>();
        servisler.AddSingleton<IZamanlayici, FormsZamanlayici>();
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
            // Beklenen hatalari (rekor dosyasi) RekorYoneticisi yakalar; buraya sadece bug gelir.
            await yonetici.BaslatAsync();
        };

        // kapanirken ilk istegi iptal et, rekor kaydini bekle (en fazla 2 sn), sonra kapat.
        bool kapanisaHazir = false;
        form.FormClosing += async (_, e) =>
        {
            if (kapanisaHazir)
            {
                return;
            }

            e.Cancel = true;
            await yonetici.KapatAsync();

            kapanisaHazir = true;
            form.Close();
        };
    }

    // oyun hic acilamadi. Kullaniciya soyle ve mumkunse loga yaz.
    private static void BaslangicHatasiniBildir(Exception hata)
    {
        try
        {
            MessageBox.Show("AliBird başlatılamadı. Ayrıntılar log dosyasında.", Baslik,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch
        {
            // bilerek bos: zaten kapanis yolundayiz, gosterilecek baska yer yok.
        }

        try
        {
            // DI kurulamamis olabilir; log icin gecici bir gunlukcu (kompozisyon kokunun yedek yolu).
            using var gunlukcu = new SerilogGunlukcu(new UygulamaYollari(UygulamaYollari.VarsayilanKokKlasor()));
            gunlukcu.Hata(hata, "Uygulama başlatılamadı.");
        }
        catch
        {
            // bilerek bos: zaten kapanis yolundayiz, gosterilecek baska yer yok.
        }
    }
}
