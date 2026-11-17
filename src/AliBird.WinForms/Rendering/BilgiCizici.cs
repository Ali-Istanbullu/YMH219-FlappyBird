using System.Globalization;
using AliBird.Core.Configuration;
using AliBird.Core.Models;

namespace AliBird.WinForms.Rendering;

// Katman 2: BILGI YAZILARI (K11, K12, K59).
// Tek sorumlulugu: kenarlardaki yazilari cizmek (S):
//   sol ust  -> puan ve rekor (tarihli) / "YENI REKOR!"
//   sol alt  -> tus aciklamalari (her zaman gorunur)
//   sag alt  -> ses durumu ("Ses yok" kirmizi / "Ses kapali (M)" gri)
public sealed class BilgiCizici : IKatmanCizici
{
    private const string TuslarMetni = "SPACE: Zıpla   P: Duraklat   M: Ses";
    private const string YeniRekorMetni = "YENİ REKOR!";
    private const string SesYokMetni = "Ses yok";
    private const string SesKapaliMetni = "Ses kapalı (M)";

    // Rekor tarihi gg.aa.yyyy (K10).
    private const string TarihBicimi = "dd.MM.yyyy";
    private static readonly CultureInfo Turkce = CultureInfo.GetCultureInfo("tr-TR");

    private const float KenarBoslugu = 12f;

    private readonly CizimKaynaklari _kaynaklar;
    private readonly IYaziCizici _yazici;
    private readonly OyunAyarlari _ayarlar;

    public BilgiCizici(CizimKaynaklari kaynaklar, IYaziCizici yazici, OyunAyarlari ayarlar)
    {
        ArgumentNullException.ThrowIfNull(kaynaklar);
        ArgumentNullException.ThrowIfNull(yazici);
        ArgumentNullException.ThrowIfNull(ayarlar);
        _kaynaklar = kaynaklar;
        _yazici = yazici;
        _ayarlar = ayarlar;
    }

    public void Ciz(Graphics g, OyunGoruntusu goruntu)
    {
        PuanVeRekoruCiz(g, goruntu);
        AltBilgiyiCiz(g, goruntu.SesDurumu);
    }

    // Sol ust: puan ve rekor (K12, K10, CK35).
    private void PuanVeRekoruCiz(Graphics g, OyunGoruntusu goruntu)
    {
        _yazici.Yaz(g, $"Puan: {goruntu.Puan}", _kaynaklar.OrtaYazi, _kaynaklar.Yazi, new PointF(KenarBoslugu, 10f));

        var rekorKonumu = new PointF(KenarBoslugu, 38f);

        if (goruntu.YeniRekorMu)
        {
            // CK35: rekor kirildiysa rekor satiri guncel puani gosterir + turuncu "YENI REKOR!"
            string rekorMetni = $"Rekor: {goruntu.Puan}  ";
            _yazici.Yaz(g, rekorMetni, _kaynaklar.KucukYazi, _kaynaklar.Yazi, rekorKonumu);
            float genislik = g.MeasureString(rekorMetni, _kaynaklar.KucukYazi).Width;
            _yazici.Yaz(g, YeniRekorMetni, _kaynaklar.KucukYazi, _kaynaklar.Vurgu,
                new PointF(rekorKonumu.X + genislik, rekorKonumu.Y));
            return;
        }

        string metin = goruntu.Rekor is null
            ? "Rekor: -"
            : $"Rekor: {goruntu.Rekor.Puan} ({goruntu.Rekor.Tarih.ToString(TarihBicimi, Turkce)})";
        _yazici.Yaz(g, metin, _kaynaklar.KucukYazi, _kaynaklar.Yazi, rekorKonumu);
    }

    // Sol alt: tuslar (K11). Sag alt: ses durumu (K12). Ikisi de zemin seridinin uzerinde.
    private void AltBilgiyiCiz(Graphics g, SesDurumu sesDurumu)
    {
        float y = _ayarlar.ZeminUstY + 12f;
        _yazici.Yaz(g, TuslarMetni, _kaynaklar.KucukYazi, _kaynaklar.Yazi, new PointF(KenarBoslugu, y));

        var sagAlt = new PointF(_ayarlar.AlanGenisligi - KenarBoslugu, y);
        if (sesDurumu == SesDurumu.Yok)
        {
            _yazici.Yaz(g, SesYokMetni, _kaynaklar.KucukYazi, _kaynaklar.SesYok, sagAlt, _kaynaklar.SagaYasla);
        }
        else if (sesDurumu == SesDurumu.Kapali)
        {
            _yazici.Yaz(g, SesKapaliMetni, _kaynaklar.KucukYazi, _kaynaklar.SesKapali, sagAlt, _kaynaklar.SagaYasla);
        }
    }
}
