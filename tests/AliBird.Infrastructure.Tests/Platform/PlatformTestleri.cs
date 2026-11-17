using AliBird.Infrastructure.Platform;

namespace AliBird.Infrastructure.Tests.Platform;

// Saat ve rastgele sayi ureticisinin sozlesmeye uydugunu dogrular.
[TestClass]
public sealed class PlatformTestleri
{
    [TestMethod]
    public void SistemSaati_SuAnkiYerelSaatiVerir()
    {
        DateTime once = DateTime.Now;
        DateTime saat = new SistemSaati().Simdi;
        DateTime sonra = DateTime.Now;

        Assert.IsTrue(saat >= once && saat <= sonra);
        Assert.AreEqual(DateTimeKind.Local, saat.Kind);
    }

    [TestMethod]
    public void SistemRastgele_AltSiniriDahilUstSiniriHaricTutar()
    {
        var rastgele = new SistemRastgele();

        for (int i = 0; i < 1000; i++)
        {
            int sayi = rastgele.Sonraki(60, 231);
            Assert.IsTrue(sayi is >= 60 and <= 230);
        }
    }
}
