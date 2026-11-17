using AliBird.Core.Models;

namespace AliBird.Core.Services;

// Sesle ilgili kararlari veren sozlesme (D4b, K12, K13, K61).
// OyunYoneticisi'nden ayrildi: "ses kapali mi, calinabilir mi, gosterge ne yazsin" kararlari burada (S).
public interface ISesYoneticisi
{
    // Sag alttaki gosterge icin: Acik / Kapali (M tusu) / Yok (cihaz yok).
    SesDurumu Durum { get; }

    // M tusu: sesi kapatir ya da acar (CK5).
    void SessizligiDegistir();

    // Ses kapali degilse ve cihaz varsa sesi calar. Asla exception firlatmaz (R6).
    void Cal(SesTuru tur);
}
