namespace AliBird.Core.Abstractions;

// Sure olcmek icin monoton (geri gitmeyen) sayac (CK53).
// Neden ISaat degil? ISaat duvar saatidir; kullanici saati degistirirse ya da yaz saati
// uygulamasi girerse geri/ileri atlayabilir. Kare suresi olcmek icin bu yanlis olur.
// Uygulamasi: Infrastructure'daki SistemKronometresi (Stopwatch).
public interface IKronometre
{
    // Kronometre baslatildigindan beri gecen sure.
    TimeSpan Gecen { get; }
}
