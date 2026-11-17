using AliBird.Core.Abstractions;
using AliBird.Core.Configuration;

namespace AliBird.Core.Services;

// IKareCarpaniHesaplayici'nin uygulamasi (K68).
// Tek sorumlulugu: ms -> kare carpani donusumu ve ust sinir (S).
public sealed class KareCarpaniHesaplayici : IKareCarpaniHesaplayici
{
    private readonly float _hedefKareMs;
    private readonly float _enCokCarpan;

    public KareCarpaniHesaplayici(OyunAyarlari ayarlar)
    {
        ArgumentNullException.ThrowIfNull(ayarlar);
        _hedefKareMs = ayarlar.HedefKareSuresiMs;     // 1000/60 = 16.67
        _enCokCarpan = ayarlar.EnCokKareCarpani;      // 3
    }

    public float Hesapla(double gecenMs)
    {
        // Ilk kare ya da duraklatmadan sonraki ilk kare: olculen aralik yok, 1 kare say.
        if (gecenMs <= 0)
        {
            return 1f;
        }

        // Ust sinir: bilgisayar bir an takilirsa (logda 159 ms goruldu) kus tek adimda
        // borunun icinden gecmesin. 3 kare = 50 ms'den uzun takilmada oyun o an biraz yavaslar.
        return (float)Math.Min(gecenMs / _hedefKareMs, _enCokCarpan);
    }
}
