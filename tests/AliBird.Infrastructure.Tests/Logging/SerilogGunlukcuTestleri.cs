using AliBird.Infrastructure.FileSystem;
using AliBird.Infrastructure.Logging;
using AliBird.Infrastructure.Tests.Fakes;

namespace AliBird.Infrastructure.Tests.Logging;

// Log dosyasinin bicimini, boyut sinirini ve "asla cokme" kuralini dogrular.
[TestClass]
public sealed class SerilogGunlukcuTestleri
{
    [TestMethod]
    public void HataKaydi_TarihSeviyeMesajVeAyrintiIcerir()
    {
        using var kok = new GeciciKlasor();
        var yollar = new UygulamaYollari(kok.Yol);

        using (var gunlukcu = new SerilogGunlukcu(yollar))
        {
            gunlukcu.Hata(new InvalidOperationException("ic ayrinti"), "Rekor kaydedilemedi {test}");
        }   // Dispose: satirlar diske yazilir ve dosya kapanir

        string icerik = File.ReadAllText(yollar.GunlukDosyasi());

        StringAssert.Contains(icerik, "[ERR]");
        StringAssert.Contains(icerik, "Rekor kaydedilemedi {test}");   // { } aynen yazildi
        StringAssert.Contains(icerik, "ic ayrinti");                   // hata ayrintisi
        StringAssert.Matches(icerik, new System.Text.RegularExpressions.Regex(@"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}"));   // tarih-saat
    }

    [TestMethod]
    public void CokLogYazilinca_EnFazlaBesDosyaKalirVeToplamYaklasikBesMbOlur()
    {
        using var kok = new GeciciKlasor();
        var yollar = new UygulamaYollari(kok.Yol);
        string satir = new('x', 1000);   // ~1 KB'lik satir

        using (var gunlukcu = new SerilogGunlukcu(yollar))
        {
            // ~8 MB log: sinir olmasaydi 8 dosya olurdu.
            for (int i = 0; i < 8000; i++)
            {
                gunlukcu.Bilgi(satir);
            }
        }

        string[] dosyalar = Directory.GetFiles(yollar.GunlukKlasoru());
        long toplam = dosyalar.Sum(d => new FileInfo(d).Length);

        Assert.IsLessThanOrEqualTo(5, dosyalar.Length);                      // en fazla 5 dosya
        Assert.IsLessThanOrEqualTo(5L * 1024 * 1024 + 64 * 1024, toplam);   // ~5 MB (+ son satir payi)
    }

    [TestMethod]
    public void LogKlasoruOlusturulamazsa_CokmezVeSessizDevamEder()
    {
        using var kok = new GeciciKlasor();
        // "logs" adinda bir DOSYA olustur: artik ayni adla klasor olusturulamaz.
        File.WriteAllText(Path.Combine(kok.Yol, "logs"), "engel");
        var yollar = new UygulamaYollari(kok.Yol);

        // Hicbiri exception firlatmamali.
        var gunlukcu = new SerilogGunlukcu(yollar);
        gunlukcu.Bilgi("yazilamayacak");
        gunlukcu.Hata(new Exception("x"), "yazilamayacak");
        gunlukcu.Dispose();
    }
}
