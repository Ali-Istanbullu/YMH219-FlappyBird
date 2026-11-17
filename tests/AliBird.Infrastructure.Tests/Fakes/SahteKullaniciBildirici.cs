using AliBird.Core.Abstractions;

namespace AliBird.Infrastructure.Tests.Fakes;

// Mesaj kutusu acmak yerine gosterilen mesajlari listeye ekler. Istenirse hata firlatir.
public sealed class SahteKullaniciBildirici : IKullaniciBildirici
{
    public List<string> GosterilenMesajlar { get; } = [];
    public bool HataFirlatsin { get; set; }

    public void MesajGoster(string mesaj)
    {
        if (HataFirlatsin)
        {
            throw new InvalidOperationException("Sahte mesaj kutusu hatasi");
        }
        GosterilenMesajlar.Add(mesaj);
    }
}
