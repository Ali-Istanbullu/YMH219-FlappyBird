using AliBird.Core.Configuration;
using AliBird.Core.Models;

namespace AliBird.WinForms.Rendering;

// Katman 3: DURUM EKRANI.
// Tek sorumlulugu: oyunun durumuna gore ortadaki buyuk yazilari cizmek:
//   Hazir        -> "AliBird" + "Baslamak icin SPACE'e bas"
//   Duraklatildi -> karartma + "Duraklatildi" + "Devam etmek icin P'ye bas"
//   OyunBitti    -> karartma + "Oyun Bitti" + puan + "Yeniden baslamak icin SPACE"
// (butonun kendisi gercek bir WinForms butonu; AnaForm onu yerlestirir)
//   Calisiyor    -> hicbir sey
public sealed class DurumKatmaniCizici : IKatmanCizici
{
    private const string BaslikMetni = "AliBird";
    private const string BaslaMetni = "Başlamak için SPACE'e bas";
    private const string DuraklatildiMetni = "Duraklatıldı";
    private const string DevamMetni = "Devam etmek için P'ye bas";
    private const string OyunBittiMetni = "Oyun Bitti";
    private const string YenidenBaslaMetni = "Yeniden başlamak için SPACE";

    private readonly CizimKaynaklari _kaynaklar;
    private readonly IYaziCizici _yazici;
    private readonly OyunAyarlari _ayarlar;

    public DurumKatmaniCizici(CizimKaynaklari kaynaklar, IYaziCizici yazici, OyunAyarlari ayarlar)
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
        float orta = _ayarlar.AlanGenisligi / 2f;

        switch (goruntu.Durum)
        {
            case OyunDurumu.Hazir:
                _yazici.Yaz(g, BaslikMetni, _kaynaklar.BuyukYazi, _kaynaklar.Yazi, new PointF(orta, 120f), _kaynaklar.Ortala);
                _yazici.Yaz(g, BaslaMetni, _kaynaklar.OrtaYazi, _kaynaklar.Yazi, new PointF(orta, 185f), _kaynaklar.Ortala);
                break;

            case OyunDurumu.Duraklatildi:
                Karart(g);
                _yazici.Yaz(g, DuraklatildiMetni, _kaynaklar.BuyukYazi, _kaynaklar.Yazi, new PointF(orta, 165f), _kaynaklar.Ortala);
                _yazici.Yaz(g, DevamMetni, _kaynaklar.OrtaYazi, _kaynaklar.Yazi, new PointF(orta, 230f), _kaynaklar.Ortala);
                break;

            case OyunDurumu.OyunBitti:
                Karart(g);
                _yazici.Yaz(g, OyunBittiMetni, _kaynaklar.BuyukYazi, _kaynaklar.Yazi, new PointF(orta, 115f), _kaynaklar.Ortala);
                _yazici.Yaz(g, $"Puan: {goruntu.Puan}", _kaynaklar.OrtaYazi, _kaynaklar.Yazi, new PointF(orta, 180f), _kaynaklar.Ortala);
                _yazici.Yaz(g, YenidenBaslaMetni, _kaynaklar.KucukYazi, _kaynaklar.Yazi, new PointF(orta, 225f), _kaynaklar.Ortala);
                // Buton: EkranYerlesimi.YenidenBaslatButonAlani (y = 270) - AnaForm koyar.
                break;
        }
    }

    // Tum ekrani yari saydam siyahla ortup arka plani bastirir.
    private void Karart(Graphics g)
    {
        g.FillRectangle(_kaynaklar.Karartma, 0f, 0f, _ayarlar.AlanGenisligi, _ayarlar.AlanYuksekligi);
    }
}
