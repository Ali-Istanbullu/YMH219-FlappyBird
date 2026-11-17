namespace AliBird.WinForms.Rendering;

// ================================================================================
//  GDI+ KAYNAK PAKETI
//
//  GDI+'ta firca (Brush), kalem (Pen), yazi tipi (Font) ve metin bicimi (StringFormat)
//  isletim sistemi kaynagi kullanir; her karede yeniden olusturmak yavas olur ve
//  Dispose edilmezse bellek sizar. Bu yuzden hepsi BIR KEZ, burada olusturulur ve
//  uygulama kapanirken burada birakilir.
//
//  Tek sorumlulugu: cizim kaynaklarini Tema'dan uretmek, tutmak ve birakmak (S).
//  Hicbir sey cizmez. Katman cizicileri bu paketi Tema/OyunAyarlari gibi bir "ayar nesnesi"
//  olarak constructor'dan alir.
// ================================================================================
public sealed class CizimKaynaklari : IDisposable
{
    public CizimKaynaklari(Tema tema)
    {
        ArgumentNullException.ThrowIfNull(tema);

        // ---- Fircalar (ic doldurma) ----
        Gokyuzu = new SolidBrush(tema.Gokyuzu);
        Boru = new SolidBrush(tema.Boru);
        Zemin = new SolidBrush(tema.Zemin);
        KusGovde = new SolidBrush(tema.KusGovde);
        KusKanat = new SolidBrush(tema.KusKanat);
        Gaga = new SolidBrush(tema.Gaga);
        GozBeyaz = new SolidBrush(tema.GozBeyaz);
        GozBebek = new SolidBrush(tema.GozBebek);
        Yazi = new SolidBrush(tema.Yazi);
        YaziGolge = new SolidBrush(tema.YaziGolge);
        Vurgu = new SolidBrush(tema.Vurgu);
        SesYok = new SolidBrush(tema.SesYok);
        SesKapali = new SolidBrush(tema.SesKapali);
        Karartma = new SolidBrush(tema.Karartma);

        // ---- Kalemler (kenar cizgisi) ----
        BoruKenar = new Pen(tema.BoruKenar, 3f);
        ZeminCizgi = new Pen(tema.ZeminCizgi, 3f);
        KusKenar = new Pen(tema.KusKenar, 2f);
    }

    public SolidBrush Gokyuzu { get; }
    public SolidBrush Boru { get; }
    public SolidBrush Zemin { get; }
    public SolidBrush KusGovde { get; }
    public SolidBrush KusKanat { get; }
    public SolidBrush Gaga { get; }
    public SolidBrush GozBeyaz { get; }
    public SolidBrush GozBebek { get; }
    public SolidBrush Yazi { get; }
    public SolidBrush YaziGolge { get; }
    public SolidBrush Vurgu { get; }
    public SolidBrush SesYok { get; }
    public SolidBrush SesKapali { get; }
    public SolidBrush Karartma { get; }

    public Pen BoruKenar { get; }
    public Pen ZeminCizgi { get; }
    public Pen KusKenar { get; }

    // ---- Yazi tipleri (CK31: Segoe UI, piksel cinsinden; olcekle birlikte buyur/kuculur) ----
    public Font BuyukYazi { get; } = new("Segoe UI", 36f, FontStyle.Bold, GraphicsUnit.Pixel);
    public Font OrtaYazi { get; } = new("Segoe UI", 18f, FontStyle.Bold, GraphicsUnit.Pixel);
    public Font KucukYazi { get; } = new("Segoe UI", 12f, FontStyle.Regular, GraphicsUnit.Pixel);

    // ---- Metin bicimleri ----
    public StringFormat Ortala { get; } = new() { Alignment = StringAlignment.Center };
    public StringFormat SagaYasla { get; } = new() { Alignment = StringAlignment.Far };

    // Kaynaklari isletim sistemine geri verir.
    public void Dispose()
    {
        foreach (IDisposable kaynak in new IDisposable[]
        {
            Gokyuzu, Boru, Zemin, KusGovde, KusKanat, Gaga, GozBeyaz, GozBebek, Yazi, YaziGolge,
            Vurgu, SesYok, SesKapali, Karartma, BoruKenar, ZeminCizgi, KusKenar,
            BuyukYazi, OrtaYazi, KucukYazi, Ortala, SagaYasla
        })
        {
            try
            {
                kaynak.Dispose();
            }
            catch
            {
                // R6: kapanista bile exception disari cikmaz.
            }
        }
    }
}
