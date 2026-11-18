using AliBird.Core.Abstractions;
using AliBird.Core.Errors;
using AliBird.Core.Models;

namespace AliBird.Core.Services;

// IRekorYoneticisi'nin uygulamasi.
// Tek sorumlulugu: rekoru okumak, gerektiginde (oyun basina bir kez) kaydetmek ve
// kapanista bekleyen kaydi beklemek. Diske nasil yazildigini bilmez; IPuanDeposu'na birakir.
public sealed class RekorYoneticisi : IRekorYoneticisi
{
    private const string TarihSaatBicimi = "dd.MM.yyyy HH:mm";

    private readonly IPuanDeposu _puanDeposu;
    private readonly IHataBildirici _hataBildirici;
    private readonly IGunlukcu _gunlukcu;
    private readonly ISaat _saat;

    // Bu oyunun rekoru zaten kaydedildi mi? (oyun bitince + kapanista iki kez yazilmasin)
    private bool _buOyundaKaydedildi;

    // Devam eden kayit (kapanista bitmesini beklemek icin).
    private Task _bekleyenKayit = Task.CompletedTask;

    public RekorYoneticisi(IPuanDeposu puanDeposu, IHataBildirici hataBildirici, IGunlukcu gunlukcu, ISaat saat)
    {
        ArgumentNullException.ThrowIfNull(puanDeposu);
        ArgumentNullException.ThrowIfNull(hataBildirici);
        ArgumentNullException.ThrowIfNull(gunlukcu);
        ArgumentNullException.ThrowIfNull(saat);
        _puanDeposu = puanDeposu;
        _hataBildirici = hataBildirici;
        _gunlukcu = gunlukcu;
        _saat = saat;
    }

    public RekorKaydi? Rekor { get; private set; }

    public async Task YukleAsync()
    {
        try
        {
            Rekor = await _puanDeposu.RekoruOkuAsync();

            // onemli olay -> log
            _gunlukcu.Bilgi(Rekor is null
                ? "Kayıtlı rekor yok."
                : $"Rekor okundu: {Rekor.Puan} ({Rekor.Tarih.ToString(TarihSaatBicimi)})");
        }
        catch (PuanDeposuHatasi hata)
        {
            // Oyun rekorsuz acilir, kullanici sebebiyle bilgilendirilir.
            Rekor = null;
            _hataBildirici.Bildir(hata, Gorunurluk.EkranVeGunluk, "Rekor okunamadı. Oyun rekorsuz başlatılıyor.");
        }
    }

    public void YeniOyunBasladi()
    {
        _buOyundaKaydedildi = false;
    }

    public void GerekirseKaydet(bool kaydedilecekRekorVar, int puan)
    {
        if (_buOyundaKaydedildi || !kaydedilecekRekorVar)
        {
            return;
        }

        _buOyundaKaydedildi = true;
        var yeniRekor = new RekorKaydi(puan, _saat.Simdi);

        // kayit basarisiz olsa bile bu oturumda yeni rekor gecerli sayilir.
        Rekor = yeniRekor;

        // Beklenmez (await yok): oyun donmasin diye arka planda yazilir. Hatalari kendi icinde yakalar.
        _bekleyenKayit = KaydetAsync(yeniRekor);
    }

    public async Task BekleyenKaydiBekleAsync(int enFazlaMs)
    {
        // Kayit bitince ya da sure dolunca (hangisi once olursa) devam et; pencere asili kalmasin.
        await Task.WhenAny(_bekleyenKayit, Task.Delay(enFazlaMs));
    }

    // Kaydi yapar. Disk/izin hatasi kullaniciya gosterilir; kod hatasi (bug) yutulmaz.
    private async Task KaydetAsync(RekorKaydi rekor)
    {
        try
        {
            await _puanDeposu.RekoruKaydetAsync(rekor);
            _gunlukcu.Bilgi($"Yeni rekor kaydedildi: {rekor.Puan} ({rekor.Tarih.ToString(TarihSaatBicimi)})");
        }
        catch (PuanDeposuHatasi hata)
        {
            // Rekor kaybolacagi icin kullanici bilmeli.
            _hataBildirici.Bildir(hata, Gorunurluk.EkranVeGunluk,
                "Rekor kaydedilemedi. Bu rekor oyun kapanınca kaybolabilir.");
        }
    }
}
