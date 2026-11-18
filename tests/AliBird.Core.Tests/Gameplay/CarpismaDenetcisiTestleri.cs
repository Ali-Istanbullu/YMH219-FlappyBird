using System.Drawing;
using AliBird.Core.Configuration;
using AliBird.Core.Gameplay;

namespace AliBird.Core.Tests.Gameplay;

// Zemin ve engel carpismalarini dogrular.
[TestClass]
public sealed class CarpismaDenetcisiTestleri
{
    private readonly OyunAyarlari _ayarlar = new();
    private CarpismaDenetcisi Denetci => new(_ayarlar);

    // Engel x 100-160, bosluk y 100-250.
    private Engel OrnekEngel() => new(_ayarlar, 100f, 100f);

    [TestMethod]
    public void BoslugunIcindekiKus_Carpmaz()
    {
        var kutu = new RectangleF(110f, 150f, 24f, 18f);   // bosluk icinde

        Assert.IsFalse(Denetci.CarptiMi(kutu, [OrnekEngel()]));
    }

    [TestMethod]
    public void UstBoruyaGirenKus_Carpar()
    {
        var kutu = new RectangleF(110f, 90f, 24f, 18f);    // ust kenari 90 < 100

        Assert.IsTrue(Denetci.CarptiMi(kutu, [OrnekEngel()]));
    }

    [TestMethod]
    public void AltBoruyaGirenKus_Carpar()
    {
        var kutu = new RectangleF(110f, 240f, 24f, 18f);   // alt kenari 258 > 250

        Assert.IsTrue(Denetci.CarptiMi(kutu, [OrnekEngel()]));
    }

    [TestMethod]
    public void ZemineUlasanKus_Carpar()
    {
        var kutu = new RectangleF(10f, 422f, 24f, 18f);    // alt kenari tam 440

        Assert.IsTrue(Denetci.CarptiMi(kutu, []));
    }

    [TestMethod]
    public void ZemininUstundekiKus_Carpmaz()
    {
        var kutu = new RectangleF(10f, 421f, 24f, 18f);    // alt kenari 439

        Assert.IsFalse(Denetci.CarptiMi(kutu, []));
    }

    [TestMethod]
    public void KucukKutu_GorselKoseDegseBileCarpmaz()
    {
        // kus cizimde boruya 2 px degiyor ama carpisma kutusu 3 px iceride.
        var ayarlar = new OyunAyarlari { KusBaslangicY = 100f - 3f + 1f, KusX = 110f }; // cizim ust kenari 98
        var kus = new Kus(ayarlar);

        Assert.IsFalse(new CarpismaDenetcisi(ayarlar).CarptiMi(kus.CarpismaKutusu, [new Engel(ayarlar, 100f, 100f)]));
    }
}
