using AliBird.Core.Configuration;
using AliBird.Core.Gameplay;
using AliBird.Core.Services;
using AliBird.Core.Tests.Fakes;

namespace AliBird.Core.Tests.Gameplay;

// hareket kare sayisina degil gecen sureye bagli (carpan = kac kare, 1 kare = 1/60 sn).
[TestClass]
public sealed class DeltaZamanTestleri
{
    [TestMethod]
    public void Kus_Carpan2_IkiKarelikYerCekimiVeHareket()
    {
        var kus = new Kus(new OyunAyarlari());

        kus.Guncelle(2f);

        Assert.AreEqual(1.2f, kus.DikeyHiz, 0.0001f);    // 0 + 0.6 * 2
        Assert.AreEqual(210.4f, kus.Y, 0.0001f);         // 208 + 1.2 * 2
    }

    [TestMethod]
    public void HizDenetcisi_Carpan2_IkiKarelikArtis()
    {
        var denetci = new HizDenetcisi(new OyunAyarlari());

        denetci.KareIlerlet(2f);

        Assert.AreEqual(6.002f, denetci.Hiz, 0.00001f);
    }

    [TestMethod]
    public void Motor_CarpaniKusaVeEngelHizinaAktarir()
    {
        var cagrilar = new List<string>();
        var kus = new SahteKus(cagrilar);
        var engelAlani = new SahteEngelAlani(cagrilar);
        var motor = new OyunMotoru(kus, engelAlani, new SahteCarpismaDenetcisi(cagrilar),
            new SahteHizDenetcisi(cagrilar), new SahtePuanTablosu());

        motor.Ilerlet(2f);

        Assert.AreEqual(2f, kus.SonCarpan);
        Assert.AreEqual(14f, engelAlani.SonHiz);   // hiz 7 * 2 kare
    }

    [TestMethod]
    public void Motor_SifirCarpan_Reddedilir()
    {
        var cagrilar = new List<string>();
        var motor = new OyunMotoru(new SahteKus(cagrilar), new SahteEngelAlani(cagrilar),
            new SahteCarpismaDenetcisi(cagrilar), new SahteHizDenetcisi(cagrilar), new SahtePuanTablosu());

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => motor.Ilerlet(0f));
    }

    [TestMethod]
    public void KareSuresiOlcer_GecenSureyiDondurur()
    {
        var kronometre = new SahteKronometre();
        var olcer = new KareSuresiOlcer(kronometre);

        double ilk = olcer.KareGeldi();
        kronometre.Ilerlet(28);
        double ikinci = olcer.KareGeldi();

        Assert.AreEqual(0.0, ilk);
        Assert.AreEqual(28.0, ikinci, 0.0001);
    }
}
