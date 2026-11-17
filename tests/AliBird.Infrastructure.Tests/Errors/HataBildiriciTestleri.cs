using AliBird.Core.Models;
using AliBird.Infrastructure.Errors;
using AliBird.Infrastructure.Tests.Fakes;

namespace AliBird.Infrastructure.Tests.Errors;

// Hatanin dogru kanala gittigini ve bildiricinin asla cokmedigini dogrular (K14, M24, CK18, R6).
[TestClass]
public sealed class HataBildiriciTestleri
{
    private SahteGunlukcu _gunlukcu = null!;
    private SahteKullaniciBildirici _kullanici = null!;
    private HataBildirici _bildirici = null!;

    [TestInitialize]
    public void Hazirla()
    {
        _gunlukcu = new SahteGunlukcu();
        _kullanici = new SahteKullaniciBildirici();
        _bildirici = new HataBildirici(_gunlukcu, _kullanici);
    }

    [TestMethod]
    public void SadeceGunluk_LogaYazarEkranaGostermez()
    {
        _bildirici.Bildir(new Exception("x"), Gorunurluk.SadeceGunluk, "mesaj");

        Assert.HasCount(1, _gunlukcu.Kayitlar);
        Assert.IsEmpty(_kullanici.GosterilenMesajlar);
    }

    [TestMethod]
    public void EkranVeGunluk_HemLogaYazarHemGosterir()
    {
        _bildirici.Bildir(new Exception("x"), Gorunurluk.EkranVeGunluk, "Rekor kaydedilemedi.");

        Assert.HasCount(1, _gunlukcu.Kayitlar);
        Assert.AreEqual("Rekor kaydedilemedi.", _kullanici.GosterilenMesajlar.Single());
    }

    [TestMethod]
    public void BosMesaj_GenelMesajlaGosterilir()
    {
        _bildirici.Bildir(new Exception("x"), Gorunurluk.EkranVeGunluk, "");

        Assert.AreEqual("Beklenmeyen bir hata oluştu.", _kullanici.GosterilenMesajlar.Single());   // CK18
    }

    [TestMethod]
    public void MesajKutusuHataVerirse_CokmezVeBuDaLogaYazilir()
    {
        _kullanici.HataFirlatsin = true;

        _bildirici.Bildir(new Exception("x"), Gorunurluk.EkranVeGunluk, "m");   // exception yok

        Assert.HasCount(2, _gunlukcu.Kayitlar);   // asil hata + gosterilemedi kaydi
    }

    [TestMethod]
    public void LogYazilamazsa_CokmezVeMesajYineGosterilir()
    {
        _gunlukcu.HataFirlatsin = true;

        _bildirici.Bildir(new Exception("x"), Gorunurluk.EkranVeGunluk, "m");   // exception yok

        Assert.HasCount(1, _kullanici.GosterilenMesajlar);
    }

    [TestMethod]
    public void HerSeyHataVerse_BileCokmez()
    {
        _gunlukcu.HataFirlatsin = true;
        _kullanici.HataFirlatsin = true;

        _bildirici.Bildir(null!, Gorunurluk.EkranVeGunluk, "m");   // R6: hicbir exception disari cikmaz
    }
}
