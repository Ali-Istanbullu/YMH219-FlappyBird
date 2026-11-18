using AliBird.Core.Abstractions;
using AliBird.Core.Models;

namespace AliBird.Core.Services;

// IKareSuresiOlcer'in uygulamasi.
// Tek sorumlulugu: ardisik iki kare arasindaki sureleri toplamak; ortalama/min/max vermek.
// Loga yazmaz; ozeti OyunYoneticisi oyun bitince loga yazar.
public sealed class KareSuresiOlcer : IKareSuresiOlcer
{
    private readonly IKronometre _kronometre;

    private TimeSpan? _oncekiKare;   // null: bir sonraki kare yeni baslangic
    private int _aralikSayisi;
    private double _toplamMs;
    private double _enAzMs;
    private double _enCokMs;

    public KareSuresiOlcer(IKronometre kronometre)
    {
        ArgumentNullException.ThrowIfNull(kronometre);
        _kronometre = kronometre;
    }

    public double KareGeldi()
    {
        TimeSpan simdi = _kronometre.Gecen;
        double gecenMs = 0;
        if (_oncekiKare is TimeSpan onceki)
        {
            double ms = (simdi - onceki).TotalMilliseconds;
            gecenMs = ms;
            _enAzMs = _aralikSayisi == 0 ? ms : Math.Min(_enAzMs, ms);
            _enCokMs = _aralikSayisi == 0 ? ms : Math.Max(_enCokMs, ms);
            _toplamMs += ms;
            _aralikSayisi++;
        }
        _oncekiKare = simdi;
        return gecenMs;
    }

    public void AraVer() => _oncekiKare = null;

    public void Sifirla()
    {
        _oncekiKare = null;
        _aralikSayisi = 0;
        _toplamMs = 0;
        _enAzMs = 0;
        _enCokMs = 0;
    }

    public KareOlcumu? Ozet() => _aralikSayisi == 0
        ? null
        : new KareOlcumu(_aralikSayisi, _toplamMs / _aralikSayisi, _enAzMs, _enCokMs);
}
