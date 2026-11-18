using AliBird.Core.Abstractions;
using AliBird.Core.Models;

namespace AliBird.Core.Gameplay;

// Bu oyundaki puani ve onceki rekorla karsilastirmayi tutar.
// Tek sorumlulugu: sayi tutmak ve "rekor kirildi mi" sorusunu cevaplamak.
// Veritabanina YAZMAZ; kaydetme isi RekorYoneticisi + IPuanDeposu'nun.
public sealed class PuanTablosu : IPuanTablosu
{
    // Oyun basladiginda veritabanindan okunan rekor. Hic yoksa null.
    public RekorKaydi? OncekiRekor { get; }

    // Bu oyundaki puan = gecilen engel sayisi.
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

    // Ekranda "Yeni Rekor" vurgusu cikmali mi?
    //  - Hic rekor yoksa (ilk oyun) vurgu CIKMAZ.
    //  - Rekora esit puan rekor sayilmaz, gecmek gerekir.
    public bool YeniRekorMu => OncekiRekor is not null && Puan > OncekiRekor.Puan;

    // Oyun bitince veritabanina yeni rekor yazilmali mi?
    //  - Onceki rekor varsa: sadece gecildiyse.
    //  - Onceki rekor yoksa: puan 0'dan buyukse (ilk oyunun puani ilk rekor olur, ama 0 puan kaydedilmez).
    public bool KaydedilecekRekorVarMi => OncekiRekor is null ? Puan > 0 : YeniRekorMu;
}
