using AliBird.Core.Configuration;
using AliBird.Core.Gameplay;
using AliBird.Core.Models;
using AliBird.Core.Presentation;
using AliBird.Core.Services;
using AliBird.Core.Tests.Fakes;

namespace AliBird.Core.Tests.Presentation;

// Presenter'in durum gecislerini, ses, rekor ve hata davranisini dogrular
// (K11, K13, K14, K52, K63, CK1-CK7, M24).
// Adim 8c'den sonra presenter'a GERCEK RekorYoneticisi/SesYoneticisi/KareHataSayaci verilir;
// bunlarin altindaki dis dunya (depo, ses calar, log) sahte siniflardir.
[TestClass]
public sealed class OyunYoneticisiTestleri
{
    private SahteGorunum _gorunum = null!;
    private SahtePuanDeposu _depo = null!;
    private SahteSesCalar _ses = null!;
    private SahteZamanlayici _zamanlayici = null!;
    private SahteHataBildirici _hata = null!;
    private SahteSaat _saat = null!;
    private SahteGunlukcu _gunlukcu = null!;

    // Her testten once temiz sahte nesneler.
    [TestInitialize]
    public void Hazirla()
    {
        _gorunum = new SahteGorunum();
        _depo = new SahtePuanDeposu();
        _ses = new SahteSesCalar();
        _zamanlayici = new SahteZamanlayici();
        _hata = new SahteHataBildirici();
        _saat = new SahteSaat();
        _gunlukcu = new SahteGunlukcu();
    }

    private async Task<OyunYoneticisi> BaslatilmisYonetici(OyunAyarlari? ayarlar = null)
    {
        ayarlar ??= new OyunAyarlari();
        var fabrika = new OyunMotoruFabrikasi(ayarlar, new SahteRastgele(150), new CarpismaDenetcisi(ayarlar), new EngelUretici(ayarlar));
        var yonetici = new OyunYoneticisi(
            _gorunum,
            fabrika,
            new RekorYoneticisi(_depo, _hata, _gunlukcu, _saat),
            new SesYoneticisi(_ses, _hata),
            new KareHataSayaci(_hata, ayarlar),
            new KareSuresiOlcer(new SahteKronometre()),
            _zamanlayici,
            _hata,
            _gunlukcu,
            _saat,
            ayarlar);
        await yonetici.BaslatAsync();
        return yonetici;
    }

    // Kus hic ziplamadan zemine dusene kadar kare ilerletir.
    private void OyunBitenekadarIlerlet(OyunYoneticisi yonetici)
    {
        for (int i = 0; i < 200 && yonetici.Durum != OyunDurumu.OyunBitti; i++)
        {
            _zamanlayici.Tikla();
        }
    }

    [TestMethod]
    public async Task Baslat_RekoruOkurHazirGoruntuGosterirVeZamanlayiciyiCalistirir()
    {
        _depo.KayitliRekor = new RekorKaydi(7, _saat.Simdi);

        var yonetici = await BaslatilmisYonetici();

        Assert.AreEqual(OyunDurumu.Hazir, yonetici.Durum);
        Assert.AreEqual(7, _gorunum.SonGoruntu!.Rekor!.Puan);
        Assert.IsTrue(_zamanlayici.CalisiyorMu);
        Assert.AreEqual(16, _zamanlayici.AralikMs);
    }

    [TestMethod]
    public async Task RekorOkunamazsa_EkranaBildirilirVeOyunRekorsuzAcilir()
    {
        _depo.OkumaHataFirlatsin = true;

        var yonetici = await BaslatilmisYonetici();

        Assert.AreEqual(1, _hata.EkranaGidenSayisi);   // CK4, M24 #1
        Assert.AreEqual(OyunDurumu.Hazir, yonetici.Durum);
        Assert.IsNull(_gorunum.SonGoruntu!.Rekor);
    }

    [TestMethod]
    public async Task HazirdaSpace_OyunuBaslatirVeZiplamaSesiCalar()
    {
        var yonetici = await BaslatilmisYonetici();

        _gorunum.BoslugaBas();

        Assert.AreEqual(OyunDurumu.Calisiyor, yonetici.Durum);
        CollectionAssert.Contains(_ses.CalinanSesler, SesTuru.Zipla);
    }

    [TestMethod]
    public async Task Hazirda_KusHareketEtmez()
    {
        await BaslatilmisYonetici();

        _zamanlayici.Tikla(10);

        Assert.AreEqual(208f, _gorunum.SonGoruntu!.KusAlani.Y);   // E2a
    }

    [TestMethod]
    public async Task P_DuraklatirVeDevamEttirir_DuraklamadaSpaceEtkisizdir()
    {
        var yonetici = await BaslatilmisYonetici();
        _gorunum.BoslugaBas();

        _gorunum.PyeBas();
        Assert.AreEqual(OyunDurumu.Duraklatildi, yonetici.Durum);

        _gorunum.BoslugaBas();                                     // CK1: etkisiz
        Assert.AreEqual(OyunDurumu.Duraklatildi, yonetici.Durum);

        _gorunum.PyeBas();
        Assert.AreEqual(OyunDurumu.Calisiyor, yonetici.Durum);
    }

    [TestMethod]
    public async Task OdakKaybi_OyunuDuraklatir()
    {
        var yonetici = await BaslatilmisYonetici();
        _gorunum.BoslugaBas();

        _gorunum.OdagiKaybet();

        Assert.AreEqual(OyunDurumu.Duraklatildi, yonetici.Durum);   // K11
    }

    [TestMethod]
    public async Task M_SesiKapatir_SesCalinmazVeGostergeKapaliOlur()
    {
        await BaslatilmisYonetici();

        _gorunum.MyeBas();
        _gorunum.BoslugaBas();
        _zamanlayici.Tikla();

        Assert.IsEmpty(_ses.CalinanSesler);                              // D4b
        Assert.AreEqual(SesDurumu.Kapali, _gorunum.SonGoruntu!.SesDurumu); // K12
    }

    [TestMethod]
    public async Task SesCihaziYoksa_GostergeYokOlurVeOyunOynanir()
    {
        _ses.KullanilabilirMi = false;
        var yonetici = await BaslatilmisYonetici();

        _gorunum.BoslugaBas();
        _zamanlayici.Tikla();

        Assert.AreEqual(SesDurumu.Yok, _gorunum.SonGoruntu!.SesDurumu);
        Assert.AreEqual(OyunDurumu.Calisiyor, yonetici.Durum);
    }

    [TestMethod]
    public async Task Carpinca_OyunBiterVeIkiAyriSesCalar()
    {
        var yonetici = await BaslatilmisYonetici();
        _gorunum.BoslugaBas();

        OyunBitenekadarIlerlet(yonetici);

        Assert.AreEqual(OyunDurumu.OyunBitti, yonetici.Durum);
        CollectionAssert.Contains(_ses.CalinanSesler, SesTuru.Carpma);     // K13
        CollectionAssert.Contains(_ses.CalinanSesler, SesTuru.OyunBitti);
    }

    [TestMethod]
    public async Task GameOverdan_Hemen_SpaceYokSayilir_750msSonraYeniOyunHazirlanir()
    {
        var yonetici = await BaslatilmisYonetici();
        _gorunum.BoslugaBas();
        OyunBitenekadarIlerlet(yonetici);

        _saat.Ileri(500);
        _gorunum.BoslugaBas();
        Assert.AreEqual(OyunDurumu.OyunBitti, yonetici.Durum);   // CK2

        _saat.Ileri(300);
        _gorunum.BoslugaBas();
        Assert.AreEqual(OyunDurumu.Hazir, yonetici.Durum);       // CK7
    }

    [TestMethod]
    public async Task YenidenBaslatButonu_BeklemedenYeniOyunHazirlar()
    {
        var yonetici = await BaslatilmisYonetici();
        _gorunum.BoslugaBas();
        OyunBitenekadarIlerlet(yonetici);

        _gorunum.YenidenBaslatTikla();

        Assert.AreEqual(OyunDurumu.Hazir, yonetici.Durum);
    }

    [TestMethod]
    public async Task OyunBitince_YeniRekorKaydedilir()
    {
        // Ilk engel kusun arkasinda: ilk karede 1 puan (rekor yokken 1+ puan ilk rekor olur, K50).
        var yonetici = await BaslatilmisYonetici(new OyunAyarlari { IlkEngelX = -50f });
        _gorunum.BoslugaBas();

        OyunBitenekadarIlerlet(yonetici);

        Assert.AreEqual(1, _depo.KaydetmeSayisi);
        Assert.AreEqual(1, _depo.KayitliRekor!.Puan);
        Assert.AreEqual(_saat.Simdi, _depo.KayitliRekor.Tarih);
    }

    [TestMethod]
    public async Task SifirPuanlaBitenIlkOyun_RekorKaydetmez()
    {
        var yonetici = await BaslatilmisYonetici();
        _gorunum.BoslugaBas();

        OyunBitenekadarIlerlet(yonetici);

        Assert.AreEqual(0, _depo.KaydetmeSayisi);   // K50
    }

    [TestMethod]
    public async Task RekorKaydedilemezse_KullaniciyaBildirilir()
    {
        _depo.KaydetmeHataFirlatsin = true;
        var yonetici = await BaslatilmisYonetici(new OyunAyarlari { IlkEngelX = -50f });
        _gorunum.BoslugaBas();

        OyunBitenekadarIlerlet(yonetici);

        Assert.AreEqual(1, _hata.EkranaGidenSayisi);   // M24 #2
    }

    [TestMethod]
    public async Task OyunSurerkenKapanirsa_RekorKaydedilir()
    {
        var yonetici = await BaslatilmisYonetici(new OyunAyarlari { IlkEngelX = -50f });
        _gorunum.BoslugaBas();
        _zamanlayici.Tikla();   // 1 puan, oyun suruyor

        await yonetici.KapatAsync();

        Assert.AreEqual(1, _depo.KayitliRekor!.Puan);   // K52 b
        Assert.IsFalse(_zamanlayici.CalisiyorMu);
    }

    [TestMethod]
    public async Task OyunBittiktenSonraKapanirsa_RekorIkinciKezYazilmaz()
    {
        var yonetici = await BaslatilmisYonetici(new OyunAyarlari { IlkEngelX = -50f });
        _gorunum.BoslugaBas();
        OyunBitenekadarIlerlet(yonetici);

        await yonetici.KapatAsync();

        Assert.AreEqual(1, _depo.KaydetmeSayisi);
    }

    [TestMethod]
    public async Task UstUsteUcKareHatasi_OyunuDurdururVeEkranaSadeceBirKezBildirir()
    {
        var yonetici = await BaslatilmisYonetici();
        _gorunum.BoslugaBas();
        _gorunum.GosterHataFirlatsin = true;

        _zamanlayici.Tikla(10);

        Assert.AreEqual(OyunDurumu.OyunBitti, yonetici.Durum);   // CK3
        Assert.AreEqual(1, _hata.EkranaGidenSayisi);              // M24 #7: tekrar tekrar gosterilmez
        Assert.HasCount(3, _hata.Bildirimler);                   // log da her karede dolmaz
    }

    [TestMethod]
    public async Task OyunBitince_PuanLogaYazilir()
    {
        var yonetici = await BaslatilmisYonetici();
        _gorunum.BoslugaBas();

        OyunBitenekadarIlerlet(yonetici);

        // K63 + CK53: puan ve olculen kare araligi ayni satirda.
        string? satir = _gunlukcu.Bilgiler.FirstOrDefault(m => m.StartsWith("Oyun bitti.", StringComparison.Ordinal));
        Assert.IsNotNull(satir);
        Assert.StartsWith("Oyun bitti. Puan: 0. Kare araligi: ort ", satir);
    }

    [TestMethod]
    public async Task YeniRekorKaydedilince_LogaYazilir()
    {
        var yonetici = await BaslatilmisYonetici(new OyunAyarlari { IlkEngelX = -50f });
        _gorunum.BoslugaBas();

        OyunBitenekadarIlerlet(yonetici);

        Assert.IsTrue(_gunlukcu.Bilgiler.Any(b => b.StartsWith("Yeni rekor kaydedildi: 1")));   // K63
    }
}
