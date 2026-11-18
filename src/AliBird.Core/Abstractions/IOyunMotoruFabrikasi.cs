using AliBird.Core.Gameplay;
using AliBird.Core.Models;

namespace AliBird.Core.Abstractions;

// Her yeni oyun icin temiz bir OyunMotoru ureten sozlesme.
// OyunMotoru DI container'a kaydedilmez, cunku her oyunda yenisi gerekir;
// presenter yeni oyunu bu fabrikadan ister.
public interface IOyunMotoruFabrikasi
{
    // oncekiRekor: yeni oyunun karsilastiracagi rekor (yoksa null)
    OyunMotoru Olustur(RekorKaydi? oncekiRekor);
}
