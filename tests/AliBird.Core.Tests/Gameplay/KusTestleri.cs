using AliBird.Core.Configuration;
using AliBird.Core.Gameplay;

namespace AliBird.Core.Tests.Gameplay;

// Kus sinifinin fizik kurallarini dogrular (K6, E1, E4, E6).
[TestClass]
public sealed class KusTestleri
{
    // Varsayilan (onayli) ayarlar.
    private readonly OyunAyarlari _ayarlar = new();

    [TestMethod]
    public void YeniKus_BaslangicKonumundaVeHareketsizdir()
    {
        var kus = new Kus(_ayarlar);

        Assert.AreEqual(100f, kus.X);        // E1a
        Assert.AreEqual(208f, kus.Y);        // E1a
        Assert.AreEqual(0f, kus.DikeyHiz);   // E2a
    }

    [TestMethod]
    public void Zipla_DikeyHiziZiplamaHizinaAyarlar()
    {
        var kus = new Kus(_ayarlar);
        kus.Guncelle();   // once biraz dussun (hiz +0.6 olsun)

        kus.Zipla();

        // Hiz -12'ye AYARLANMALI, -11.4'e degil (K6: eklenmez, ayarlanir).
        Assert.AreEqual(-12f, kus.DikeyHiz);
    }

    [TestMethod]
    public void Guncelle_YerCekiminiUygularVeKonumuDegistirir()
    {
        var kus = new Kus(_ayarlar);

        kus.Guncelle();

        Assert.AreEqual(0.6f, kus.DikeyHiz, 0.0001f);
        Assert.AreEqual(208.6f, kus.Y, 0.0001f);
    }

    [TestMethod]
    public void Guncelle_DusmeHiziMaksimumuGecemez()
    {
        var kus = new Kus(_ayarlar);

        // 100 kare: hiz sinirsiz artsaydi 60 olurdu.
        for (int i = 0; i < 100; i++)
        {
            kus.Guncelle();
        }

        Assert.AreEqual(12f, kus.DikeyHiz);
    }

    [TestMethod]
    public void Guncelle_KusTavaniGecemezVeHiziKorunur()
    {
        // Kusu tavana yakin baslat: y = 5
        var ayarlar = new OyunAyarlari { KusBaslangicY = 5f };
        var kus = new Kus(ayarlar);
        kus.Zipla();   // hiz -12

        kus.Guncelle();   // hiz -11.4, y = 5 - 11.4 = -6.4 -> 0'a sabitlenir

        Assert.AreEqual(0f, kus.Y);                       // K6: tavanda durur
        Assert.AreEqual(-11.4f, kus.DikeyHiz, 0.0001f);   // E6b: hiz sifirlanmaz
    }

    [TestMethod]
    public void CarpismaKutusu_HerKenardanUcPikselKucuktur()
    {
        var kus = new Kus(_ayarlar);

        var kutu = kus.CarpismaKutusu;

        // E4b: 30x24 -> 24x18, sol ust kose 3 px iceride.
        Assert.AreEqual(103f, kutu.X);
        Assert.AreEqual(211f, kutu.Y);
        Assert.AreEqual(24f, kutu.Width);
        Assert.AreEqual(18f, kutu.Height);
    }

    [TestMethod]
    public void Olusturucu_AyarlarNullIse_HataFirlatir()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new Kus(null!));
    }
}
