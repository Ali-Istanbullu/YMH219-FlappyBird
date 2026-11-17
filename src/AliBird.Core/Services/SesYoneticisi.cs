using AliBird.Core.Abstractions;
using AliBird.Core.Models;

namespace AliBird.Core.Services;

// ISesYoneticisi'nin uygulamasi.
// Tek sorumlulugu: ses kararlarini vermek (S). Sesi kendisi calmaz; ISesCalar'a birakir (D4b, D).
public sealed class SesYoneticisi : ISesYoneticisi
{
    private readonly ISesCalar _sesCalar;
    private readonly IHataBildirici _hataBildirici;

    // M tusu ile kullanici sesi kapatti mi?
    private bool _sessiz;

    public SesYoneticisi(ISesCalar sesCalar, IHataBildirici hataBildirici)
    {
        ArgumentNullException.ThrowIfNull(sesCalar);
        ArgumentNullException.ThrowIfNull(hataBildirici);
        _sesCalar = sesCalar;
        _hataBildirici = hataBildirici;
    }

    // K12: cihaz yoksa "Yok" (kirmizi), kullanici kapattiysa "Kapali" (gri), yoksa "Acik".
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
        // D4b: ses kapaliysa ya da cihaz yoksa ses caliciya hic gidilmez.
        if (_sessiz || !_sesCalar.KullanilabilirMi)
        {
            return;
        }

        try
        {
            _sesCalar.Cal(tur);
        }
        catch (Exception hata)
        {
            // Ses hatasi oyunu ASLA durdurmaz; kullanici "Ses yok" gostergesinden gorur (M24 #6, CK11).
            _hataBildirici.Bildir(hata, Gorunurluk.SadeceGunluk, string.Empty);
        }
    }
}
