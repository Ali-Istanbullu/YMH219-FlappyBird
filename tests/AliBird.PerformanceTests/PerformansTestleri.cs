using System.Diagnostics;
using System.Drawing;
using AliBird.Core.Abstractions;
using AliBird.Core.Gameplay;
using AliBird.Core.Models;
using AliBird.WinForms;
using AliBird.WinForms.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace AliBird.PerformanceTests;

// Performans: oyun 60 FPS hedefler, yani bir kareye 16 ms butce var (oyun mantigi + cizim).
// Siniflar Program.cs'teki GERCEK kayitlarla kurulur. Esikler bilerek comert: yavas bir CI sunucusunda
// da gecmeli; amac kotu bir degisikligi (her karede gereksiz nesne, O(n^2) dongu) yakalamak.
// Olculen sureler test ciktisina yazilir.
[TestClass]
[TestCategory("Performans")]
public sealed class PerformansTestleri
{
    private const int KareSayisi = 10_000;   // 60 FPS'te yaklasik 2,8 dakikalik oyun
    private const double KareButcesiMs = 1000.0 / 60;

    private string _klasor = null!;
    private ServiceProvider _servisler = null!;

    public TestContext TestContext { get; set; } = null!;

    [TestInitialize]
    public void Hazirla()
    {
        _klasor = Path.Combine(Path.GetTempPath(), "alibird-perf-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_klasor);
        _servisler = Program.ServisleriKur(_klasor);
    }

    [TestCleanup]
    public void Temizle()
    {
        _servisler.Dispose();
        try { Directory.Delete(_klasor, recursive: true); } catch (IOException) { } catch (UnauthorizedAccessException) { }
    }

    // 10.000 kare oyun mantigi (fizik, engeller, carpisma, puan). Kus ara ara ziplar; carparsa yeni oyun baslar.
    private static int KareleriOynat(IOyunMotoruFabrikasi fabrika, int kareSayisi)
    {
        OyunMotoru motor = fabrika.Olustur(null);
        int oyunSayisi = 1;
        for (int i = 0; i < kareSayisi; i++)
        {
            if (i % 20 == 0)
            {
                motor.Zipla();
            }
            if (motor.Ilerlet(1f).Carpti)
            {
                motor = fabrika.Olustur(null);
                oyunSayisi++;
            }
        }
        return oyunSayisi;
    }

    [TestMethod]
    public void OyunMantigi_10binKare_OrtalamaKare01msAltinda()
    {
        var fabrika = _servisler.GetRequiredService<IOyunMotoruFabrikasi>();
        KareleriOynat(fabrika, 1000);   // isinma (JIT)

        var sayac = Stopwatch.StartNew();
        int oyunSayisi = KareleriOynat(fabrika, KareSayisi);
        sayac.Stop();

        double ortalamaMs = sayac.Elapsed.TotalMilliseconds / KareSayisi;
        TestContext.WriteLine($"{KareSayisi} kare ({oyunSayisi} oyun): {sayac.ElapsedMilliseconds} ms, kare basina {ortalamaMs:F4} ms");
        Assert.IsLessThanOrEqualTo(0.1, ortalamaMs);   // butcenin %0,6'si
    }

    [TestMethod]
    public void Cizim_1000Kare_OrtalamaKareButcesininYarisindanAz()
    {
        var cizici = _servisler.GetRequiredService<IOyunCizici>();
        OyunMotoru motor = _servisler.GetRequiredService<IOyunMotoruFabrikasi>().Olustur(new RekorKaydi(12, DateTime.Now));
        for (int i = 0; i < 120; i++)
        {
            if (i % 20 == 0) motor.Zipla();
            motor.Ilerlet(1f);   // engeller ekrana girsin
        }
        var goruntu = new OyunGoruntusu(OyunDurumu.Calisiyor, motor.KusAlani, motor.EngelGoruntuleri,
            motor.Puan, new RekorKaydi(12, DateTime.Now), motor.YeniRekorMu, SesDurumu.Acik);

        var pencere = new Size(960, 720);   // 640x480 alanin 1,5 kati: olcekleme de olculsun
        using var tuval = new Bitmap(pencere.Width, pencere.Height);
        using Graphics g = Graphics.FromImage(tuval);
        for (int i = 0; i < 50; i++) cizici.Ciz(g, goruntu, pencere);   // isinma

        const int cizimSayisi = 1000;
        var sayac = Stopwatch.StartNew();
        for (int i = 0; i < cizimSayisi; i++)
        {
            cizici.Ciz(g, goruntu, pencere);
        }
        sayac.Stop();

        double ortalamaMs = sayac.Elapsed.TotalMilliseconds / cizimSayisi;
        TestContext.WriteLine($"{cizimSayisi} cizim: {sayac.ElapsedMilliseconds} ms, kare basina {ortalamaMs:F3} ms");
        Assert.IsLessThanOrEqualTo(KareButcesiMs / 2, ortalamaMs);
    }

    [TestMethod]
    public void TekKare_MantikVeCizimBirlikte_KareButcesininAltinda()
    {
        // En kotu tek kare: oyun mantigi + cizim, 100 karenin en yavasi bile 16 ms'yi gecmemeli.
        var cizici = _servisler.GetRequiredService<IOyunCizici>();
        OyunMotoru motor = _servisler.GetRequiredService<IOyunMotoruFabrikasi>().Olustur(null);
        var pencere = new Size(640, 480);
        using var tuval = new Bitmap(pencere.Width, pencere.Height);
        using Graphics g = Graphics.FromImage(tuval);

        double enYavasMs = 0;
        for (int i = 0; i < 150; i++)
        {
            var sayac = Stopwatch.StartNew();
            if (i % 20 == 0) motor.Zipla();
            motor.Ilerlet(1f);
            cizici.Ciz(g, new OyunGoruntusu(OyunDurumu.Calisiyor, motor.KusAlani, motor.EngelGoruntuleri,
                motor.Puan, null, motor.YeniRekorMu, SesDurumu.Acik), pencere);
            sayac.Stop();
            if (i >= 50)   // ilk 50 kare isinma
            {
                enYavasMs = Math.Max(enYavasMs, sayac.Elapsed.TotalMilliseconds);
            }
        }

        TestContext.WriteLine($"En yavas kare: {enYavasMs:F3} ms (butce {KareButcesiMs:F1} ms)");
        Assert.IsLessThanOrEqualTo(KareButcesiMs, enYavasMs);
    }
}
