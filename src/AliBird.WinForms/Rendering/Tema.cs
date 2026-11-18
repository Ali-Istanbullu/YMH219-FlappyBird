namespace AliBird.WinForms.Rendering;

// Oyunun renk paleti: "Gece" temasi.
// Butun renkler tek yerde; baska bir tema istenirse sadece burasi degisir (OCP).
// "record": olusturulduktan sonra degistirilemez; DI'a tek ornek olarak kaydedilir.
public sealed record Tema
{
    // Pencere oyun alanindan genisse kalan kenarlar (oran korundugu icin).
    public required Color KenarBosluk { get; init; }
    public required Color Gokyuzu { get; init; }

    public required Color Boru { get; init; }
    public required Color BoruKenar { get; init; }

    public required Color Zemin { get; init; }
    public required Color ZeminCizgi { get; init; }

    public required Color KusGovde { get; init; }
    public required Color KusKenar { get; init; }
    public required Color KusKanat { get; init; }
    public required Color Gaga { get; init; }
    public required Color GozBeyaz { get; init; }
    public required Color GozBebek { get; init; }

    public required Color Yazi { get; init; }
    public required Color YaziGolge { get; init; }
    public required Color Vurgu { get; init; }        // "YENI REKOR!" ve buton
    public required Color SesYok { get; init; }       // kirmizi
    public required Color SesKapali { get; init; }    // gri
    public required Color Karartma { get; init; }     // duraklatma / oyun bitti ekraninda yari saydam ortu

    // Gece temasi - lacivert gokyuzu, gri-mor borular, acik sari kus, koyu gri zemin.
    public static Tema Gece() => new()
    {
        KenarBosluk = Color.FromArgb(11, 15, 31),
        Gokyuzu = Color.FromArgb(27, 36, 64),
        Boru = Color.FromArgb(107, 101, 147),
        BoruKenar = Color.FromArgb(70, 64, 107),
        Zemin = Color.FromArgb(46, 46, 54),
        ZeminCizgi = Color.FromArgb(61, 61, 72),
        KusGovde = Color.FromArgb(246, 226, 127),
        KusKenar = Color.FromArgb(201, 169, 58),
        KusKanat = Color.FromArgb(230, 200, 90),
        Gaga = Color.FromArgb(242, 155, 56),
        GozBeyaz = Color.White,
        GozBebek = Color.FromArgb(27, 27, 27),
        Yazi = Color.White,
        YaziGolge = Color.FromArgb(160, 0, 0, 0),
        Vurgu = Color.FromArgb(255, 159, 28),
        SesYok = Color.FromArgb(255, 92, 92),
        SesKapali = Color.FromArgb(154, 154, 168),
        Karartma = Color.FromArgb(140, 0, 0, 0)
    };
}
