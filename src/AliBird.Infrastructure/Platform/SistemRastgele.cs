using AliBird.Core.Abstractions;

namespace AliBird.Infrastructure.Platform;

// IRastgele'nin gercek uygulamasi.
// Random.Shared: .NET'in hazir, paylasilan rastgele sayi ureticisi; her seferinde "new Random()"
// yapmaya gerek kalmaz.
public sealed class SistemRastgele : IRastgele
{
    // enAz dahil, enCok haric (IRastgele sozlesmesi).
    public int Sonraki(int enAz, int enCok) => Random.Shared.Next(enAz, enCok);
}
