using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using AliBird.Core.Configuration;
using AliBird.Core.Models;

namespace AliBird.WinForms.Rendering;

// ================================================================================
//  GDI+ CIZIM KATMANI (K1/K4: cizim tamamen GDI+ ile, harici resim YOK)
//
//  GDI+ nedir? Windows'un 2B cizim kutuphanesi. .NET'te System.Drawing ile kullanilir.
//  Temel parcalar:
//    Graphics : "tuval". Form her yeniden cizildiginde (OnPaint) bize bir tane verilir.
//    Brush    : "firca". Bir sekli icini doldurmak icin (FillRectangle, FillEllipse).
//    Pen      : "kalem". Bir seklin kenar cizgisini cizmek icin (DrawRectangle).
//    Font     : yazi tipi. DrawString ile metin yazilir.
//  Firca, kalem ve yazi tipi isletim sistemi kaynagi kullanir; ise yaramayinca Dispose
//  edilmeleri gerekir. Bu yuzden hepsini BIR KEZ olusturup sinif kapanirken birakiyoruz.
//
//  Olcekleme: Tum cizimi 640x480'lik sanal alanda yapariz. Cizmeden once Graphics'e
//  "su kadar kaydir, su kadar buyut" deriz (TranslateTransform + ScaleTransform);
//  boylece pencere ne boyutta olursa olsun ayni kod dogru yere cizer (K5, K7).
//
//  Tek sorumlulugu: OyunGoruntusu'nu ekrana cizmek (S). Oyunla ilgili hicbir karar vermez.
// ================================================================================
public sealed class OyunCizici : IDisposable
{
    // ---- Ekrandaki metinler (Adim 8, G2: Ali onayladi) ----
    private const string BaslikMetni = "AliBird";
    private const string BaslaMetni = "Başlamak için SPACE'e bas";
    private const string DuraklatildiMetni = "Duraklatıldı";
    private const string DevamMetni = "Devam etmek için P'ye bas";
    private const string OyunBittiMetni = "Oyun Bitti";
    private const string YenidenBaslaMetni = "Yeniden başlamak için SPACE";
    private const string TuslarMetni = "SPACE: Zıpla   P: Duraklat   M: Ses";
    private const string YeniRekorMetni = "YENİ REKOR!";
    private const string SesYokMetni = "Ses yok";
    private const string SesKapaliMetni = "Ses kapalı (M)";

    // Rekor tarihi gg.aa.yyyy (K10).
    private const string TarihBicimi = "dd.MM.yyyy";
    private static readonly CultureInfo Turkce = CultureInfo.GetCultureInfo("tr-TR");

    // Oyun Bitti ekranindaki "Yeniden Baslat" butonunun SANAL alandaki yeri.
    // Form gercek butonu bu dikdortgene gore yerlestirir (CK32).
    public static readonly RectangleF YenidenBaslatButonAlani = new(245f, 270f, 150f, 40f);

    private readonly OyunAyarlari _ayarlar;
    private readonly OlcekHesaplayici _olcekHesaplayici;
    private readonly SizeF _alanBoyutu;

    // ---- GDI+ kaynaklari: bir kez olusturulur, Dispose'da birakilir ----
    private readonly SolidBrush _gokyuzuFircasi, _boruFircasi, _zeminFircasi, _kusFircasi, _kanatFircasi,
        _gagaFircasi, _gozBeyazFircasi, _gozBebekFircasi, _yaziFircasi, _golgeFircasi, _vurguFircasi,
        _sesYokFircasi, _sesKapaliFircasi, _karartmaFircasi;
    private readonly Pen _boruKalemi, _zeminKalemi, _kusKalemi;

    // CK31: Segoe UI. Boyutlar piksel cinsinden; olcekleme donusumu yaziyi da buyutup kucultur.
    private readonly Font _buyukYazi = new("Segoe UI", 36f, FontStyle.Bold, GraphicsUnit.Pixel);
    private readonly Font _ortaYazi = new("Segoe UI", 18f, FontStyle.Bold, GraphicsUnit.Pixel);
    private readonly Font _kucukYazi = new("Segoe UI", 12f, FontStyle.Regular, GraphicsUnit.Pixel);

    // Metni yatayda ortalamak icin.
    private readonly StringFormat _ortala = new() { Alignment = StringAlignment.Center };
    private readonly StringFormat _sagaYasla = new() { Alignment = StringAlignment.Far };

    public OyunCizici(Tema tema, OyunAyarlari ayarlar, OlcekHesaplayici olcekHesaplayici)
    {
        ArgumentNullException.ThrowIfNull(tema);
        ArgumentNullException.ThrowIfNull(ayarlar);
        ArgumentNullException.ThrowIfNull(olcekHesaplayici);

        KenarRengi = tema.KenarBosluk;
        _ayarlar = ayarlar;
        _olcekHesaplayici = olcekHesaplayici;
        _alanBoyutu = new SizeF(ayarlar.AlanGenisligi, ayarlar.AlanYuksekligi);

        _gokyuzuFircasi = new SolidBrush(tema.Gokyuzu);
        _boruFircasi = new SolidBrush(tema.Boru);
        _zeminFircasi = new SolidBrush(tema.Zemin);
        _kusFircasi = new SolidBrush(tema.KusGovde);
        _kanatFircasi = new SolidBrush(tema.KusKanat);
        _gagaFircasi = new SolidBrush(tema.Gaga);
        _gozBeyazFircasi = new SolidBrush(tema.GozBeyaz);
        _gozBebekFircasi = new SolidBrush(tema.GozBebek);
        _yaziFircasi = new SolidBrush(tema.Yazi);
        _golgeFircasi = new SolidBrush(tema.YaziGolge);
        _vurguFircasi = new SolidBrush(tema.Vurgu);
        _sesYokFircasi = new SolidBrush(tema.SesYok);
        _sesKapaliFircasi = new SolidBrush(tema.SesKapali);
        _karartmaFircasi = new SolidBrush(tema.Karartma);

        _boruKalemi = new Pen(tema.BoruKenar, 3f);
        _zeminKalemi = new Pen(tema.ZeminCizgi, 3f);
        _kusKalemi = new Pen(tema.KusKenar, 2f);
    }

    // Form arka plani icin (kenar bosluklari).
    public Color KenarRengi { get; }

    // Form her yeniden cizildiginde bunu cagirir. goruntu null ise (oyun henuz baslamadi) sadece zemin cizilir.
    public void Ciz(Graphics g, OyunGoruntusu? goruntu, Size pencereBoyutu)
    {
        g.Clear(KenarRengi);
        if (pencereBoyutu.Width <= 0 || pencereBoyutu.Height <= 0)
        {
            return;   // pencere simge durumunda: cizilecek yer yok
        }

        OlcekBilgisi olcek = _olcekHesaplayici.Hesapla(pencereBoyutu, _alanBoyutu);

        // Graphics'in mevcut ayarlarini sakla; cizim bitince geri yukle.
        GraphicsState onceki = g.Save();
        try
        {
            // 1) Sanal alani pencereye tasi ve buyut (K5).
            g.TranslateTransform(olcek.KaydirmaX, olcek.KaydirmaY);
            g.ScaleTransform(olcek.Olcek, olcek.Olcek);

            // 2) Disari tasan cizimleri kes (borular kenar bosluguna tasmasin).
            g.SetClip(new RectangleF(PointF.Empty, _alanBoyutu));

            // 3) Kenar yumusatma: egri ve yazilar tirtikli gorunmesin.
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            // 4) Arkadan one dogru ciz: gokyuzu -> borular -> zemin -> kus -> yazilar -> katman.
            g.FillRectangle(_gokyuzuFircasi, 0f, 0f, _alanBoyutu.Width, _alanBoyutu.Height);
            if (goruntu is null)
            {
                return;
            }

            foreach (EngelGoruntusu engel in goruntu.Engeller)
            {
                EngeliCiz(g, engel);
            }
            ZeminiCiz(g);
            KusuCiz(g, goruntu.KusAlani);
            UstBilgiyiCiz(g, goruntu);
            AltBilgiyiCiz(g, goruntu.SesDurumu);
            DurumKatmaniniCiz(g, goruntu);
        }
        finally
        {
            g.Restore(onceki);
        }
    }

    // ================= PARCALAR =================

    // Boru: dolgu + kenar cizgisi + bosluga bakan uca biraz daha genis bir "agiz".
    private void EngeliCiz(Graphics g, EngelGoruntusu engel)
    {
        const float agizYuksekligi = 14f;
        const float agizTasmasi = 4f;   // agiz borudan her yana 4 px genis

        BoruParcasiCiz(g, engel.UstAlan);
        BoruParcasiCiz(g, engel.AltAlan);

        // Ust borunun agzi altta, alt borunun agzi ustte (boslugun iki kenari).
        var ustAgiz = new RectangleF(engel.UstAlan.X - agizTasmasi, engel.UstAlan.Bottom - agizYuksekligi,
            engel.UstAlan.Width + 2 * agizTasmasi, agizYuksekligi);
        var altAgiz = new RectangleF(engel.AltAlan.X - agizTasmasi, engel.AltAlan.Y,
            engel.AltAlan.Width + 2 * agizTasmasi, agizYuksekligi);
        BoruParcasiCiz(g, ustAgiz);
        BoruParcasiCiz(g, altAgiz);
    }

    private void BoruParcasiCiz(Graphics g, RectangleF alan)
    {
        if (alan.Width <= 0 || alan.Height <= 0)
        {
            return;
        }
        g.FillRectangle(_boruFircasi, alan);
        g.DrawRectangle(_boruKalemi, alan.X, alan.Y, alan.Width, alan.Height);
    }

    // Zemin: alttaki 40 px'lik serit ve ust kenarinda bir cizgi (K6).
    private void ZeminiCiz(Graphics g)
    {
        float y = _ayarlar.ZeminUstY;
        g.FillRectangle(_zeminFircasi, 0f, y, _alanBoyutu.Width, _ayarlar.ZeminYuksekligi);
        g.DrawLine(_zeminKalemi, 0f, y, _alanBoyutu.Width, y);
    }

    // Kus: elips govde + kanat + goz + ucgen gaga. Olculer govdeye oranla verilir.
    private void KusuCiz(Graphics g, RectangleF k)
    {
        // Govde
        g.FillEllipse(_kusFircasi, k);
        g.DrawEllipse(_kusKalemi, k);

        // Kanat: govdenin sol-ortasinda kucuk bir elips
        g.FillEllipse(_kanatFircasi, k.X + k.Width * 0.08f, k.Y + k.Height * 0.45f, k.Width * 0.45f, k.Height * 0.35f);

        // Goz: sag-ust tarafta beyaz daire ve icinde siyah bebek
        float gozCapi = k.Height * 0.42f;
        float gozX = k.X + k.Width * 0.58f;
        float gozY = k.Y + k.Height * 0.12f;
        g.FillEllipse(_gozBeyazFircasi, gozX, gozY, gozCapi, gozCapi);
        g.FillEllipse(_gozBebekFircasi, gozX + gozCapi * 0.45f, gozY + gozCapi * 0.3f, gozCapi * 0.4f, gozCapi * 0.4f);

        // Gaga: govdenin sagindan disari tasan ucgen
        PointF[] gaga =
        [
            new(k.Right - k.Width * 0.08f, k.Y + k.Height * 0.45f),
            new(k.Right + k.Width * 0.28f, k.Y + k.Height * 0.60f),
            new(k.Right - k.Width * 0.08f, k.Y + k.Height * 0.78f)
        ];
        g.FillPolygon(_gagaFircasi, gaga);
    }

    // Sol ust: puan ve rekor (K12, K10, CK35).
    private void UstBilgiyiCiz(Graphics g, OyunGoruntusu goruntu)
    {
        GolgeliYaz(g, $"Puan: {goruntu.Puan}", _ortaYazi, _yaziFircasi, new PointF(12f, 10f), null);

        if (goruntu.YeniRekorMu)
        {
            // CK35: yeni rekor kirildiysa rekor satiri guncel puani gosterir + turuncu "YENI REKOR!"
            string rekorMetni = $"Rekor: {goruntu.Puan}  ";
            GolgeliYaz(g, rekorMetni, _kucukYazi, _yaziFircasi, new PointF(12f, 38f), null);
            float genislik = g.MeasureString(rekorMetni, _kucukYazi).Width;
            GolgeliYaz(g, YeniRekorMetni, _kucukYazi, _vurguFircasi, new PointF(12f + genislik, 38f), null);
            return;
        }

        string metin = goruntu.Rekor is null
            ? "Rekor: -"
            : $"Rekor: {goruntu.Rekor.Puan} ({goruntu.Rekor.Tarih.ToString(TarihBicimi, Turkce)})";
        GolgeliYaz(g, metin, _kucukYazi, _yaziFircasi, new PointF(12f, 38f), null);
    }

    // Sol alt: tuslar (hep gorunur, K11). Sag alt: ses durumu (K12).
    private void AltBilgiyiCiz(Graphics g, SesDurumu sesDurumu)
    {
        float y = _ayarlar.ZeminUstY + 12f;
        GolgeliYaz(g, TuslarMetni, _kucukYazi, _yaziFircasi, new PointF(12f, y), null);

        if (sesDurumu == SesDurumu.Yok)
        {
            GolgeliYaz(g, SesYokMetni, _kucukYazi, _sesYokFircasi, new PointF(_alanBoyutu.Width - 12f, y), _sagaYasla);
        }
        else if (sesDurumu == SesDurumu.Kapali)
        {
            GolgeliYaz(g, SesKapaliMetni, _kucukYazi, _sesKapaliFircasi, new PointF(_alanBoyutu.Width - 12f, y), _sagaYasla);
        }
    }

    // Oyunun durumuna gore ortadaki yazilar (G2).
    private void DurumKatmaniniCiz(Graphics g, OyunGoruntusu goruntu)
    {
        float orta = _alanBoyutu.Width / 2f;

        switch (goruntu.Durum)
        {
            case OyunDurumu.Hazir:
                GolgeliYaz(g, BaslikMetni, _buyukYazi, _yaziFircasi, new PointF(orta, 120f), _ortala);
                GolgeliYaz(g, BaslaMetni, _ortaYazi, _yaziFircasi, new PointF(orta, 185f), _ortala);
                break;

            case OyunDurumu.Duraklatildi:
                g.FillRectangle(_karartmaFircasi, 0f, 0f, _alanBoyutu.Width, _alanBoyutu.Height);
                GolgeliYaz(g, DuraklatildiMetni, _buyukYazi, _yaziFircasi, new PointF(orta, 165f), _ortala);
                GolgeliYaz(g, DevamMetni, _ortaYazi, _yaziFircasi, new PointF(orta, 230f), _ortala);
                break;

            case OyunDurumu.OyunBitti:
                g.FillRectangle(_karartmaFircasi, 0f, 0f, _alanBoyutu.Width, _alanBoyutu.Height);
                GolgeliYaz(g, OyunBittiMetni, _buyukYazi, _yaziFircasi, new PointF(orta, 115f), _ortala);
                GolgeliYaz(g, $"Puan: {goruntu.Puan}", _ortaYazi, _yaziFircasi, new PointF(orta, 180f), _ortala);
                GolgeliYaz(g, YenidenBaslaMetni, _kucukYazi, _yaziFircasi, new PointF(orta, 225f), _ortala);
                // "Yeniden Baslat" butonu gercek bir WinForms butonu; Form onu YenidenBaslatButonAlani'na koyar.
                break;
        }
    }

    // Metni once 2 px kaydirilmis golgeyle, sonra asil renkle yazar (koyu zeminde okunurluk icin).
    private void GolgeliYaz(Graphics g, string metin, Font yaziTipi, Brush firca, PointF konum, StringFormat? bicim)
    {
        var golgeKonumu = new PointF(konum.X + 2f, konum.Y + 2f);
        if (bicim is null)
        {
            g.DrawString(metin, yaziTipi, _golgeFircasi, golgeKonumu);
            g.DrawString(metin, yaziTipi, firca, konum);
        }
        else
        {
            g.DrawString(metin, yaziTipi, _golgeFircasi, golgeKonumu, bicim);
            g.DrawString(metin, yaziTipi, firca, konum, bicim);
        }
    }

    // GDI+ kaynaklarini isletim sistemine geri verir.
    public void Dispose()
    {
        foreach (IDisposable kaynak in new IDisposable[]
        {
            _gokyuzuFircasi, _boruFircasi, _zeminFircasi, _kusFircasi, _kanatFircasi, _gagaFircasi,
            _gozBeyazFircasi, _gozBebekFircasi, _yaziFircasi, _golgeFircasi, _vurguFircasi, _sesYokFircasi,
            _sesKapaliFircasi, _karartmaFircasi, _boruKalemi, _zeminKalemi, _kusKalemi,
            _buyukYazi, _ortaYazi, _kucukYazi, _ortala, _sagaYasla
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
