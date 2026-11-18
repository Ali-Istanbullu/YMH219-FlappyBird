using System.Diagnostics;
using AliBird.Core.Abstractions;

namespace AliBird.Infrastructure.Platform;

// IKronometre'nin gercek uygulamasi: .NET Stopwatch (yuksek cozunurluklu, monoton sayac).
// Stopwatch kutuphane nesnesi oldugu icin adaptorun icinde olusturulur.
public sealed class SistemKronometresi : IKronometre
{
    private readonly Stopwatch _sayac = Stopwatch.StartNew();

    public TimeSpan Gecen => _sayac.Elapsed;
}
