using AliBird.Core.Models;
using AliBird.Core.Services;
using AliBird.Core.Tests.Fakes;

namespace AliBird.Core.Tests.Services;

// Kare araligi olcumunu dogrular.
[TestClass]
public sealed class KareSuresiOlcerTestleri
{
    private readonly SahteKronometre _kronometre = new();

    [TestMethod]
    public void TekKare_OlcumYok()
    {
        var olcer = new KareSuresiOlcer(_kronometre);

        olcer.KareGeldi();

        Assert.IsNull(olcer.Ozet());
    }

    [TestMethod]
    public void Araliklar_OrtalamaMinMaxDogru()
    {
        var olcer = new KareSuresiOlcer(_kronometre);

        olcer.KareGeldi();
        _kronometre.Ilerlet(10); olcer.KareGeldi();
        _kronometre.Ilerlet(20); olcer.KareGeldi();
        _kronometre.Ilerlet(30); olcer.KareGeldi();

        KareOlcumu? ozet = olcer.Ozet();
        Assert.IsNotNull(ozet);
        Assert.AreEqual(3, ozet.AralikSayisi);
        Assert.AreEqual(20.0, ozet.OrtalamaMs, 0.0001);
        Assert.AreEqual(10.0, ozet.EnAzMs, 0.0001);
        Assert.AreEqual(30.0, ozet.EnCokMs, 0.0001);
        Assert.AreEqual(50.0, ozet.Fps, 0.0001);
    }

    [TestMethod]
    public void AraVer_DuraklatmaSuresiOlcumeGirmez()
    {
        var olcer = new KareSuresiOlcer(_kronometre);

        olcer.KareGeldi();
        _kronometre.Ilerlet(16); olcer.KareGeldi();
        olcer.AraVer();
        _kronometre.Ilerlet(5000);   // 5 sn duraklatma
        olcer.KareGeldi();
        _kronometre.Ilerlet(16); olcer.KareGeldi();

        KareOlcumu? ozet = olcer.Ozet();
        Assert.IsNotNull(ozet);
        Assert.AreEqual(2, ozet.AralikSayisi);
        Assert.AreEqual(16.0, ozet.EnCokMs, 0.0001);
    }

    [TestMethod]
    public void Sifirla_OlcumuSiler()
    {
        var olcer = new KareSuresiOlcer(_kronometre);
        olcer.KareGeldi();
        _kronometre.Ilerlet(16); olcer.KareGeldi();

        olcer.Sifirla();

        Assert.IsNull(olcer.Ozet());
    }
}
