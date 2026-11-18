using AliBird.Core.Configuration;
using AliBird.Core.Gameplay;

namespace AliBird.Core.Tests.Gameplay;

// Tek bir engelin konum, alan ve gecis kurallarini dogrular.
[TestClass]
public sealed class EngelTestleri
{
    private readonly OyunAyarlari _ayarlar = new();

    [TestMethod]
    public void Alanlar_BoslukEtrafindaDogruHesaplanir()
    {
        // Bosluk ust kenari y = 100.
        var engel = new Engel(_ayarlar, x: 300f, boslukUstY: 100f);

        // Ust boru: y 0'dan 100'e.
        Assert.AreEqual(0f, engel.UstAlan.Y);
        Assert.AreEqual(100f, engel.UstAlan.Height);

        // Alt boru: 100 + 150 = 250'den zemine (440) kadar -> yukseklik 190.
        Assert.AreEqual(250f, engel.AltAlan.Y);
        Assert.AreEqual(190f, engel.AltAlan.Height);

        Assert.AreEqual(60f, engel.UstAlan.Width);
    }

    [TestMethod]
    public void Kaydir_EngeliSolaTasir()
    {
        var engel = new Engel(_ayarlar, 300f, 100f);

        engel.Kaydir(6f);

        Assert.AreEqual(294f, engel.X);
    }

    [TestMethod]
    public void EkrandanCiktiMi_SagKenarSifirinSolundaysaTrue()
    {
        // x = -61: sag kenar -61 + 60 = -1 < 0 -> cikti.
        Assert.IsTrue(new Engel(_ayarlar, -61f, 100f).EkrandanCiktiMi);

        // x = -59: sag kenar 1 -> hala ekranda.
        Assert.IsFalse(new Engel(_ayarlar, -59f, 100f).EkrandanCiktiMi);
    }

    [TestMethod]
    public void GecisiKontrolEt_SadeceIlkGeciste_TrueDoner()
    {
        // Engel 0-60 arasinda; kusun sol kenari 61 -> sag kenari gecti.
        var engel = new Engel(_ayarlar, 0f, 100f);

        Assert.IsTrue(engel.GecisiKontrolEt(61f));    // ilk gecis: puan
        Assert.IsFalse(engel.GecisiKontrolEt(62f));   // ayni engel: tekrar puan YOK
    }

    [TestMethod]
    public void GecisiKontrolEt_SagKenaraEsitken_HenuzGecmemistir()
    {
        var engel = new Engel(_ayarlar, 0f, 100f);

        // Sol kenar tam 60 = engelin sag kenari: "tamamen gecti" sayilmaz.
        Assert.IsFalse(engel.GecisiKontrolEt(60f));
    }

    [TestMethod]
    public void YenidenKonumla_GecildiBilgisiniSifirlar()
    {
        var engel = new Engel(_ayarlar, 0f, 100f);
        engel.GecisiKontrolEt(61f);

        engel.YenidenKonumla(900f, 150f);

        Assert.AreEqual(900f, engel.X);
        Assert.AreEqual(150f, engel.BoslukUstY);
        Assert.IsFalse(engel.GecildiMi);
    }
}
