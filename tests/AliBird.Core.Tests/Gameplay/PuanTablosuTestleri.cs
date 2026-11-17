using AliBird.Core.Gameplay;
using AliBird.Core.Models;

namespace AliBird.Core.Tests.Gameplay;

// Puan ve rekor kurallarini dogrular (K9, K10, E7).
[TestClass]
public sealed class PuanTablosuTestleri
{
    private static readonly RekorKaydi UcPuanlikRekor = new(3, new DateTime(2026, 11, 17));

    [TestMethod]
    public void IlkOyun_RekorYokken_YeniRekorVurgusuCikmaz()
    {
        var tablo = new PuanTablosu(oncekiRekor: null);
        tablo.PuanEkle(5);

        Assert.IsFalse(tablo.YeniRekorMu);             // E7.1b
        Assert.IsTrue(tablo.KaydedilecekRekorVarMi);   // ama 5 puan ilk rekor olarak kaydedilir
    }

    [TestMethod]
    public void IlkOyun_SifirPuan_Kaydedilmez()
    {
        var tablo = new PuanTablosu(null);

        Assert.IsFalse(tablo.KaydedilecekRekorVarMi);
    }

    [TestMethod]
    public void RekoraEsitPuan_RekorSayilmaz()
    {
        var tablo = new PuanTablosu(UcPuanlikRekor);
        tablo.PuanEkle(3);

        Assert.IsFalse(tablo.YeniRekorMu);             // E7.2a
        Assert.IsFalse(tablo.KaydedilecekRekorVarMi);
    }

    [TestMethod]
    public void RekoruGecenPuan_YeniRekordur()
    {
        var tablo = new PuanTablosu(UcPuanlikRekor);
        tablo.PuanEkle(4);

        Assert.IsTrue(tablo.YeniRekorMu);
        Assert.IsTrue(tablo.KaydedilecekRekorVarMi);
    }

    [TestMethod]
    public void EksiPuan_HataFirlatir()
    {
        var tablo = new PuanTablosu(null);

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => tablo.PuanEkle(-1));
    }
}
