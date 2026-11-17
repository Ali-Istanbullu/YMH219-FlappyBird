using AliBird.Core.Abstractions;

namespace AliBird.Core.Tests.Fakes;

// Log yazmak yerine kayitlari listeye ekler; testte "su olay loglandi mi" diye bakariz (K63).
public sealed class SahteGunlukcu : IGunlukcu
{
    public List<string> Bilgiler { get; } = [];
    public List<string> Uyarilar { get; } = [];
    public List<string> Hatalar { get; } = [];

    public void Bilgi(string mesaj) => Bilgiler.Add(mesaj);
    public void Uyari(string mesaj) => Uyarilar.Add(mesaj);
    public void Hata(Exception hata, string mesaj) => Hatalar.Add(mesaj);
}
