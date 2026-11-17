using AliBird.Core.Models;
using AliBird.Core.Services;
using AliBird.Core.Tests.Fakes;

namespace AliBird.Core.Tests.Services;

// Ses kapali/acik karari, gosterge durumu ve cokmeme (D4b, K12, K13, CK5, R6).
[TestClass]
public sealed class SesYoneticisiTestleri
{
    [TestMethod]
    public void Acikken_SesCalinir()
    {
        var calar = new SahteSesCalar();
        var yonetici = new SesYoneticisi(calar, new SahteHataBildirici());

        yonetici.Cal(SesTuru.Puan);

        CollectionAssert.Contains(calar.CalinanSesler, SesTuru.Puan);
        Assert.AreEqual(SesDurumu.Acik, yonetici.Durum);
    }

    [TestMethod]
    public void SessizlikAcikken_SesCaliciyaHicGidilmez()
    {
        var calar = new SahteSesCalar();
        var yonetici = new SesYoneticisi(calar, new SahteHataBildirici());

        yonetici.SessizligiDegistir();
        yonetici.Cal(SesTuru.Zipla);

        Assert.IsEmpty(calar.CalinanSesler);              // D4b
        Assert.AreEqual(SesDurumu.Kapali, yonetici.Durum); // K12: gri "Ses kapali (M)"
    }

    [TestMethod]
    public void IkinciM_SesiGeriAcar()
    {
        var yonetici = new SesYoneticisi(new SahteSesCalar(), new SahteHataBildirici());

        yonetici.SessizligiDegistir();
        yonetici.SessizligiDegistir();

        Assert.AreEqual(SesDurumu.Acik, yonetici.Durum);
    }

    [TestMethod]
    public void CihazYoksa_GostergeYokOlurVeSesCalinmaz()
    {
        var calar = new SahteSesCalar { KullanilabilirMi = false };
        var yonetici = new SesYoneticisi(calar, new SahteHataBildirici());

        yonetici.Cal(SesTuru.Puan);

        Assert.IsEmpty(calar.CalinanSesler);
        Assert.AreEqual(SesDurumu.Yok, yonetici.Durum);    // K12: kirmizi "Ses yok"
    }
}
