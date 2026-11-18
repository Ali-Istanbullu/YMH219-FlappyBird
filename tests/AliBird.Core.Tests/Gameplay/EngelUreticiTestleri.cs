using AliBird.Core.Configuration;
using AliBird.Core.Gameplay;

namespace AliBird.Core.Tests.Gameplay;

// Engel ureticisinin istenen konumda, ayarlara uygun engel verdigini dogrular.
[TestClass]
public sealed class EngelUreticiTestleri
{
    [TestMethod]
    public void Olustur_IstenenKonumdaYeniEngelVerir()
    {
        var uretici = new EngelUretici(new OyunAyarlari());

        Engel engel = uretici.Olustur(x: 500f, boslukUstY: 120f);

        Assert.AreEqual(500f, engel.X);
        Assert.AreEqual(120f, engel.BoslukUstY);
        Assert.AreEqual(60f, engel.UstAlan.Width);   // ayarlardan
        Assert.IsFalse(engel.GecildiMi);
    }

    [TestMethod]
    public void Olustur_HerCagridaFarkliNesneVerir()
    {
        var uretici = new EngelUretici(new OyunAyarlari());

        Assert.AreNotSame(uretici.Olustur(0f, 100f), uretici.Olustur(0f, 100f));
    }
}
