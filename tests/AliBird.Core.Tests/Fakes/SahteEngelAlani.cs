using AliBird.Core.Abstractions;
using AliBird.Core.Gameplay;

namespace AliBird.Core.Tests.Fakes;

// Engelsiz alan: her karede GecilenSayisi kadar "gecildi" der.
public sealed class SahteEngelAlani(List<string> cagrilar) : IEngelAlani
{
    public int GecilenSayisi { get; set; }
    public float SonHiz { get; private set; }
    public IReadOnlyList<Engel> Engeller { get; } = [];

    public void Guncelle(float hiz)
    {
        SonHiz = hiz;
        cagrilar.Add("engel");
    }

    public int YeniGecilenleriSay(float kusSolKenari)
    {
        cagrilar.Add("gecis");
        return GecilenSayisi;
    }
}
