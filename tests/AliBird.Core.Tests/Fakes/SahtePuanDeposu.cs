using AliBird.Core.Abstractions;
using AliBird.Core.Models;

namespace AliBird.Core.Tests.Fakes;

// Veritabani yerine bellekte calisan sahte depo.
// Istenirse okuma/kaydetme hata firlatir (M24 #1, #2 senaryolari).
public sealed class SahtePuanDeposu : IPuanDeposu
{
    public RekorKaydi? KayitliRekor { get; set; }
    public bool OkumaHataFirlatsin { get; set; }
    public bool KaydetmeHataFirlatsin { get; set; }
    public int KaydetmeSayisi { get; private set; }

    public Task<RekorKaydi?> RekoruOkuAsync(CancellationToken iptal = default)
    {
        if (OkumaHataFirlatsin)
        {
            throw new IOException("Sahte okuma hatasi");
        }
        return Task.FromResult(KayitliRekor);
    }

    public Task RekoruKaydetAsync(RekorKaydi rekor, CancellationToken iptal = default)
    {
        KaydetmeSayisi++;
        if (KaydetmeHataFirlatsin)
        {
            throw new IOException("Sahte kaydetme hatasi");
        }
        KayitliRekor = rekor;
        return Task.CompletedTask;
    }
}
