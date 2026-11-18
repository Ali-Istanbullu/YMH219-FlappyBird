using AliBird.Core.Models;

namespace AliBird.Core.Services;

// Sesle ilgili kararlari veren sozlesme.
// OyunYoneticisi'nden ayrildi: "ses kapali mi, calinabilir mi, gosterge ne yazsin" kararlari burada.
public interface ISesYoneticisi
{
    // Sag alttaki gosterge icin: Acik / Kapali (M tusu) / Yok (cihaz yok).
    SesDurumu Durum { get; }

    // M tusu: sesi kapatir ya da acar.
    void SessizligiDegistir();

    // Ses kapali degilse ve cihaz varsa sesi calar. Asla exception firlatmaz.
    void Cal(SesTuru tur);
}
