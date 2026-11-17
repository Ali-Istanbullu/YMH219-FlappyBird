using AliBird.Core.Abstractions;
using AliBird.Core.Configuration;

namespace AliBird.Core.Gameplay;

// Ekrandaki engellerin hepsini yonetir (G2).
// Tek sorumlulugu: engelleri kaydirmak, ekrandan cikani saga tasimak, gecilenleri saymak (S).
public sealed class EngelAlani : IEngelAlani
{
    private readonly OyunAyarlari _ayarlar;

    // Bosluk yuksekligini secmek icin. Arayuz oldugu icin testte sahte sinif verilebilir (DIP).
    private readonly IRastgele _rastgele;

    // Engelleri bu sinif "new"lemez; ureticiden ister (K65, R2).
    private readonly IEngelUretici _engelUretici;

    private readonly List<Engel> _engeller;

    // Disaridan sadece okunabilir liste: baska sinif engel ekleyip silemez.
    public IReadOnlyList<Engel> Engeller => _engeller;

    public EngelAlani(OyunAyarlari ayarlar, IRastgele rastgele, IEngelUretici engelUretici)
    {
        ArgumentNullException.ThrowIfNull(ayarlar);
        ArgumentNullException.ThrowIfNull(rastgele);
        ArgumentNullException.ThrowIfNull(engelUretici);
        _ayarlar = ayarlar;
        _rastgele = rastgele;
        _engelUretici = engelUretici;

        // Kac engel gerekir? Ekrana sigan engel sayisi + 1 yedek.
        // 640 / 320 = 2, +1 = 3 engel. Ayarlar degisirse sayi da kendiliginden degisir.
        int engelSayisi = (int)(ayarlar.AlanGenisligi / ayarlar.EngelAraligi) + 1;

        _engeller = new List<Engel>(engelSayisi);
        for (int i = 0; i < engelSayisi; i++)
        {
            // Ilk engel x = 960'ta (E5b), sonrakiler 320'ser piksel arayla: 960, 1280, 1600.
            float x = ayarlar.IlkEngelX + i * ayarlar.EngelAraligi;
            _engeller.Add(_engelUretici.Olustur(x, RastgeleBoslukUstY()));
        }
    }

    // Her karede cagrilir: tum engeller hiz kadar sola kayar, ekrandan cikan saga tasinir.
    public void Guncelle(float hiz)
    {
        foreach (Engel engel in _engeller)
        {
            engel.Kaydir(hiz);
        }

        foreach (Engel engel in _engeller)
        {
            if (engel.EkrandanCiktiMi)
            {
                // Yeni yer: en sagdaki engelin bir aralik sagi. Boylece engeller hep 320 arayla dizilir.
                float enSagX = _engeller.Max(e => e.X);
                engel.YenidenKonumla(enSagX + _ayarlar.EngelAraligi, RastgeleBoslukUstY());
            }
        }
    }

    // Bu karede kusun yeni gectigi engel sayisini dondurur (genelde 0 ya da 1). Puan buna gore artar (G4).
    public int YeniGecilenleriSay(float kusSolKenari)
    {
        int sayi = 0;
        foreach (Engel engel in _engeller)
        {
            if (engel.GecisiKontrolEt(kusSolKenari))
            {
                sayi++;
            }
        }
        return sayi;
    }

    // Boslugun ust kenari icin rastgele bir Y secer (K7).
    // Bosluk tavana en az 60 px, zemine en az 60 px uzak olmali:
    //   en az  = 60
    //   en cok = 440 (zemin) - 60 - 150 (bosluk) = 230
    // IRastgele.Sonraki ust siniri HARIC tuttugu icin +1 veriyoruz: 60..230 arasi.
    private float RastgeleBoslukUstY()
    {
        int enAz = (int)_ayarlar.KenarPayi;
        int enCok = (int)(_ayarlar.ZeminUstY - _ayarlar.KenarPayi - _ayarlar.BoslukYuksekligi);
        return _rastgele.Sonraki(enAz, enCok + 1);
    }
}
