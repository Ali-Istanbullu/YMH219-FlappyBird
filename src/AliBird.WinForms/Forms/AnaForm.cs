using AliBird.Core.Abstractions;
using AliBird.Core.Configuration;
using AliBird.Core.Models;
using AliBird.WinForms.Rendering;

namespace AliBird.WinForms.Forms;

// Oyunun penceresi. MVP'deki PASIF View (M19a, N5a): IOyunGorunumu'nu uygular.
// Gorevleri sadece:
//   1) Ham tuslari ve olaylari presenter'a haber vermek (D5a): Space, P, M, buton, odak kaybi
//   2) Presenter'in verdigi OyunGoruntusu'nu cizdirmek (cizimi OyunCizici yapar)
// Oyunla ilgili HICBIR karar vermez; Space'in ne anlama geldigini bilmez.
// Bagimliliklarini constructor'dan alir (R2, DIP). Designer dosyasi yok; arayuz kodla kurulur.
public sealed class AnaForm : Form, IOyunGorunumu
{
    // K5: pencere 800x600 acilir, en kucuk 480x360 (ikisi de 4:3).
    private static readonly Size BaslangicBoyutu = new(800, 600);
    private static readonly Size EnKucukBoyut = new(480, 360);

    private readonly IOyunCizici _cizici;                 // K65: arayuz uzerinden
    private readonly IOlcekHesaplayici _olcekHesaplayici; // K65: arayuz uzerinden
    private readonly IHataBildirici _hataBildirici;
    private readonly SizeF _alanBoyutu;

    private readonly OdakAlmayanButon _yenidenBaslatButonu;
    private Font? _butonYaziTipi;

    // Presenter'in en son gonderdigi goruntu; OnPaint bunu cizer.
    private OyunGoruntusu? _sonGoruntu;

    // CK34: basili tutulan tus tekrar tekrar olay uretmesin diye o an basili olan tuslar.
    private readonly HashSet<Keys> _basiliTuslar = [];

    // M24 #7: cizim hatasi kullaniciya sadece BIR KEZ gosterilir.
    private bool _cizimHatasiBildirildi;

    public AnaForm(IOyunCizici cizici, IOlcekHesaplayici olcekHesaplayici, IHataBildirici hataBildirici, OyunAyarlari ayarlar, Tema tema)
    {
        ArgumentNullException.ThrowIfNull(cizici);
        ArgumentNullException.ThrowIfNull(olcekHesaplayici);
        ArgumentNullException.ThrowIfNull(hataBildirici);
        ArgumentNullException.ThrowIfNull(ayarlar);
        ArgumentNullException.ThrowIfNull(tema);
        _cizici = cizici;
        _olcekHesaplayici = olcekHesaplayici;
        _hataBildirici = hataBildirici;
        _alanBoyutu = new SizeF(ayarlar.AlanGenisligi, ayarlar.AlanYuksekligi);

        // ---- Pencere ayarlari ----
        Text = "AliBird";                                   // K3
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = BaslangicBoyutu;
        MinimumSize = SizeFromClientSize(EnKucukBoyut);     // K5
        BackColor = tema.KenarBosluk;
        KeyPreview = true;                                  // tuslar once forma gelsin (G7)

        // CK33: cift tamponlama -> cizim sirasinda titreme olmaz.
        DoubleBuffered = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

        // ---- Yeniden Baslat butonu (K11, CK32) ----
        // Arayuz kontrolleri formun kendi parcasidir; designer'in yaptigi gibi burada olusturulur.
        _yenidenBaslatButonu = new OdakAlmayanButon
        {
            Text = "Yeniden Başlat",
            Visible = false,
            FlatStyle = FlatStyle.Flat,
            BackColor = tema.Vurgu,      // renkler tek yerde: Tema (G1b)
            ForeColor = tema.Yazi,
            Cursor = Cursors.Hand
        };
        _yenidenBaslatButonu.FlatAppearance.BorderSize = 0;
        _yenidenBaslatButonu.Click += (_, _) => OlayiTetikle(YenidenBaslatTiklandi);
        Controls.Add(_yenidenBaslatButonu);

        // ---- Olaylar ----
        KeyDown += TusaBasildiginda;
        KeyUp += TusBirakildiginda;
        Deactivate += (_, _) => OlayiTetikle(OdakKaybedildi);   // K11: baska pencereye gecildi
        Resize += BoyutDegistiginde;
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

    // ================= TUSLAR (G7) =================

    private void TusaBasildiginda(object? gonderen, KeyEventArgs e)
    {
        if (e.KeyCode is not (Keys.Space or Keys.P or Keys.M))
        {
            return;
        }

        // Bu tusu biz isliyoruz; Windows baska bir seye (ornegin "ding" sesi) kullanmasin.
        e.Handled = true;
        e.SuppressKeyPress = true;

        // CK34: tus basili tutulursa Windows KeyDown'u tekrar tekrar gonderir; sadece ilkini al.
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
        // K11: pencere simge durumuna kucultulunce oyun duraklar.
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
        try
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
        catch (Exception hata)
        {
            _hataBildirici.Bildir(hata, Gorunurluk.SadeceGunluk, string.Empty);   // R6
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

    protected override void OnPaint(PaintEventArgs e)
    {
        try
        {
            _cizici.Ciz(e.Graphics, _sonGoruntu, ClientSize);
        }
        catch (Exception hata)
        {
            // M24 #7: cizim hatasi her karede tekrar gosterilmez; ilkinde bir kez bildirilir.
            if (!_cizimHatasiBildirildi)
            {
                _cizimHatasiBildirildi = true;
                // Mesaj kutusunu cizimin ICINDE acmayalim (pencere kilitlenebilir); cizim bitince ac.
                BeginInvoke(() => _hataBildirici.Bildir(hata, Gorunurluk.EkranVeGunluk,
                    "Oyun ekranı çizilirken bir hata oluştu. Ayrıntılar log dosyasında."));
            }
        }
    }

    // Arka plani OnPaint'te tamamen biz ciziyoruz; Windows'un ayrica silmesine gerek yok (titreme olmasin).
    protected override void OnPaintBackground(PaintEventArgs e)
    {
    }

    // ================= YARDIMCI =================

    // Olayi tetikler; dinleyicide hata olsa bile form cokmez (R6).
    private void OlayiTetikle(EventHandler? olay)
    {
        try
        {
            olay?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception hata)
        {
            _hataBildirici.Bildir(hata, Gorunurluk.SadeceGunluk, string.Empty);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _butonYaziTipi?.Dispose();
        }
        base.Dispose(disposing);
    }
}
