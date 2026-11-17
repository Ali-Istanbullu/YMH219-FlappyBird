using System.Drawing.Drawing2D;
using System.Drawing.Text;
using AliBird.Core.Configuration;
using AliBird.Core.Models;

namespace AliBird.WinForms.Rendering;

// ================================================================================
//  GDI+ CIZIM KATMANI (K1/K4: cizim tamamen GDI+ ile, harici resim YOK)
//
//  GDI+ nedir? Windows'un 2B cizim kutuphanesi. .NET'te System.Drawing ile kullanilir.
//  Temel parcalar:
//    Graphics : "tuval". Form her yeniden cizildiginde (OnPaint) bize bir tane verilir.
//    Brush    : "firca". Bir seklin icini doldurur (FillRectangle, FillEllipse).
//    Pen      : "kalem". Bir seklin kenar cizgisini cizer (DrawRectangle).
//    Font     : yazi tipi. DrawString ile metin yazilir.
//  Bunlar CizimKaynaklari'nda bir kez olusturulur ve kapanista birakilir.
//
//  Olcekleme: Tum cizim 640x480'lik SANAL alanda yapilir. Cizmeden once Graphics'e
//  "su kadar kaydir, su kadar buyut" denir (TranslateTransform + ScaleTransform);
//  boylece pencere ne boyutta olursa olsun ayni kod dogru yere cizer (K5, K7).
//
//  Katmanlar (K65, Adim 8d'de bolundu):
//    1) SahneCizici         : gokyuzu, borular, zemin, kus
//    2) BilgiCizici         : puan/rekor, tus aciklamalari, ses durumu
//    3) DurumKatmaniCizici  : baslangic / duraklatma / oyun bitti ekrani
//
//  Bu sinifin tek sorumlulugu (S): tuvali hazirlamak (temizle, olcekle, kes, yumusat)
//  ve katmanlari SIRAYLA cagirmak. Kendisi hicbir sekil cizmez.
// ================================================================================
public sealed class OyunCizici : IOyunCizici
{
    private readonly IOlcekHesaplayici _olcekHesaplayici;
    private readonly IReadOnlyList<IKatmanCizici> _katmanlar;
    private readonly SizeF _alanBoyutu;
    private readonly Color _kenarRengi;
    private readonly SolidBrush _gokyuzuFircasi;

    // katmanlar: Program.cs'te kayit sirasiyla gelir (sahne, bilgi, durum).
    public OyunCizici(IOlcekHesaplayici olcekHesaplayici, IEnumerable<IKatmanCizici> katmanlar,
        CizimKaynaklari kaynaklar, Tema tema, OyunAyarlari ayarlar)
    {
        ArgumentNullException.ThrowIfNull(olcekHesaplayici);
        ArgumentNullException.ThrowIfNull(katmanlar);
        ArgumentNullException.ThrowIfNull(kaynaklar);
        ArgumentNullException.ThrowIfNull(tema);
        ArgumentNullException.ThrowIfNull(ayarlar);

        _olcekHesaplayici = olcekHesaplayici;
        _katmanlar = katmanlar.ToList();
        _alanBoyutu = new SizeF(ayarlar.AlanGenisligi, ayarlar.AlanYuksekligi);
        _kenarRengi = tema.KenarBosluk;
        _gokyuzuFircasi = kaynaklar.Gokyuzu;
    }

    public void Ciz(Graphics g, OyunGoruntusu? goruntu, Size pencereBoyutu)
    {
        // Pencerenin tamami once kenar rengiyle doldurulur (oran korundugu icin kalan bosluklar).
        g.Clear(_kenarRengi);
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

            // 4) Gokyuzu her zaman cizilir; oyun hazir degilse katmanlar cizilmez.
            g.FillRectangle(_gokyuzuFircasi, 0f, 0f, _alanBoyutu.Width, _alanBoyutu.Height);
            if (goruntu is null)
            {
                return;
            }

            // 5) Katmanlar arkadan one: sahne -> bilgi -> durum.
            foreach (IKatmanCizici katman in _katmanlar)
            {
                katman.Ciz(g, goruntu);
            }
        }
        finally
        {
            g.Restore(onceki);
        }
    }
}
