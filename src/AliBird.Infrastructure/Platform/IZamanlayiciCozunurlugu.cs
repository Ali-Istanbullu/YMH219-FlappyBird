namespace AliBird.Infrastructure.Platform;

// Windows zamanlayici cozunurlugunu oyun suresince artirma sozlesmesi (Adim 9, K67).
// Neden? Olcum (CK53): varsayilan cozunurlukte (~15.6 ms adim) 16 ms'lik zamanlayici
// ortalama ~29 ms'de bir calisti (~35 FPS). Fizik kare basina oldugu icin oyun yavas akti.
// Geri alma isi Dispose'ta: container kapanirken kendiliginden yapilir.
public interface IZamanlayiciCozunurlugu : IDisposable
{
    // Cozunurlugu artirmayi dener. Basarisiz olursa sadece loga yazar; oyun devam eder (R6).
    void Artir();
}
