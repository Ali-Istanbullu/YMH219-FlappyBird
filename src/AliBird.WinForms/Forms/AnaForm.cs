using AliBird.Core.Abstractions;
using AliBird.Core.Configuration;
using AliBird.Core.Models;
using AliBird.WinForms.Rendering;

namespace AliBird.WinForms.Forms;

// Oyunun penceresi. MVP'deki PASIF View: IOyunGorunumu'nu uygular.
// Gorevleri sadece:
//   1) Ham tuslari ve olaylari presenter'a haber vermek: Space, P, M, buton, odak kaybi
//   2) Presenter'in verdigi OyunGoruntusu'nu cizdirmek (cizimi OyunCizici yapar)
// Oyunla ilgili HICBIR karar vermez; Space'in ne anlama geldigini bilmez.
// Bagimliliklarini constructor'dan alir (DIP). Designer dosyasi yok; arayuz kodla kurulur.
public sealed class AnaForm : Form, IOyunGorunumu
{
    // pencere 800x600 acilir, en kucuk 480x360 (ikisi de 4:3).
    private static readonly Size BaslangicBoyutu = new(800, 600);
    private static readonly Size EnKucukBoyut = new(480, 360);

    private readonly IOyunCizici _cizici;                 // arayuz uzerinden
    private readonly IOlcekHesaplayici _olcekHesaplayici; // arayuz uzerinden
    private readonly SizeF _alanBoyutu;

    private readonly OdakAlmayanButon _yenidenBaslatButonu;
    private Font? _butonYaziTipi;

    // Presenter'in en son gonderdigi goruntu; OnPaint bunu cizer.
    private OyunGoruntusu? _sonGoruntu;

    // basili tutulan tus tekrar tekrar olay uretmesin diye o an basili olan tuslar.
    private readonly HashSet<Keys> _basiliTuslar = [];

    public AnaForm(IOyunCizici cizici, IOlcekHesaplayici olcekHesaplayici, IHataBildirici hataBildirici, OyunAyarlari ayarlar, Tema tema)
    {
        ArgumentNullException.ThrowIfNull(cizici);
        ArgumentNullException.ThrowIfNull(olcekHesaplayici);
        ArgumentNullException.ThrowIfNull(hataBildirici);
        ArgumentNullException.ThrowIfNull(ayarlar);
        ArgumentNullException.ThrowIfNull(tema);
        _cizici = cizici;
        _olcekHesaplayici = olcekHesaplayici;
        _alanBoyutu = new SizeF(ayarlar.AlanGenisligi, ayarlar.AlanYuksekligi);

        // ---- Pencere ayarlari ----
        Text = "AliBird";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = BaslangicBoyutu;
        MinimumSize = SizeFromClientSize(EnKucukBoyut);
        BackColor = tema.KenarBosluk;
        KeyPreview = true;                                  // tuslar once forma gelsin
        PencereIkonunuYukle(hataBildirici);

        // cift tamponlama -> cizim sirasinda titreme olmaz.
        DoubleBuffered = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

        // ---- Yeniden Baslat butonu ----
        // Arayuz kontrolleri formun kendi parcasidir; designer'in yaptigi gibi burada olusturulur.
        _yenidenBaslatButonu = new OdakAlmayanButon
        {
            Text = "Yeniden Başlat",
            Visible = false,
            FlatStyle = FlatStyle.Flat,
            BackColor = tema.Vurgu,      // renkler tek yerde: Tema
            ForeColor = tema.Yazi,
            Cursor = Cursors.Hand
        };
        _yenidenBaslatButonu.FlatAppearance.BorderSize = 0;
        _yenidenBaslatButonu.Click += (_, _) => OlayiTetikle(YenidenBaslatTiklandi);
        Controls.Add(_yenidenBaslatButonu);

        // ---- Olaylar ----
        KeyDown += TusaBasildiginda;
        KeyUp += TusBirakildiginda;
        Deactivate += (_, _) => OlayiTetikle(OdakKaybedildi);   // baska pencereye gecildi
        Resize += BoyutDegistiginde;
    }

    // Pencerenin sol ust ikonu = exe'ye gomulu alibird.ico. Okunamazsa varsayilan ikon kalir.
    private void PencereIkonunuYukle(IHataBildirici hataBildirici)
    {
        try
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? Icon;
        }
        catch (Exception hata) when (hata is IOException or ArgumentException or UnauthorizedAccessException)
        {
            hataBildirici.Bildir(hata, Gorunurluk.SadeceGunluk, string.Empty);
        }
    }

    // ================= IOyunGorunumu =================
    public event EventHandler? BoslukBasildi;
    public event EventHandler? PBasildi;
    public event EventHandler? MBasildi;
    public event EventHandler? YenidenBaslatTiklandi;
    public event EventHandler? OdakKaybedildi;

    public void Goster(OyunGoruntusu goruntu)
    {
        if (IsDisposed)
        {
            return;   // pencere kapanirken gelen son kareyi yok say
        }

        _sonGoruntu = goruntu;
        ButonuGuncelle();
        Invalidate();   // Windows'a "yeniden ciz" de; OnPaint calisacak
    }

    // ================= TUSLAR =================

    private void TusaBasildiginda(object? gonderen, KeyEventArgs e)
    {
        if (e.KeyCode is not (Keys.Space or Keys.P or Keys.M))
        {
            return;
        }

        // Bu tusu biz isliyoruz; Windows baska bir seye (ornegin "ding" sesi) kullanmasin.
        e.Handled = true;
        e.SuppressKeyPress = true;

        // tus basili tutulursa Windows KeyDown'u tekrar tekrar gonderir; sadece ilkini al.
        if (!_basiliTuslar.Add(e.KeyCode))
        {
            return;
        }

        switch (e.KeyCode)
        {
            case Keys.Space: OlayiTetikle(BoslukBasildi); break;
            case Keys.P: OlayiTetikle(PBasildi); break;
            case Keys.M: OlayiTetikle(MBasildi); break;
        }
    }

    private void TusBirakildiginda(object? gonderen, KeyEventArgs e)
    {
        _basiliTuslar.Remove(e.KeyCode);   // tus birakildi: bir sonraki basis yeniden sayilir
    }

    // ================= PENCERE =================

    private void BoyutDegistiginde(object? gonderen, EventArgs e)
    {
        // pencere simge durumuna kucultulunce oyun duraklar.
        if (WindowState == FormWindowState.Minimized)
        {
            OlayiTetikle(OdakKaybedildi);
            return;
        }
        ButonuGuncelle();
    }

    // Butonu sadece Oyun Bitti'de gosterir ve pencere boyutuna gore yerlestirir.
    private void ButonuGuncelle()
    {
        bool gorunsun = _sonGoruntu?.Durum == OyunDurumu.OyunBitti;
        if (gorunsun)
        {
            OlcekBilgisi olcek = _olcekHesaplayici.Hesapla(ClientSize, _alanBoyutu);
            Rectangle alan = _olcekHesaplayici.SanaldanEkrana(EkranYerlesimi.YenidenBaslatButonAlani, olcek);
            if (_yenidenBaslatButonu.Bounds != alan)
            {
                _yenidenBaslatButonu.Bounds = alan;
                ButonYaziTipiniAyarla(olcek.Olcek);
            }
        }
        if (_yenidenBaslatButonu.Visible != gorunsun)
        {
            _yenidenBaslatButonu.Visible = gorunsun;
        }
    }

    // Buton yazisi da pencereyle birlikte buyuyup kuculsun.
    private void ButonYaziTipiniAyarla(float olcek)
    {
        Font? eski = _butonYaziTipi;
        _butonYaziTipi = new Font("Segoe UI", Math.Max(8f, 15f * olcek), FontStyle.Bold, GraphicsUnit.Pixel);
        _yenidenBaslatButonu.Font = _butonYaziTipi;
        eski?.Dispose();
    }

    // ================= CIZIM =================

    // Cizimde hata olursa yakalanmaz: o bir bug'dir (Release'te genel yakalayici loglar).
    protected override void OnPaint(PaintEventArgs e)
    {
        _cizici.Ciz(e.Graphics, _sonGoruntu, ClientSize);
    }

    // Arka plani OnPaint'te tamamen biz ciziyoruz; Windows'un ayrica silmesine gerek yok (titreme olmasin).
    protected override void OnPaintBackground(PaintEventArgs e)
    {
    }

    // ================= YARDIMCI =================

    private void OlayiTetikle(EventHandler? olay) => olay?.Invoke(this, EventArgs.Empty);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _butonYaziTipi?.Dispose();
        }
        base.Dispose(disposing);
    }
}
