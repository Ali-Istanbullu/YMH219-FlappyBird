using AliBird.Core.Abstractions;
using AliBird.Core.Models;

namespace AliBird.Core.Gameplay;

// Bu oyundaki puani ve onceki rekorla karsilastirmayi tutar (G4, K9, K10).
// Tek sorumlulugu: sayi tutmak ve "rekor kirildi mi" sorusunu cevaplamak.
// Veritabanina YAZMAZ; kaydetme isi Adim 4'te OyunYoneticisi + IPuanDeposu'nun (S, DIP).
public sealed class PuanTablosu : IPuanTablosu
{
    // Oyun basladiginda veritabanindan okunan rekor. Hic yoksa null (D3a).
    public RekorKaydi? OncekiRekor { get; }

    // Bu oyundaki puan = gecilen engel sayisi (K9).
    public int Puan { get; private set; }

    public PuanTablosu(RekorKaydi? oncekiRekor)
    {
        OncekiRekor = oncekiRekor;
        Puan = 0;
    }

    // Puani artirir. Normalde 1 gelir; ayni karede iki engel gecilirse 2 de gelebilir.
    public void PuanEkle(int miktar)
    {
        // Eksi puan mantiksiz; hatali cagriyi hemen yakalamak icin exception.
        ArgumentOutOfRangeException.ThrowIfNegative(miktar);
        Puan += miktar;
    }

    // Ekranda "Yeni Rekor" vurgusu cikmali mi? (K10, E7)
    //  - Hic rekor yoksa (ilk oyun) vurgu CIKMAZ (E7.1b).
    //  - Rekora esit puan rekor sayilmaz, gecmek gerekir (E7.2a).
    public bool YeniRekorMu => OncekiRekor is not null && Puan > OncekiRekor.Puan;

    // Oyun bitince veritabanina yeni rekor yazilmali mi?
    //  - Onceki rekor varsa: sadece gecildiyse.
    //  - Onceki rekor yoksa: puan 0'dan buyukse (ilk oyunun puani ilk rekor olur, ama 0 puan kaydedilmez).
    public bool KaydedilecekRekorVarMi => OncekiRekor is null ? Puan > 0 : YeniRekorMu;
}
