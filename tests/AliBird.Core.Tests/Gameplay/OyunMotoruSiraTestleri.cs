using AliBird.Core.Gameplay;
using AliBird.Core.Tests.Fakes;

namespace AliBird.Core.Tests.Gameplay;

// OyunMotoru'nu SAHTE parcalarla test eder (K66): fizik/carpisma hesabindan bagimsiz olarak
// sadece motorun kendi isini (dogru sira, puan aktarimi, bitince durma) dogrular.
[TestClass]
public sealed class OyunMotoruSiraTestleri
{
    private readonly List<string> _cagrilar = [];
    private SahteKus _kus = null!;
    private SahteEngelAlani _engelAlani = null!;
    private SahteCarpismaDenetcisi _carpisma = null!;
    private SahtePuanTablosu _puan = null!;
    private OyunMotoru _motor = null!;

    [TestInitialize]
    public void Hazirla()
    {
        _kus = new SahteKus(_cagrilar);
        _engelAlani = new SahteEngelAlani(_cagrilar);
        _carpisma = new SahteCarpismaDenetcisi(_cagrilar);
        _puan = new SahtePuanTablosu();
        _motor = new OyunMotoru(_kus, _engelAlani, _carpisma, new SahteHizDenetcisi(_cagrilar), _puan);
    }

    [TestMethod]
    public void Ilerlet_ParcalariDogruSiradaCagirir()
    {
        _motor.Ilerlet(1f);

        CollectionAssert.AreEqual(new[] { "kus", "engel", "gecis", "carpisma", "hiz" }, _cagrilar);
        Assert.AreEqual(7f, _engelAlani.SonHiz);   // engeller guncel hizla kayar
    }

    [TestMethod]
    public void Ilerlet_GecilenEngelSayisiPuanaEklenir()
    {
        _engelAlani.GecilenSayisi = 2;

        KareSonucu sonuc = _motor.Ilerlet(1f);

        Assert.AreEqual(2, sonuc.KazanilanPuan);
        Assert.AreEqual(2, _puan.Puan);
    }

    [TestMethod]
    public void Carpinca_MotorDururVeParcalarArtikCagrilmaz()
    {
        _carpisma.Carpti = true;
        _motor.Ilerlet(1f);
        _cagrilar.Clear();

        KareSonucu sonuc = _motor.Ilerlet(1f);
        _motor.Zipla();

        Assert.IsTrue(_motor.BittiMi);
        Assert.IsFalse(sonuc.Carpti);
        Assert.IsEmpty(_cagrilar);
        Assert.AreEqual(0, _kus.ZiplamaSayisi);
    }
}
