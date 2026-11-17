using AliBird.Core.Configuration;
using AliBird.Core.Gameplay;
using AliBird.Core.Tests.Fakes;

namespace AliBird.Core.Tests.Gameplay;

// Bir karenin dogru sirayla islendigini ve oyunun bitisini dogrular.
[TestClass]
public sealed class OyunMotoruTestleri
{
    private static OyunMotoru MotorOlustur(OyunAyarlari ayarlar)
        => new OyunMotoruFabrikasi(ayarlar, new SahteRastgele(150), new CarpismaDenetcisi(ayarlar), new EngelUretici(ayarlar)).Olustur(null);

    [TestMethod]
    public void Ilerlet_KusDuserVeHizArtar()
    {
        var motor = MotorOlustur(new OyunAyarlari());

        motor.Ilerlet(1f);

        Assert.AreEqual(208.6f, motor.KusAlani.Y, 0.0001f);   // yer cekimi uygulandi
        Assert.AreEqual(6.001f, motor.Hiz, 0.0001f);           // K8
    }

    [TestMethod]
    public void Ilerlet_GecilenEngelIcinPuanVerir()
    {
        // Ilk engeli kusun arkasina koy: x = -50, sag kenari 10 < kus 100 -> ilk karede gecilmis sayilir.
        var motor = MotorOlustur(new OyunAyarlari { IlkEngelX = -50f });

        KareSonucu sonuc = motor.Ilerlet(1f);

        Assert.AreEqual(1, sonuc.KazanilanPuan);
        Assert.AreEqual(1, motor.Puan);
    }

    [TestMethod]
    public void KusZemineDusunce_OyunBiterVeMotorDurur()
    {
        var motor = MotorOlustur(new OyunAyarlari());

        // Hic ziplamadan birakirsak kus zemine duser (~26 kare).
        KareSonucu sonuc = default;
        for (int i = 0; i < 200 && !motor.BittiMi; i++)
        {
            sonuc = motor.Ilerlet(1f);
        }

        Assert.IsTrue(motor.BittiMi);
        Assert.IsTrue(sonuc.Carpti);

        // Bittikten sonra hicbir sey degismez.
        float y = motor.KusAlani.Y;
        motor.Ilerlet(1f);
        Assert.AreEqual(y, motor.KusAlani.Y);
    }

    [TestMethod]
    public void Fabrika_HerCagrida_YeniVeTemizMotorVerir()
    {
        var ayarlar = new OyunAyarlari();
        var fabrika = new OyunMotoruFabrikasi(ayarlar, new SahteRastgele(150), new CarpismaDenetcisi(ayarlar), new EngelUretici(ayarlar));

        OyunMotoru birinci = fabrika.Olustur(null);
        birinci.Ilerlet(1f);
        OyunMotoru ikinci = fabrika.Olustur(null);

        Assert.AreNotSame(birinci, ikinci);
        Assert.AreEqual(208f, ikinci.KusAlani.Y);   // onceki oyundan iz kalmadi
    }
}
