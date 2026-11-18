using AliBird.Core.Models;

namespace AliBird.Core.Abstractions;

// Iki kare arasindaki gercek sureyi olcer.
// Fizik "kare basina" calistigi icin oyunun hizi zamanlayicinin gercek araligina baglidir;
// bu yuzden aralik tahmin edilmez, olculur.
public interface IKareSuresiOlcer
{
    // Oyun calisirken her karede cagrilir. Onceki kareden beri gecen ms'yi dondurur
    // (ilk karede / AraVer'den sonra 0). Kare carpani buna gore hesaplanir.
    double KareGeldi();

    // Duraklatma vb. araliklar olcume girmesin: bir sonraki kare yeni baslangic sayilir.
    void AraVer();

    // Yeni oyun: tum olcum silinir.
    void Sifirla();

    // Su ana kadarki olcum. Hic aralik olculmediyse null.
    KareOlcumu? Ozet();
}
