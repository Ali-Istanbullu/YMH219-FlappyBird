using AliBird.Core.Models;
using AliBird.Core.Services;
using AliBird.Core.Tests.Fakes;

namespace AliBird.Core.Tests.Services;

// Rekorun okunmasi, oyun basina bir kez kaydedilmesi ve loglanmasi (K10, K50, K52, K63, CK4, CK12, M24).
[TestClass]
public sealed class RekorYoneticisiTestleri
{
    private SahtePuanDeposu _depo = null!;
    private SahteHataBildirici _hata = null!;
    private SahteGunlukcu _gunlukcu = null!;
    private SahteSaat _saat = null!;
    private RekorYoneticisi _yonetici = null!;

    [TestInitialize]
    public void Hazirla()
    {
        _depo = new SahtePuanDeposu();
        _hata = new SahteHataBildirici();
        _gunlukcu = new SahteGunlukcu();
        _saat = new SahteSaat();
        _yonetici = new RekorYoneticisi(_depo, _hata, _gunlukcu, _saat);
    }

    [TestMethod]
    public async Task Yukle_KayitliRekoruOkurVeLoglar()
    {
        _depo.KayitliRekor = new RekorKaydi(8, new DateTime(2026, 11, 17, 18, 0, 0));

        await _yonetici.YukleAsync();

        Assert.AreEqual(8, _yonetici.Rekor!.Puan);
        Assert.StartsWith("Rekor okundu: 8", _gunlukcu.Bilgiler.Single());   // K63
    }

    [TestMethod]
    public async Task Yukle_OkunamazsaEkranaBildirirVeRekorNullKalir()
    {
        _depo.OkumaHataFirlatsin = true;

        await _yonetici.YukleAsync();   // exception yok (R6)

        Assert.IsNull(_yonetici.Rekor);
        Assert.AreEqual(1, _hata.EkranaGidenSayisi);   // CK4
    }

    [TestMethod]
    public void GerekirseKaydet_AyniOyundaIkinciKezYazmaz()
    {
        _yonetici.GerekirseKaydet(kaydedilecekRekorVar: true, puan: 5);
        _yonetici.GerekirseKaydet(kaydedilecekRekorVar: true, puan: 5);

        Assert.AreEqual(1, _depo.KaydetmeSayisi);
        Assert.AreEqual(5, _yonetici.Rekor!.Puan);
    }

    [TestMethod]
    public void YeniOyunBasladiktan_SonraTekrarKaydedebilir()
    {
        _yonetici.GerekirseKaydet(true, 5);
        _yonetici.YeniOyunBasladi();
        _yonetici.GerekirseKaydet(true, 7);

        Assert.AreEqual(2, _depo.KaydetmeSayisi);
        Assert.AreEqual(7, _depo.KayitliRekor!.Puan);
    }

    [TestMethod]
    public void KaydedilecekRekorYoksa_HicYazmaz()
    {
        _yonetici.GerekirseKaydet(kaydedilecekRekorVar: false, puan: 3);

        Assert.AreEqual(0, _depo.KaydetmeSayisi);
    }

    [TestMethod]
    public void KayitBasarisizsa_EkranaBildirirAmaOturumdaRekorGecerli()
    {
        _depo.KaydetmeHataFirlatsin = true;

        _yonetici.GerekirseKaydet(true, 9);

        Assert.AreEqual(1, _hata.EkranaGidenSayisi);   // M24 #2
        Assert.AreEqual(9, _yonetici.Rekor!.Puan);     // CK12
    }

    [TestMethod]
    public async Task BekleyenKaydiBekle_KayitYokkenHemenDoner()
    {
        await _yonetici.BekleyenKaydiBekleAsync(2000);   // asili kalmaz
    }
}
