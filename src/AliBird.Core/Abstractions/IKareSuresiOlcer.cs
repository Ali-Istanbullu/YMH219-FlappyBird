using AliBird.Core.Models;

namespace AliBird.Core.Abstractions;

// Iki kare arasindaki gercek sureyi olcer (Adim 9, CK53).
// Fizik "kare basina" calistigi icin (K8) oyunun hizi zamanlayicinin gercek araligina baglidir;
// bu yuzden aralik tahmin edilmez, olculur.
public interface IKareSuresiOlcer
{
    // Oyun calisirken her karede cagrilir.
    void KareGeldi();

    // Duraklatma vb. araliklar olcume girmesin: bir sonraki kare yeni baslangic sayilir.
    void AraVer();

    // Yeni oyun: tum olcum silinir.
    void Sifirla();

    // Su ana kadarki olcum. Hic aralik olculmediyse null.
    KareOlcumu? Ozet();
}
