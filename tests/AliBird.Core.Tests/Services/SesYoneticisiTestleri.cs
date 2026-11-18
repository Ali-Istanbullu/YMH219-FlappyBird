using AliBird.Core.Models;
using AliBird.Core.Services;
using AliBird.Core.Tests.Fakes;

namespace AliBird.Core.Tests.Services;

// Ses kapali/acik karari, gosterge durumu ve cokmeme.
[TestClass]
public sealed class SesYoneticisiTestleri
{
    [TestMethod]
    public void Acikken_SesCalinir()
    {
        var calar = new SahteSesCalar();
        var yonetici = new SesYoneticisi(calar);

        yonetici.Cal(SesTuru.Puan);

        CollectionAssert.Contains(calar.CalinanSesler, SesTuru.Puan);
        Assert.AreEqual(SesDurumu.Acik, yonetici.Durum);
    }

    [TestMethod]
    public void SessizlikAcikken_SesCaliciyaHicGidilmez()
    {
        var calar = new SahteSesCalar();
        var yonetici = new SesYoneticisi(calar);

        yonetici.SessizligiDegistir();
        yonetici.Cal(SesTuru.Zipla);

        Assert.IsEmpty(calar.CalinanSesler);
        Assert.AreEqual(SesDurumu.Kapali, yonetici.Durum); // gri "Ses kapali (M)"
    }

    [TestMethod]
    public void IkinciM_SesiGeriAcar()
    {
        var yonetici = new SesYoneticisi(new SahteSesCalar());

        yonetici.SessizligiDegistir();
        yonetici.SessizligiDegistir();

        Assert.AreEqual(SesDurumu.Acik, yonetici.Durum);
    }

    [TestMethod]
    public void CihazYoksa_GostergeYokOlurVeSesCalinmaz()
    {
        var calar = new SahteSesCalar { KullanilabilirMi = false };
        var yonetici = new SesYoneticisi(calar);

        yonetici.Cal(SesTuru.Puan);

        Assert.IsEmpty(calar.CalinanSesler);
        Assert.AreEqual(SesDurumu.Yok, yonetici.Durum);    // kirmizi "Ses yok"
    }
}
