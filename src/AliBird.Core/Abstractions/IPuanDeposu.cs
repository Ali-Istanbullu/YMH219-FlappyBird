using AliBird.Core.Models;

namespace AliBird.Core.Abstractions;

// En yuksek skorun kalici olarak saklandigi yer (K10).
// Core sadece bu sozlesmeyi bilir; SQLite oldugunu bilmez (DIP).
// Gercek uygulamasi: Infrastructure'daki SqlitePuanDeposu (Adim 6).
//
// Metotlar async (D2b): disk islemi suresince arayuz (UI) donmaz.
// CancellationToken: uygulama kapanirken yarim kalan islemi iptal edebilmek icin.
public interface IPuanDeposu
{
    // Kayitli rekoru okur. Hic rekor yoksa null doner (D3a).
    Task<RekorKaydi?> RekoruOkuAsync(CancellationToken iptal = default);

    // Yeni rekoru kaydeder (tek satir guncellenir, K10).
    Task RekoruKaydetAsync(RekorKaydi rekor, CancellationToken iptal = default);
}
