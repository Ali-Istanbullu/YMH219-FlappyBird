namespace AliBird.Core.Abstractions;

// Su anki tarih/saati veren sozlesme.
// Neden DateTime.Now'u dogrudan kullanmiyoruz? Testte "rekor 01.01.2026'da kirildi"
// gibi sabit bir zaman verebilmek icin (DIP). Uygulamasi: SistemSaati (Adim 5).
public interface ISaat
{
    DateTime Simdi { get; }
}
