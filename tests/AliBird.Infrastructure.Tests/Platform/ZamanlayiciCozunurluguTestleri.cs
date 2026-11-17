using AliBird.Infrastructure.Platform;
using AliBird.Infrastructure.Tests.Fakes;

namespace AliBird.Infrastructure.Tests.Platform;

// Zamanlayici cozunurlugu: gercek winmm.dll ile istenir ve geri verilir (K67).
[TestClass]
public sealed class ZamanlayiciCozunurluguTestleri
{
    [TestMethod]
    public void Artir_BasariliOlunca_BilgiLogaYazilir()
    {
        var gunlukcu = new SahteGunlukcu();
        using var cozunurluk = new WindowsZamanlayiciCozunurlugu(gunlukcu);

        cozunurluk.Artir();

        Assert.HasCount(1, gunlukcu.Kayitlar);
        Assert.StartsWith("INF Zamanlayıcı çözünürlüğü 1 ms", gunlukcu.Kayitlar[0]);
    }

    [TestMethod]
    public void IkiKezArtir_VeIkiKezDispose_HataVermez()
    {
        var gunlukcu = new SahteGunlukcu();
        var cozunurluk = new WindowsZamanlayiciCozunurlugu(gunlukcu);

        cozunurluk.Artir();
        cozunurluk.Artir();   // ikinci istek yok sayilir
        cozunurluk.Dispose();
        cozunurluk.Dispose();

        Assert.HasCount(1, gunlukcu.Kayitlar);
    }
}
