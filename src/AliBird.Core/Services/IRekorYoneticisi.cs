using AliBird.Core.Models;

namespace AliBird.Core.Services;

// Rekorun yasam dongusunu yoneten sozlesme (K10, K52, K61).
// OyunYoneticisi'nden ayrildi: presenter artik rekoru nasil okuyup kaydedecegini bilmez (S).
public interface IRekorYoneticisi
{
    // Bilinen en yuksek skor. Hic yoksa null (D3a).
    RekorKaydi? Rekor { get; }

    // Uygulama acilinca rekoru okur. Okunamazsa kullaniciya bildirir, rekor null kalir (CK4).
    Task YukleAsync();

    // Yeni bir oyun basladi: "bu oyunun rekoru kaydedildi" bilgisi sifirlanir.
    void YeniOyunBasladi();

    // Bu oyunda yeni rekor varsa arka planda kaydeder. Ayni oyunda ikinci kez kaydetmez.
    void GerekirseKaydet(bool kaydedilecekRekorVar, int puan);

    // Devam eden kayit bitene kadar bekler, ama en fazla verilen sure kadar (CK6).
    Task BekleyenKaydiBekleAsync(int enFazlaMs);
}
