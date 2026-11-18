using AliBird.Core.Models;
using AliBird.Infrastructure.Audio;
using AliBird.Infrastructure.Tests.Fakes;
using NAudio.Wave;

namespace AliBird.Infrastructure.Tests.Audio;

// Ses calicinin cihaz durumlarinda cokmedigini, bug'lari ise gizlemedigini dogrular.
// Not: Bu testler hoparlorden ses CALMAZ (Cal cagrilari ya devre disi durumda ya da Dispose sonrasi).
[TestClass]
public sealed class NAudioSesCalarTestleri
{
    [TestMethod]
    public void SesUretimindekiBug_Yutulmaz()
    {
        // Ses uretimi saf hesaptir; oradaki hata bir bug'dir ve gizlenmemeli.
        var hata = new SahteHataBildirici();

        Assert.ThrowsExactly<InvalidOperationException>(() =>
            new NAudioSesCalar(new HataVerenSesUreteci(), new DiziSesSaglayiciUretici(), hata, new SahteGunlukcu()));
        Assert.IsEmpty(hata.Bildirimler);
    }

    [TestMethod]
    public void KapattiktanSonra_CalVeTekrarKapat_Cokmez()
    {
        // Gercek uretici: bilgisayarda ses cihazi varsa acilir, yoksa sessiz moda gecer. Iki durumda da cokmemeli.
        var calar = new NAudioSesCalar(new SesUreteci(), new DiziSesSaglayiciUretici(), new SahteHataBildirici(), new SahteGunlukcu());

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

    [TestMethod]
    public void SaglayiciUretici_HerCagridaYeniVeBastanOkunanSaglayiciVerir()
    {
        var uretici = new DiziSesSaglayiciUretici();
        var bicim = WaveFormat.CreateIeeeFloatWaveFormat(44100, 1);
        float[] ses = [0.1f, 0.2f];

        ISampleProvider birinci = uretici.Olustur(ses, bicim);
        birinci.Read(new float[2]);                    // birinci sonuna kadar okundu
        ISampleProvider ikinci = uretici.Olustur(ses, bicim);

        Assert.AreNotSame(birinci, ikinci);
        Assert.AreEqual(2, ikinci.Read(new float[2]));  // ikinci bastan okunuyor: ayni ses ust uste calabilir
    }
}
