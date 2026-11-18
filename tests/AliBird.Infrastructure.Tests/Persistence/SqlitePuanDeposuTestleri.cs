using AliBird.Core.Errors;
using AliBird.Core.Models;
using AliBird.Infrastructure.FileSystem;
using AliBird.Infrastructure.Persistence;
using AliBird.Infrastructure.Tests.Fakes;

namespace AliBird.Infrastructure.Tests.Persistence;

// Gercek bir SQLite dosyasi ile (gecici klasorde) rekor okuma/yazma ve hata kurallarini dogrular.
// depoya GERCEK BozukVeritabaniKurtarici verilir; log ve hata bildirimi sahtedir.
[TestClass]
public sealed class SqlitePuanDeposuTestleri
{
    private GeciciKlasor _kok = null!;
    private UygulamaYollari _yollar = null!;
    private SahteHataBildirici _hata = null!;
    private SahteGunlukcu _gunlukcu = null!;
    private SqlitePuanDeposu _depo = null!;

    [TestInitialize]
    public void Hazirla()
    {
        _kok = new GeciciKlasor();
        _yollar = new UygulamaYollari(_kok.Yol);
        _hata = new SahteHataBildirici();
        _gunlukcu = new SahteGunlukcu();
        var kurtarici = new BozukVeritabaniKurtarici(_yollar, _hata, _gunlukcu, new SahteSaat());
        _depo = new SqlitePuanDeposu(_yollar, kurtarici, _gunlukcu);
    }

    [TestCleanup]
    public void Temizle() => _kok.Dispose();

    [TestMethod]
    public async Task IlkAcilis_RekorYok_NullDoner()
    {
        Assert.IsNull(await _depo.RekoruOkuAsync());
        Assert.IsTrue(File.Exists(_yollar.VeritabaniDosyasi()));   // dosya olusturuldu
        Assert.IsTrue(_gunlukcu.Kayitlar.Any(k => k.Contains("yeniden oluşturuldu")));
    }

    [TestMethod]
    public async Task KaydedilenRekor_PuaniVeTarihiyleGeriOkunur()
    {
        var tarih = new DateTime(2026, 11, 17, 15, 8, 30, 123, DateTimeKind.Local);

        await _depo.RekoruKaydetAsync(new RekorKaydi(12, tarih));
        RekorKaydi? okunan = await _depo.RekoruOkuAsync();

        Assert.AreEqual(12, okunan!.Puan);
        Assert.AreEqual(tarih, okunan.Tarih);   // an kaybolmadi
    }

    [TestMethod]
    public async Task IkinciKayit_IlkiniGunceller_TabloTekSatirKalir()
    {
        await _depo.RekoruKaydetAsync(new RekorKaydi(5, DateTime.Now));
        await _depo.RekoruKaydetAsync(new RekorKaydi(9, DateTime.Now));

        Assert.AreEqual(9, (await _depo.RekoruOkuAsync())!.Puan);   // tek rekor
    }

    [TestMethod]
    public async Task KullaniciVeritabaniniSilerse_HataVermedenYenidenOlusur()
    {
        await _depo.RekoruKaydetAsync(new RekorKaydi(5, DateTime.Now));

        // baglanti kapali oldugu icin dosya kilitli degil, silinebilir.
        File.Delete(_yollar.VeritabaniDosyasi());

        Assert.IsNull(await _depo.RekoruOkuAsync());                 // hata yok
        await _depo.RekoruKaydetAsync(new RekorKaydi(3, DateTime.Now));
        Assert.AreEqual(3, (await _depo.RekoruOkuAsync())!.Puan);
        Assert.IsEmpty(_hata.Bildirimler);                           // kullaniciya bir sey gosterilmedi
        Assert.IsTrue(_gunlukcu.Kayitlar.Any(k => k.Contains("yeniden oluşturuldu")));   // ama loga yazildi
    }

    [TestMethod]
    public async Task KullaniciDataKlasorunuSilerse_HataVermedenYenidenOlusur()
    {
        await _depo.RekoruKaydetAsync(new RekorKaydi(5, DateTime.Now));

        Directory.Delete(_yollar.VeriKlasoru(), recursive: true);

        Assert.IsNull(await _depo.RekoruOkuAsync());
    }

    [TestMethod]
    public async Task BozukDosya_KenaraAlinirKullaniciyaBildirilirVeYeniDosyaCalisir()
    {
        // Veritabani yerine duz metin yaz: SQLite "bu bir veritabani degil" der (kod 26).
        string dosya = _yollar.VeritabaniDosyasi();
        await File.WriteAllTextAsync(dosya, new string('x', 4096));

        RekorKaydi? okunan = await _depo.RekoruOkuAsync();

        Assert.IsNull(okunan);                                   // rekor sifirlandi
        Assert.AreEqual(1, _hata.EkranaGidenSayisi);             // kullanici bilgilendirildi
        Assert.HasCount(1, Directory.GetFiles(_yollar.VeriKlasoru(), "alibird.corrupt-*.db"));   // silinmedi

        await _depo.RekoruKaydetAsync(new RekorKaydi(4, DateTime.Now));
        Assert.AreEqual(4, (await _depo.RekoruOkuAsync())!.Puan);   // yeni dosya calisiyor
    }

    [TestMethod]
    public async Task BozukDosyayaKayit_RekorKaybolmadanYeniDosyayaYazilir()
    {
        await File.WriteAllTextAsync(_yollar.VeritabaniDosyasi(), new string('x', 4096));

        await _depo.RekoruKaydetAsync(new RekorKaydi(7, DateTime.Now));   // exception yok

        Assert.AreEqual(7, (await _depo.RekoruOkuAsync())!.Puan);
        Assert.AreEqual(1, _hata.EkranaGidenSayisi);
    }

    [TestMethod]
    public async Task KilitliDosya_PuanDeposuHatasinaSarilir()
    {
        // Baska bir program dosyayi kilitlediyse: beklenen hata, Core'un tanidigi tipe sarilir.
        string dosya = _yollar.VeritabaniDosyasi();
        await File.WriteAllBytesAsync(dosya, []);
        using (new FileStream(dosya, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            await Assert.ThrowsExactlyAsync<PuanDeposuHatasi>(() => _depo.RekoruOkuAsync());
        }
        Assert.IsEmpty(_hata.Bildirimler);   // bozuk sayilmadi, dosya kenara alinmadi
    }
}
