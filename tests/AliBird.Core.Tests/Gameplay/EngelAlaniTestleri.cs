using AliBird.Core.Configuration;
using AliBird.Core.Gameplay;
using AliBird.Core.Tests.Fakes;

namespace AliBird.Core.Tests.Gameplay;

// Engellerin dizilisi, kaymasi ve yeniden konumlanmasi (G2, K7, E5).
[TestClass]
public sealed class EngelAlaniTestleri
{
    private readonly OyunAyarlari _ayarlar = new();

    [TestMethod]
    public void YeniAlan_UcEngeliIlkEngelXdenBaslayarakDizer()
    {
        var alan = new EngelAlani(_ayarlar, new SahteRastgele(100), new EngelUretici(_ayarlar));

        Assert.HasCount(3, alan.Engeller);                // 640 / 320 + 1
        Assert.AreEqual(960f, alan.Engeller[0].X);        // E5b
        Assert.AreEqual(1280f, alan.Engeller[1].X);
        Assert.AreEqual(1600f, alan.Engeller[2].X);
    }

    [TestMethod]
    public void BoslukYuksekligi_KenarPaylarinaGoreSinirlanir()
    {
        var rastgele = new SahteRastgele(100);

        _ = new EngelAlani(_ayarlar, rastgele, new EngelUretici(_ayarlar));

        // K7: en az 60, en cok 440 - 60 - 150 = 230 (ust sinir haric oldugu icin 231 istenir).
        Assert.AreEqual(60, rastgele.SonEnAz);
        Assert.AreEqual(231, rastgele.SonEnCok);
    }

    [TestMethod]
    public void Guncelle_EkrandanCikanEngeliEnSagdakininAraligiKadarSaginaTasir()
    {
        var alan = new EngelAlani(_ayarlar, new SahteRastgele(100), new EngelUretici(_ayarlar));

        // Ilk engel 960'tan basliyor; sag kenari (960 + 60) sifirin soluna gecene kadar kaydir.
        // 1025 px kaydirinca: engel0 = -65, engel1 = 255, engel2 = 575.
        alan.Guncelle(1025f);

        // engel0 en sagdakinin (575) 320 sagina tasinmali: 895.
        Assert.AreEqual(895f, alan.Engeller[0].X);
        Assert.AreEqual(255f, alan.Engeller[1].X);
        Assert.AreEqual(575f, alan.Engeller[2].X);
    }

    [TestMethod]
    public void YeniGecilenleriSay_GecilenEngeliBirKezSayar()
    {
        var alan = new EngelAlani(_ayarlar, new SahteRastgele(100), new EngelUretici(_ayarlar));
        alan.Guncelle(900f);   // engel0: 960 - 900 = 60, sag kenari 120

        Assert.AreEqual(0, alan.YeniGecilenleriSay(100f));   // kus sol kenari 100 < 120: gecmedi
        alan.Guncelle(30f);                                  // engel0 sag kenari 90
        Assert.AreEqual(1, alan.YeniGecilenleriSay(100f));   // gecti: 1 puan
        Assert.AreEqual(0, alan.YeniGecilenleriSay(100f));   // ayni engel tekrar sayilmaz
    }
}
