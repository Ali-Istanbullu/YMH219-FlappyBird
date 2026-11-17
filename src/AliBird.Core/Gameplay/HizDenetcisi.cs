using AliBird.Core.Configuration;

namespace AliBird.Core.Gameplay;

// Engellerin kayma hizini yonetir (K8).
// Chrome Dino ile ayni kural: hiz 6'dan baslar, her karede 0.001 artar, 13'te durur.
// Artis skora degil KAREYE baglidir.
// Tek sorumlulugu: guncel hizi bilmek ve artirmak (S).
public sealed class HizDenetcisi
{
    private readonly OyunAyarlari _ayarlar;

    // Su anki hiz (piksel/kare).
    public float Hiz { get; private set; }

    public HizDenetcisi(OyunAyarlari ayarlar)
    {
        ArgumentNullException.ThrowIfNull(ayarlar);
        _ayarlar = ayarlar;
        Hiz = ayarlar.BaslangicHizi;   // 6
    }

    // Her karede bir kez cagrilir.
    // Math.Min: hiz tavani (13) asmaz. (Dino'da "if < MAX ise ekle" yaziyor ve sayi tavani
    // cok az asabiliyor; biz tam 13'te sabitliyoruz.)
    public void KareIlerlet()
    {
        Hiz = Math.Min(Hiz + _ayarlar.Ivme, _ayarlar.MaksimumHiz);
    }
}
