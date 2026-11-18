namespace AliBird.WinForms.Rendering;

// IYaziCizici'nin uygulamasi.
// Tek sorumlulugu: metni once 2 px kaydirilmis golgeyle, sonra asil renkle yazmak.
// Koyu gece temasinda yazilar boylece her zeminde okunur.
public sealed class YaziCizici : IYaziCizici
{
    private const float GolgeKaymasi = 2f;

    private readonly CizimKaynaklari _kaynaklar;

    public YaziCizici(CizimKaynaklari kaynaklar)
    {
        ArgumentNullException.ThrowIfNull(kaynaklar);
        _kaynaklar = kaynaklar;
    }

    public void Yaz(Graphics g, string metin, Font yaziTipi, Brush firca, PointF konum, StringFormat? bicim = null)
    {
        var golgeKonumu = new PointF(konum.X + GolgeKaymasi, konum.Y + GolgeKaymasi);

        if (bicim is null)
        {
            g.DrawString(metin, yaziTipi, _kaynaklar.YaziGolge, golgeKonumu);
            g.DrawString(metin, yaziTipi, firca, konum);
        }
        else
        {
            g.DrawString(metin, yaziTipi, _kaynaklar.YaziGolge, golgeKonumu, bicim);
            g.DrawString(metin, yaziTipi, firca, konum, bicim);
        }
    }
}
