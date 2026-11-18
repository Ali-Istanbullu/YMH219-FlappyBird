using AliBird.Core.Abstractions;
using AliBird.Core.Models;

namespace AliBird.Core.Services;

// ISesYoneticisi'nin uygulamasi.
// Tek sorumlulugu: ses kararlarini vermek. Sesi kendisi calmaz; ISesCalar'a birakir.
public sealed class SesYoneticisi : ISesYoneticisi
{
    private readonly ISesCalar _sesCalar;

    // M tusu ile kullanici sesi kapatti mi?
    private bool _sessiz;

    public SesYoneticisi(ISesCalar sesCalar)
    {
        ArgumentNullException.ThrowIfNull(sesCalar);
        _sesCalar = sesCalar;
    }

    // cihaz yoksa "Yok" (kirmizi), kullanici kapattiysa "Kapali" (gri), yoksa "Acik".
    public SesDurumu Durum
    {
        get
        {
            if (!_sesCalar.KullanilabilirMi)
            {
                return SesDurumu.Yok;
            }
            return _sessiz ? SesDurumu.Kapali : SesDurumu.Acik;
        }
    }

    public void SessizligiDegistir()
    {
        _sessiz = !_sessiz;
    }

    public void Cal(SesTuru tur)
    {
        // ses kapaliysa ya da cihaz yoksa ses caliciya hic gidilmez.
        if (_sessiz || !_sesCalar.KullanilabilirMi)
        {
            return;
        }

        // Ses cihazi hatalarini ses calar kendisi yakalar ve "Ses yok" durumuna gecer.
        _sesCalar.Cal(tur);
    }
}
