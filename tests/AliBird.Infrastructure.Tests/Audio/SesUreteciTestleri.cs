using AliBird.Core.Models;
using AliBird.Infrastructure.Audio;

namespace AliBird.Infrastructure.Tests.Audio;

// Uretilen seslerin suresini, ses seviyesini ve yumusak baslayip bittigini dogrular (K13, S1b, S2a).
[TestClass]
public sealed class SesUreteciTestleri
{
    private readonly SesUreteci _uretici = new();

    // Beklenen ornek sayilari (44100 Hz):
    //   Zipla     : 80 ms               -> 3528
    //   Puan      : 70 + 70 ms          -> 3087 + 3087 = 6174
    //   Carpma    : 150 ms              -> 6615
    //   OyunBitti : 150 sessiz + 3x150  -> 4 x 6615 = 26460
    [TestMethod]
    [DataRow(SesTuru.Zipla, 3528)]
    [DataRow(SesTuru.Puan, 6174)]
    [DataRow(SesTuru.Carpma, 6615)]
    [DataRow(SesTuru.OyunBitti, 26460)]
    public void HerSes_BeklenenUzunluktadir(SesTuru tur, int beklenenOrnek)
    {
        Assert.HasCount(beklenenOrnek, _uretici.Uret(tur));
    }

    [TestMethod]
    [DataRow(SesTuru.Zipla)]
    [DataRow(SesTuru.Puan)]
    [DataRow(SesTuru.Carpma)]
    [DataRow(SesTuru.OyunBitti)]
    public void HerSes_YuzdeOtuzSeviyesiniAsmaz(SesTuru tur)
    {
        float enYuksek = _uretici.Uret(tur).Select(o => Math.Abs(o)).Max();

        Assert.IsLessThanOrEqualTo(0.3f + 0.0001f, enYuksek);   // S2a
        Assert.IsGreaterThan(0.1f, enYuksek);                   // ama gercekten ses var
    }

    [TestMethod]
    [DataRow(SesTuru.Zipla)]
    [DataRow(SesTuru.Puan)]
    [DataRow(SesTuru.Carpma)]
    [DataRow(SesTuru.OyunBitti)]
    public void HerSes_SessizBaslarVeSessizBiter(SesTuru tur)
    {
        float[] ses = _uretici.Uret(tur);

        // Yumusak gecis: ilk ve son ornek 0 -> "cit" sesi yok.
        Assert.AreEqual(0f, ses[0], 0.0001f);
        Assert.AreEqual(0f, ses[^1], 0.0001f);
    }

    [TestMethod]
    public void OyunBittiSesi_CarpmaBitsinDiye150msSessizBaslar()
    {
        float[] ses = _uretici.Uret(SesTuru.OyunBitti);

        Assert.IsTrue(ses.Take(6615).All(o => o == 0f));
    }
}
