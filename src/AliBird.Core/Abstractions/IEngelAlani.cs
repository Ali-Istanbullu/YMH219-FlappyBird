using AliBird.Core.Gameplay;

namespace AliBird.Core.Abstractions;

// Engellerin OyunMotoru'na gorunen yuzu (K66).
public interface IEngelAlani
{
    IReadOnlyList<Engel> Engeller { get; }

    // Tum engelleri hiz kadar sola kaydirir, ekrandan cikani saga tasir.
    void Guncelle(float hiz);

    // Bu karede yeni gecilen engel sayisi (puan icin).
    int YeniGecilenleriSay(float kusSolKenari);
}
