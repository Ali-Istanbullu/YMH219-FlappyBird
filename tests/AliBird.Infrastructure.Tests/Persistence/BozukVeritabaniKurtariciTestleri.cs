using AliBird.Core.Models;
using AliBird.Infrastructure.FileSystem;
using AliBird.Infrastructure.Persistence;
using AliBird.Infrastructure.Tests.Fakes;
using Microsoft.Data.Sqlite;

namespace AliBird.Infrastructure.Tests.Persistence;

// Bozuk dosyanin taninmasi ve kenara alinmasi (M24 #4, CK23, K63, R6).
[TestClass]
public sealed class BozukVeritabaniKurtariciTestleri
{
    private GeciciKlasor _kok = null!;
    private UygulamaYollari _yollar = null!;
    private SahteHataBildirici _hata = null!;
    private SahteGunlukcu _gunlukcu = null!;
    private BozukVeritabaniKurtarici _kurtarici = null!;

    [TestInitialize]
    public void Hazirla()
    {
        _kok = new GeciciKlasor();
        _yollar = new UygulamaYollari(_kok.Yol);
        _hata = new SahteHataBildirici();
        _gunlukcu = new SahteGunlukcu();
        _kurtarici = new BozukVeritabaniKurtarici(_yollar, _hata, _gunlukcu, new SahteSaat());
    }

    [TestCleanup]
    public void Temizle() => _kok.Dispose();

    [TestMethod]
    public void BozukMu_BozuklukHatalariniTanir()
    {
        Assert.IsTrue(_kurtarici.BozukMu(new FormatException()));
        Assert.IsTrue(_kurtarici.BozukMu(new SqliteException("bozuk", 11)));          // SQLITE_CORRUPT
        Assert.IsTrue(_kurtarici.BozukMu(new SqliteException("veritabani degil", 26))); // SQLITE_NOTADB
    }

    [TestMethod]
    public void BozukMu_DigerHatalariBozukSaymaz()
    {
        Assert.IsFalse(_kurtarici.BozukMu(new IOException("disk dolu")));
        Assert.IsFalse(_kurtarici.BozukMu(new SqliteException("kilitli", 5)));        // SQLITE_BUSY
    }

    [TestMethod]
    public void KenaraAl_DosyayiYenidenAdlandirirLoglarVeKullaniciyaBildirir()
    {
        string dosya = _yollar.VeritabaniDosyasi();
        File.WriteAllText(dosya, "bozuk");

        _kurtarici.KenaraAl(new FormatException("bozuk tarih"));

        Assert.IsFalse(File.Exists(dosya));
        Assert.HasCount(1, Directory.GetFiles(_yollar.VeriKlasoru(), "alibird.corrupt-*.db"));   // CK23
        Assert.IsTrue(_gunlukcu.Kayitlar.Any(k => k.Contains("kenara alındı")));                  // K63
        Assert.AreEqual(Gorunurluk.EkranVeGunluk, _hata.Bildirimler.Single().Gorunurluk);         // M24 #4
    }

    [TestMethod]
    public void DosyaYokken_CokmezSadeceBildirir()
    {
        _kurtarici.KenaraAl(new FormatException("x"));   // R6: exception yok

        Assert.AreEqual(1, _hata.EkranaGidenSayisi);
    }
}
