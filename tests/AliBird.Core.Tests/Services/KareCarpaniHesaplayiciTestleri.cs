using AliBird.Core.Configuration;
using AliBird.Core.Services;

namespace AliBird.Core.Tests.Services;

// ms -> kare carpani donusumunu dogrular (K68).
[TestClass]
public sealed class KareCarpaniHesaplayiciTestleri
{
    private readonly KareCarpaniHesaplayici _hesaplayici = new(new OyunAyarlari());

    [TestMethod]
    [DataRow(0.0, 1f)]                 // ilk kare
    [DataRow(1000.0 / 60.0, 1f)]       // tam 60 FPS
    [DataRow(1000.0 / 30.0, 2f)]       // 30 FPS: bir tik 2 kare
    [DataRow(500.0, 3f)]               // takilma: ust sinir
    public void Hesapla_SureyiKareyeCevirir(double gecenMs, float beklenen)
    {
        Assert.AreEqual(beklenen, _hesaplayici.Hesapla(gecenMs), 0.0001f);
    }
}
