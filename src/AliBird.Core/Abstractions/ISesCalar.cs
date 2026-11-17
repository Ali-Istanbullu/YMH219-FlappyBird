using AliBird.Core.Models;

namespace AliBird.Core.Abstractions;

// Ses calma sozlesmesi (K13). Uygulamasi: Infrastructure'daki NAudioSesCalar (Adim 7).
// Sadece ses calmakla ilgilenir. "Ses kapali mi" (M tusu) kararini OyunYoneticisi verir;
// ses kapaliysa Cal() hic cagrilmaz (D4b, S: tek sorumluluk).
public interface ISesCalar
{
    // Ses cihazi bulunabildi ve sesler hazirlandi mi?
    // false ise sag altta kirmizi "Ses yok" gosterilir, oyun yine oynanir (K13, M24 #6).
    bool KullanilabilirMi { get; }

    // Istenen sesi calar. Ayni anda birden fazla ses calabilir (NAudio, M14).
    void Cal(SesTuru tur);
}
