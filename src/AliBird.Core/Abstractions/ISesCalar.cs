using AliBird.Core.Models;

namespace AliBird.Core.Abstractions;

// Ses calma sozlesmesi. Uygulamasi: Infrastructure'daki NAudioSesCalar.
// Sadece ses calmakla ilgilenir. "Ses kapali mi" (M tusu) kararini OyunYoneticisi verir;
// ses kapaliysa Cal() hic cagrilmaz (S: tek sorumluluk).
public interface ISesCalar
{
    // Ses cihazi bulunabildi ve sesler hazirlandi mi?
    // false ise sag altta kirmizi "Ses yok" gosterilir, oyun yine oynanir.
    bool KullanilabilirMi { get; }

    // Istenen sesi calar. Ayni anda birden fazla ses calabilir (NAudio).
    void Cal(SesTuru tur);
}
