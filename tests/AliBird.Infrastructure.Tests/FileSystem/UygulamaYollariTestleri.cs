using AliBird.Infrastructure.FileSystem;
using AliBird.Infrastructure.Tests.Fakes;

namespace AliBird.Infrastructure.Tests.FileSystem;

// Klasor ve dosya yollarini ve "silinirse yeniden olustur" kuralini dogrular.
[TestClass]
public sealed class UygulamaYollariTestleri
{
    [TestMethod]
    public void Yollar_DataVeLogsAltindaDogruDosyaAdlariniVerir()
    {
        using var kok = new GeciciKlasor();
        var yollar = new UygulamaYollari(kok.Yol);

        Assert.AreEqual(Path.Combine(kok.Yol, "data", "alibird.db"), yollar.VeritabaniDosyasi());
        Assert.AreEqual(Path.Combine(kok.Yol, "logs", "alibird.log"), yollar.GunlukDosyasi());
    }

    [TestMethod]
    public void KlasorYoksa_IsteninceOlusturulur()
    {
        using var kok = new GeciciKlasor();
        var yollar = new UygulamaYollari(kok.Yol);

        string veri = yollar.VeriKlasoru();

        Assert.IsTrue(Directory.Exists(veri));
    }

    [TestMethod]
    public void KullaniciKlasoruSilerse_BirSonrakiIstekteGeriGelir()
    {
        using var kok = new GeciciKlasor();
        var yollar = new UygulamaYollari(kok.Yol);
        string gunluk = yollar.GunlukKlasoru();

        Directory.Delete(gunluk, recursive: true);   // kullanici sildi
        yollar.GunlukKlasoru();

        Assert.IsTrue(Directory.Exists(gunluk));
    }

    [TestMethod]
    public void VarsayilanKok_LocalAppDataAltindakiAliBirdKlasorudur()
    {
        string beklenen = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AliBird");

        Assert.AreEqual(beklenen, UygulamaYollari.VarsayilanKokKlasor());
    }
}
