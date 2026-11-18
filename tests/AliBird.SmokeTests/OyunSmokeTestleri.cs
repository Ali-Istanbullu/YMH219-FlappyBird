using System.Diagnostics;
using AliBird.Core.Abstractions;
using AliBird.Core.Configuration;
using AliBird.Core.Gameplay;
using AliBird.Core.Models;
using AliBird.Core.Presentation;
using AliBird.Core.Services;
using AliBird.Infrastructure.Errors;
using AliBird.Infrastructure.FileSystem;
using AliBird.Infrastructure.Logging;
using AliBird.Infrastructure.Persistence;
using AliBird.Infrastructure.Platform;
using AliBird.WinForms;
using AliBird.WinForms.Forms;
using AliBird.WinForms.Rendering;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace AliBird.SmokeTests;

// Smoke: oyunun parcalari GERCEK halleriyle bir arada calisiyor mu?
[TestClass]
[TestCategory("Smoke")]
public sealed class OyunSmokeTestleri
{
    private string _klasor = null!;

    [TestInitialize]
    public void Hazirla()
    {
        _klasor = Path.Combine(Path.GetTempPath(), "alibird-smoke-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_klasor);
    }

    [TestCleanup]
    public void Temizle()
    {
        SqliteConnection.ClearAllPools();   // havuzdaki baglanti dosyayi kilitli tutmasin
        try { Directory.Delete(_klasor, recursive: true); } catch (IOException) { } catch (UnauthorizedAccessException) { }
    }

    // Program.cs'teki GERCEK kayitlarla container kuruluyor mu (ValidateOnBuild), ana parcalar olusuyor mu?
    [TestMethod]
    public void DI_GercekKayitlarlaKurulurVeAnaParcalarOlusur()
    {
        using ServiceProvider servisler = Program.ServisleriKur(_klasor);

        AnaForm form = servisler.GetRequiredService<AnaForm>();
        Assert.IsNotNull(servisler.GetRequiredService<OyunYoneticisi>());
        Assert.IsNotNull(servisler.GetRequiredService<IOyunCizici>());
        Assert.AreSame(form, servisler.GetRequiredService<IOyunGorunumu>());   // presenter ayni formu kullaniyor
    }

    // Ana akis: baslat -> Space -> kus duser, oyun biter -> rekor diske yazilir -> kapanis.
    // Gercek motor, rekor yoneticisi, SQLite ve Serilog; sadece pencere, zamanlayici, saat ve ses sahte.
    [TestMethod]
    public async Task UctanUca_OynaBitirKapat_RekorDiskeYazilir()
    {
        var ayarlar = new OyunAyarlari { IlkEngelX = -50f };   // ilk engel kusun arkasinda: ilk karede 1 puan
        var yollar = new UygulamaYollari(_klasor);
        var gorunum = new KucukGorunum();
        var zamanlayici = new KucukZamanlayici();
        var saat = new SistemSaati();

        using (var gunlukcu = new SerilogGunlukcu(yollar))
        {
            var hataBildirici = new HataBildirici(gunlukcu, new KucukBildirici());
            var depo = new SqlitePuanDeposu(yollar, new BozukVeritabaniKurtarici(yollar, hataBildirici, gunlukcu, saat), gunlukcu);
            var yonetici = new OyunYoneticisi(
                gorunum,
                new OyunMotoruFabrikasi(ayarlar, new SistemRastgele(), new CarpismaDenetcisi(ayarlar), new EngelUretici(ayarlar)),
                new RekorYoneticisi(depo, hataBildirici, gunlukcu, saat),
                new SesYoneticisi(new SessizSesCalar()),
                new KareSuresiOlcer(new AdimliKronometre()),
                new KareCarpaniHesaplayici(ayarlar),
                zamanlayici,
                gunlukcu,
                saat,
                ayarlar);

            await yonetici.BaslatAsync();
            gorunum.BoslugaBas();
            for (int i = 0; i < 2000 && yonetici.Durum != OyunDurumu.OyunBitti; i++)
            {
                zamanlayici.Tikla();
            }
            Assert.AreEqual(OyunDurumu.OyunBitti, yonetici.Durum);

            await yonetici.KapatAsync();   // bekleyen rekor kaydi biter
        }

        RekorKaydi? rekor = await new SqlitePuanDeposu(yollar,
            new BozukVeritabaniKurtarici(yollar, new SessizHataBildirici(), new SessizGunlukcu(), new SistemSaati()),
            new SessizGunlukcu()).RekoruOkuAsync();
        Assert.IsNotNull(rekor);
        Assert.IsGreaterThanOrEqualTo(1, rekor.Puan);
    }

    // Derlenen exe gercekten aciliyor ve duzgun kapaniyor mu?
    [TestMethod]
    public void Exe_AcilirVeDuzgunKapanir()
    {
        string exe = Path.Combine(AppContext.BaseDirectory, "AliBird.exe");
        if (!File.Exists(exe))
        {
            Assert.Inconclusive($"Exe bulunamadi: {exe}");
        }

        using Process surec = Process.Start(new ProcessStartInfo(exe) { UseShellExecute = false })!;
        try
        {
            // WaitForInputIdle pencere gorunmeden donebilir: ana pencere tutamaci (handle) olusana kadar bekle.
            var sayac = Stopwatch.StartNew();
            while (surec.MainWindowHandle == IntPtr.Zero && !surec.HasExited && sayac.ElapsedMilliseconds < 15000)
            {
                Thread.Sleep(100);
                surec.Refresh();
            }
            Assert.IsFalse(surec.HasExited, "Oyun acilir acilmaz kapandi.");
            Assert.AreNotEqual(IntPtr.Zero, surec.MainWindowHandle, "Ana pencere 15 sn icinde acilmadi.");
            Thread.Sleep(500);   // pencere ilk cizimini bitirsin

            Assert.IsTrue(surec.CloseMainWindow(), $"Kapat mesaji gonderilemedi. Pencere: '{surec.MainWindowTitle}'");
            Assert.IsTrue(surec.WaitForExit(10000), $"Oyun 10 sn icinde kapanmadi. Pencere: '{surec.MainWindowTitle}'");
            Assert.AreEqual(0, surec.ExitCode);
        }
        finally
        {
            if (!surec.HasExited)
            {
                surec.Kill();
            }
        }
    }

    // ---- Smoke testine ozel kucuk sahte siniflar ----

    private sealed class KucukGorunum : IOyunGorunumu
    {
        public event EventHandler? BoslukBasildi;
        public event EventHandler? PBasildi;
        public event EventHandler? MBasildi;
        public event EventHandler? YenidenBaslatTiklandi;
        public event EventHandler? OdakKaybedildi;

        public void Goster(OyunGoruntusu goruntu) { }

        public void BoslugaBas()
        {
            BoslukBasildi?.Invoke(this, EventArgs.Empty);
            _ = (PBasildi, MBasildi, YenidenBaslatTiklandi, OdakKaybedildi);   // kullanilmayan olay uyarisi olmasin
        }
    }

    private sealed class KucukZamanlayici : IZamanlayici
    {
        public event EventHandler? Tik;
        public int AralikMs { get; set; }
        public void Baslat() { }
        public void Durdur() { }
        public void Tikla() => Tik?.Invoke(this, EventArgs.Empty);
    }

    // Her okumada 1/60 sn ilerler: oyun 60 FPS'te calisiyormus gibi.
    private sealed class AdimliKronometre : IKronometre
    {
        private TimeSpan _gecen;
        public TimeSpan Gecen => _gecen += TimeSpan.FromMilliseconds(1000.0 / 60);
    }

    private sealed class SessizSesCalar : ISesCalar
    {
        public bool KullanilabilirMi => false;
        public void Cal(SesTuru tur) { }
    }

    private sealed class KucukBildirici : IKullaniciBildirici
    {
        public void MesajGoster(string mesaj) { }
    }

    private sealed class SessizHataBildirici : IHataBildirici
    {
        public void Bildir(Exception hata, Gorunurluk gorunurluk, string kullaniciMesaji) { }
    }

    private sealed class SessizGunlukcu : IGunlukcu
    {
        public void Bilgi(string mesaj) { }
        public void Uyari(string mesaj) { }
        public void Hata(Exception hata, string mesaj) { }
    }
}
