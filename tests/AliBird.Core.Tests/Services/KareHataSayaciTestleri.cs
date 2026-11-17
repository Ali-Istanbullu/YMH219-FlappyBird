using AliBird.Core.Configuration;
using AliBird.Core.Models;
using AliBird.Core.Services;
using AliBird.Core.Tests.Fakes;

namespace AliBird.Core.Tests.Services;

// Ardisik kare hatalarinin sayilmasi ve kullaniciya bir kez bildirilmesi (CK3, M24 #7 #8).
[TestClass]
public sealed class KareHataSayaciTestleri
{
    private SahteHataBildirici _hata = null!;
    private KareHataSayaci _sayac = null!;

    [TestInitialize]
    public void Hazirla()
    {
        _hata = new SahteHataBildirici();
        _sayac = new KareHataSayaci(_hata, new OyunAyarlari());   // sinir 3
    }

    [TestMethod]
    public void IlkIkiHata_SadeceLogaGiderOyunDurmaz()
    {
        Assert.IsFalse(_sayac.HatayiKaydet(new Exception("1")));
        Assert.IsFalse(_sayac.HatayiKaydet(new Exception("2")));

        Assert.AreEqual(0, _hata.EkranaGidenSayisi);
        Assert.HasCount(2, _hata.Bildirimler);
    }

    [TestMethod]
    public void UcuncuArdisikHata_BirKezEkranaGiderVeOyunuDurdurtur()
    {
        _sayac.HatayiKaydet(new Exception("1"));
        _sayac.HatayiKaydet(new Exception("2"));

        Assert.IsTrue(_sayac.HatayiKaydet(new Exception("3")));
        Assert.AreEqual(Gorunurluk.EkranVeGunluk, _hata.Bildirimler.Last().Gorunurluk);
    }

    [TestMethod]
    public void GosterildiktenSonra_AyniHatalarSessizKalir()
    {
        for (int i = 0; i < 10; i++)
        {
            _sayac.HatayiKaydet(new Exception("x"));
        }

        Assert.AreEqual(1, _hata.EkranaGidenSayisi);
        Assert.HasCount(3, _hata.Bildirimler);   // log dolmaz
    }

    [TestMethod]
    public void BasariliKare_ArdisikSayaciSifirlar()
    {
        _sayac.HatayiKaydet(new Exception("1"));
        _sayac.HatayiKaydet(new Exception("2"));
        _sayac.BasariliKare();

        Assert.IsFalse(_sayac.HatayiKaydet(new Exception("3")));   // seri bozuldu, yeniden 1
    }

    [TestMethod]
    public void Sifirla_YeniOyundaTekrarGosterebilir()
    {
        for (int i = 0; i < 3; i++) { _sayac.HatayiKaydet(new Exception("x")); }
        _sayac.Sifirla();
        for (int i = 0; i < 3; i++) { _sayac.HatayiKaydet(new Exception("y")); }

        Assert.AreEqual(2, _hata.EkranaGidenSayisi);
    }
}
