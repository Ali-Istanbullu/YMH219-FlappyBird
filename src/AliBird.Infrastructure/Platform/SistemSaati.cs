using AliBird.Core.Abstractions;

namespace AliBird.Infrastructure.Platform;

// ISaat'in gercek uygulamasi: bilgisayarin yerel saati.
// Yerel saat kullaniyoruz cunku rekor tarihi kullaniciya gg.aa.yyyy olarak gosterilecek (K10).
public sealed class SistemSaati : ISaat
{
    public DateTime Simdi => DateTime.Now;
}
