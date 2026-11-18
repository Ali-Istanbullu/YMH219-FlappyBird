using AliBird.Core.Configuration;
using AliBird.Core.Gameplay;

namespace AliBird.Core.Tests.Gameplay;

// Hiz egrisini dogrular (6 -> 13, kare basina +0.001).
[TestClass]
public sealed class HizDenetcisiTestleri
{
    private readonly OyunAyarlari _ayarlar = new();

    [TestMethod]
    public void YeniDenetci_BaslangicHiziAltidir()
    {
        Assert.AreEqual(6f, new HizDenetcisi(_ayarlar).Hiz);
    }

    [TestMethod]
    public void BinKareSonra_HizYaklasikYedidir()
    {
        var denetci = new HizDenetcisi(_ayarlar);

        for (int i = 0; i < 1000; i++)
        {
            denetci.KareIlerlet(1f);
        }

        // 6 + 1000 x 0.001 = 7. float toplama hatasi icin kucuk tolerans.
        Assert.AreEqual(7f, denetci.Hiz, 0.01f);
    }

    [TestMethod]
    public void UzunSureSonra_HizOnUcteSabitlenir()
    {
        var denetci = new HizDenetcisi(_ayarlar);

        // 7000 kare tavana ulasmaya yeter; 10000 ile tavanin asilmadigini da goruyoruz.
        for (int i = 0; i < 10000; i++)
        {
            denetci.KareIlerlet(1f);
        }

        Assert.AreEqual(13f, denetci.Hiz);
    }
}
