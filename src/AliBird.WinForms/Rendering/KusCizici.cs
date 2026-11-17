namespace AliBird.WinForms.Rendering;

// Kus: elips govde + kanat + goz + ucgen gaga. Olculer govdeye oranla verilir (K4: tamamen GDI+).
// Tek sorumlulugu: kusu cizmek (S). SahneCizici'den Adim 10'da ayrildi (CK55).
public sealed class KusCizici : IKusCizici
{
    // Gaganin govdenin sagindan disari tasma orani (ikon yerlesimi de bunu kullanir).
    public const float GagaTasmaOrani = 0.28f;

    private readonly CizimKaynaklari _kaynaklar;

    public KusCizici(CizimKaynaklari kaynaklar)
    {
        ArgumentNullException.ThrowIfNull(kaynaklar);
        _kaynaklar = kaynaklar;
    }

    public void Ciz(Graphics g, RectangleF k)
    {
        // Govde
        g.FillEllipse(_kaynaklar.KusGovde, k);
        g.DrawEllipse(_kaynaklar.KusKenar, k);

        // Kanat: govdenin sol-ortasinda kucuk bir elips
        g.FillEllipse(_kaynaklar.KusKanat, k.X + k.Width * 0.08f, k.Y + k.Height * 0.45f, k.Width * 0.45f, k.Height * 0.35f);

        // Goz: sag-ust tarafta beyaz daire ve icinde siyah bebek
        float gozCapi = k.Height * 0.42f;
        float gozX = k.X + k.Width * 0.58f;
        float gozY = k.Y + k.Height * 0.12f;
        g.FillEllipse(_kaynaklar.GozBeyaz, gozX, gozY, gozCapi, gozCapi);
        g.FillEllipse(_kaynaklar.GozBebek, gozX + gozCapi * 0.45f, gozY + gozCapi * 0.3f, gozCapi * 0.4f, gozCapi * 0.4f);

        // Gaga: govdenin sagindan disari tasan ucgen
        PointF[] gaga =
        [
            new(k.Right - k.Width * 0.08f, k.Y + k.Height * 0.45f),
            new(k.Right + k.Width * GagaTasmaOrani, k.Y + k.Height * 0.60f),
            new(k.Right - k.Width * 0.08f, k.Y + k.Height * 0.78f)
        ];
        g.FillPolygon(_kaynaklar.Gaga, gaga);
    }
}
