using AliBird.Core.Configuration;
using AliBird.Core.Models;

namespace AliBird.WinForms.Rendering;

// Katman 1: SAHNE (K4: tamamen GDI+, harici resim yok).
// Tek sorumlulugu: oyun dunyasini cizmek -> gokyuzu, borular, zemin, kus (S).
// Arkadan one dogru cizer: once arkadaki, sonra ondeki.
public sealed class SahneCizici : IKatmanCizici
{
    // Boru agzi: bosluga bakan ucta borudan biraz genis bir kapak.
    private const float AgizYuksekligi = 14f;
    private const float AgizTasmasi = 4f;   // her yana 4 px

    private readonly CizimKaynaklari _kaynaklar;
    private readonly IKusCizici _kusCizici;   // CK55: kus ikonla ortak
    private readonly OyunAyarlari _ayarlar;

    public SahneCizici(CizimKaynaklari kaynaklar, IKusCizici kusCizici, OyunAyarlari ayarlar)
    {
        ArgumentNullException.ThrowIfNull(kaynaklar);
        ArgumentNullException.ThrowIfNull(kusCizici);
        ArgumentNullException.ThrowIfNull(ayarlar);
        _kaynaklar = kaynaklar;
        _kusCizici = kusCizici;
        _ayarlar = ayarlar;
    }

    public void Ciz(Graphics g, OyunGoruntusu goruntu)
    {
        foreach (EngelGoruntusu engel in goruntu.Engeller)
        {
            EngeliCiz(g, engel);
        }
        ZeminiCiz(g);
        _kusCizici.Ciz(g, goruntu.KusAlani);
    }

    // Boru: dolgu + kenar cizgisi + bosluga bakan uca genis bir "agiz".
    private void EngeliCiz(Graphics g, EngelGoruntusu engel)
    {
        BoruParcasiCiz(g, engel.UstAlan);
        BoruParcasiCiz(g, engel.AltAlan);

        // Ust borunun agzi altta, alt borunun agzi ustte (boslugun iki kenari).
        var ustAgiz = new RectangleF(engel.UstAlan.X - AgizTasmasi, engel.UstAlan.Bottom - AgizYuksekligi,
            engel.UstAlan.Width + 2 * AgizTasmasi, AgizYuksekligi);
        var altAgiz = new RectangleF(engel.AltAlan.X - AgizTasmasi, engel.AltAlan.Y,
            engel.AltAlan.Width + 2 * AgizTasmasi, AgizYuksekligi);
        BoruParcasiCiz(g, ustAgiz);
        BoruParcasiCiz(g, altAgiz);
    }

    private void BoruParcasiCiz(Graphics g, RectangleF alan)
    {
        if (alan.Width <= 0 || alan.Height <= 0)
        {
            return;
        }
        g.FillRectangle(_kaynaklar.Boru, alan);
        g.DrawRectangle(_kaynaklar.BoruKenar, alan.X, alan.Y, alan.Width, alan.Height);
    }

    // Zemin: alttaki 40 px'lik serit ve ust kenarinda bir cizgi (K6).
    private void ZeminiCiz(Graphics g)
    {
        float y = _ayarlar.ZeminUstY;
        g.FillRectangle(_kaynaklar.Zemin, 0f, y, _ayarlar.AlanGenisligi, _ayarlar.ZeminYuksekligi);
        g.DrawLine(_kaynaklar.ZeminCizgi, 0f, y, _ayarlar.AlanGenisligi, y);
    }
}
