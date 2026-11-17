using AliBird.Core.Models;
using AliBird.Infrastructure.Audio;
using AliBird.Infrastructure.Tests.Fakes;
using NAudio.Wave;

namespace AliBird.Infrastructure.Tests.Audio;

// Ses calicinin hicbir durumda cokmedigini dogrular (R6, M24 #6).
// Not: Bu testler hoparlorden ses CALMAZ (Cal cagrilari ya devre disi durumda ya da Dispose sonrasi).
[TestClass]
public sealed class NAudioSesCalarTestleri
{
    [TestMethod]
    public void SesHazirlanamazsa_CokmezKullanilamazOlurVeLogaYazar()
    {
        var hata = new SahteHataBildirici();

        var calar = new NAudioSesCalar(new HataVerenSesUreteci(), hata);

        Assert.IsFalse(calar.KullanilabilirMi);                         // sag altta "Ses yok"
        Assert.AreEqual(Gorunurluk.SadeceGunluk, hata.Bildirimler.Single().Gorunurluk);
        calar.Cal(SesTuru.Zipla);                                         // exception yok
        calar.Dispose();
    }

    [TestMethod]
    public void KapattiktanSonra_CalVeTekrarKapat_Cokmez()
    {
        // Gercek uretici: bilgisayarda ses cihazi varsa acilir, yoksa sessiz moda gecer. Iki durumda da cokmemeli.
        var calar = new NAudioSesCalar(new SesUreteci(), new SahteHataBildirici());

        calar.Dispose();

        Assert.IsFalse(calar.KullanilabilirMi);
        calar.Cal(SesTuru.Puan);   // kapali: hicbir sey calmaz, exception yok
        calar.Dispose();           // iki kez kapatmak da sorun degil
    }

    [TestMethod]
    public void DiziSaglayici_DiziyiParcaParcaVerirSonundaSifirDoner()
    {
        var saglayici = new DiziSesSaglayici([0.1f, 0.2f, 0.3f], WaveFormat.CreateIeeeFloatWaveFormat(44100, 1));
        var tampon = new float[2];

        Assert.AreEqual(2, saglayici.Read(tampon));
        Assert.AreEqual(1, saglayici.Read(tampon));
        Assert.AreEqual(0.3f, tampon[0]);
        Assert.AreEqual(0, saglayici.Read(tampon));   // bitti
    }
}
