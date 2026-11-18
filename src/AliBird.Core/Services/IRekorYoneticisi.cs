using AliBird.Core.Models;

namespace AliBird.Core.Services;

// Rekorun yasam dongusunu yoneten sozlesme.
// OyunYoneticisi'nden ayrildi: presenter artik rekoru nasil okuyup kaydedecegini bilmez.
public interface IRekorYoneticisi
{
    // Bilinen en yuksek skor. Hic yoksa null.
    RekorKaydi? Rekor { get; }

    // Uygulama acilinca rekoru okur. Okunamazsa kullaniciya bildirir, rekor null kalir.
    Task YukleAsync();

    // Yeni bir oyun basladi: "bu oyunun rekoru kaydedildi" bilgisi sifirlanir.
    void YeniOyunBasladi();

    // Bu oyunda yeni rekor varsa arka planda kaydeder. Ayni oyunda ikinci kez kaydetmez.
    void GerekirseKaydet(bool kaydedilecekRekorVar, int puan);

    // Devam eden kayit bitene kadar bekler, ama en fazla verilen sure kadar.
    Task BekleyenKaydiBekleAsync(int enFazlaMs);
}
