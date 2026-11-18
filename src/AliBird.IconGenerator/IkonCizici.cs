using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using AliBird.Core.Configuration;
using AliBird.WinForms.Rendering;

namespace AliBird.IconGenerator;

// Tek sorumlulugu: verilen boyutta ikon resmini cizmek.
// Gorunus: yuvarlak koseli gece mavisi kare + ortada oyundaki kusun AYNISI (IKusCizici).
public sealed class IkonCizici
{
    private const float KenarPayiOrani = 0.10f;   // her kenardan %10 bosluk
    private const float KoseYaricapOrani = 0.22f;

    private readonly IKusCizici _kusCizici;
    private readonly CizimKaynaklari _kaynaklar;
    private readonly OyunAyarlari _ayarlar;

    public IkonCizici(IKusCizici kusCizici, CizimKaynaklari kaynaklar, OyunAyarlari ayarlar)
    {
        ArgumentNullException.ThrowIfNull(kusCizici);
        ArgumentNullException.ThrowIfNull(kaynaklar);
        ArgumentNullException.ThrowIfNull(ayarlar);
        _kusCizici = kusCizici;
        _kaynaklar = kaynaklar;
        _ayarlar = ayarlar;
    }

    // boyut x boyut piksellik, saydam arka planli resim. Cagiran Dispose eder.
    public Bitmap Ciz(int boyut)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(boyut, 16);

        var resim = new Bitmap(boyut, boyut, PixelFormat.Format32bppArgb);
        using Graphics g = Graphics.FromImage(resim);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.Clear(Color.Transparent);

        // 1) Arka plan: yuvarlak koseli kare (oyunun gokyuzu rengi).
        using (GraphicsPath kare = YuvarlakKare(boyut, boyut * KoseYaricapOrani))
        {
            g.FillPath(_kaynaklar.Gokyuzu, kare);
        }

        // 2) Kus: oyundaki 30x24 kus olcekle buyutulur; gaga dahil genislik alana sigdirilir.
        float kusG = _ayarlar.KusGenisligi;
        float kusY = _ayarlar.KusYuksekligi;
        float toplamGenislik = kusG * (1f + KusCizici.GagaTasmaOrani);
        float olcek = boyut * (1f - 2f * KenarPayiOrani) / toplamGenislik;

        g.TranslateTransform((boyut - toplamGenislik * olcek) / 2f, (boyut - kusY * olcek) / 2f);
        g.ScaleTransform(olcek, olcek);   // kalem kalinliklari da ayni oranda olceklenir
        _kusCizici.Ciz(g, new RectangleF(0f, 0f, kusG, kusY));

        return resim;
    }

    private static GraphicsPath YuvarlakKare(int boyut, float yaricap)
    {
        float cap = yaricap * 2f;
        float son = boyut - 1f;
        var yol = new GraphicsPath();
        yol.AddArc(0f, 0f, cap, cap, 180f, 90f);
        yol.AddArc(son - cap, 0f, cap, cap, 270f, 90f);
        yol.AddArc(son - cap, son - cap, cap, cap, 0f, 90f);
        yol.AddArc(0f, son - cap, cap, cap, 90f, 90f);
        yol.CloseFigure();
        return yol;
    }
}
