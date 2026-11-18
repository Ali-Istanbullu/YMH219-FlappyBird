using AliBird.Core.Abstractions;
using AliBird.Core.Errors;
using AliBird.Core.Models;

namespace AliBird.Core.Tests.Fakes;

// Veritabani yerine bellekte calisan sahte depo.
// Istenirse beklenen depo hatasi (PuanDeposuHatasi) ya da kod hatasi (bug) firlatir.
public sealed class SahtePuanDeposu : IPuanDeposu
{
    public RekorKaydi? KayitliRekor { get; set; }
    public bool OkumaHataFirlatsin { get; set; }
    public bool KaydetmeHataFirlatsin { get; set; }
    public bool OkumadaBugOlsun { get; set; }
    public int KaydetmeSayisi { get; private set; }

    public Task<RekorKaydi?> RekoruOkuAsync(CancellationToken iptal = default)
    {
        if (OkumadaBugOlsun)
        {
            throw new InvalidOperationException("Sahte bug");
        }
        if (OkumaHataFirlatsin)
        {
            throw new PuanDeposuHatasi("Sahte okuma hatasi", new IOException("disk"));
        }
        return Task.FromResult(KayitliRekor);
    }

    public Task RekoruKaydetAsync(RekorKaydi rekor, CancellationToken iptal = default)
    {
        KaydetmeSayisi++;
        if (KaydetmeHataFirlatsin)
        {
            throw new PuanDeposuHatasi("Sahte kaydetme hatasi", new IOException("disk"));
        }
        KayitliRekor = rekor;
        return Task.CompletedTask;
    }
}
