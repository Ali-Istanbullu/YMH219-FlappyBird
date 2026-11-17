using AliBird.Core.Abstractions;
using AliBird.Core.Configuration;
using AliBird.Core.Gameplay;
using AliBird.Core.Models;
using AliBird.Core.Services;

namespace AliBird.Core.Presentation;

// MVP'deki Presenter (M19a, N5a). Oyun mantigi ile ekran arasindaki koordinator.
//
// Tek sorumlulugu (K61, Adim 8c'de daraltildi): KOORDINASYON
//   - Form'dan gelen ham tuslari oyunun durumuna gore yorumlamak (D5a, CK1)
//   - Her tikte oyunu bir kare ilerletmek ve Form'a cizilecek goruntuyu vermek
// Kendisi YAPMAZ, ilgili servise birakir:
//   - Rekor okuma/kaydetme          -> IRekorYoneticisi
//   - Ses kapali mi / ses calma     -> ISesYoneticisi
//   - Ardisik kare hatasi sayimi    -> IKareHataSayaci
//   - Kare araligi olcumu           -> IKareSuresiOlcer (Adim 9)
//   - Fizik, carpisma, puan         -> OyunMotoru (fabrikadan)
//
// Butun bagimliliklar constructor'dan arayuz olarak gelir (R2, D). Container'i bilmez.
// Her dis cagri ve her kare try/catch ile korunur (R4, R6).
public sealed class OyunYoneticisi
{
    private readonly IOyunGorunumu _gorunum;
    private readonly IOyunMotoruFabrikasi _fabrika;
    private readonly IRekorYoneticisi _rekorYoneticisi;
    private readonly ISesYoneticisi _sesYoneticisi;
    private readonly IKareHataSayaci _kareHataSayaci;
    private readonly IKareSuresiOlcer _kareSuresiOlcer;
    private readonly IZamanlayici _zamanlayici;
    private readonly IHataBildirici _hataBildirici;
    private readonly IGunlukcu _gunlukcu;
    private readonly ISaat _saat;
    private readonly OyunAyarlari _ayarlar;

    // Su anki oyunun motoru. Her yeni oyunda fabrikadan yenisi alinir (K19).
    private OyunMotoru _motor = null!;   // BaslatAsync'te atanir

    private OyunDurumu _durum = OyunDurumu.Hazir;

    // Game Over aninin zamani: 750 ms boyunca Space dikkate alinmaz (CK2).
    private DateTime _oyunBittiZamani;

    public OyunYoneticisi(
        IOyunGorunumu gorunum,
        IOyunMotoruFabrikasi fabrika,
        IRekorYoneticisi rekorYoneticisi,
        ISesYoneticisi sesYoneticisi,
        IKareHataSayaci kareHataSayaci,
        IKareSuresiOlcer kareSuresiOlcer,
        IZamanlayici zamanlayici,
        IHataBildirici hataBildirici,
        IGunlukcu gunlukcu,
        ISaat saat,
        OyunAyarlari ayarlar)
    {
        ArgumentNullException.ThrowIfNull(gorunum);
        ArgumentNullException.ThrowIfNull(fabrika);
        ArgumentNullException.ThrowIfNull(rekorYoneticisi);
        ArgumentNullException.ThrowIfNull(sesYoneticisi);
        ArgumentNullException.ThrowIfNull(kareHataSayaci);
        ArgumentNullException.ThrowIfNull(kareSuresiOlcer);
        ArgumentNullException.ThrowIfNull(zamanlayici);
        ArgumentNullException.ThrowIfNull(hataBildirici);
        ArgumentNullException.ThrowIfNull(gunlukcu);
        ArgumentNullException.ThrowIfNull(saat);
        ArgumentNullException.ThrowIfNull(ayarlar);

        _gorunum = gorunum;
        _fabrika = fabrika;
        _rekorYoneticisi = rekorYoneticisi;
        _sesYoneticisi = sesYoneticisi;
        _kareHataSayaci = kareHataSayaci;
        _kareSuresiOlcer = kareSuresiOlcer;
        _zamanlayici = zamanlayici;
        _hataBildirici = hataBildirici;
        _gunlukcu = gunlukcu;
        _saat = saat;
        _ayarlar = ayarlar;
    }

    // Disaridan durumu okumak icin (testler).
    public OyunDurumu Durum => _durum;

    // ================= YASAM DONGUSU =================

    // Uygulama acilinca bir kez: rekoru oku, ilk oyunu hazirla, zamanlayiciyi baslat.
    public async Task BaslatAsync()
    {
        // 1) Rekor (hatalari RekorYoneticisi kendisi yakalar ve bildirir, CK4).
        await _rekorYoneticisi.YukleAsync();

        // 2) Ilk oyun (Hazir: kus sabit bekler, E2a).
        Guvenli(YeniOyunHazirla, Gorunurluk.EkranVeGunluk, "Oyun hazırlanamadı.");

        // 3) Form olaylarina abone ol.
        _gorunum.BoslukBasildi += BoslukBasildiginda;
        _gorunum.PBasildi += PBasildiginda;
        _gorunum.MBasildi += MBasildiginda;
        _gorunum.YenidenBaslatTiklandi += YenidenBaslatTiklandiginda;
        _gorunum.OdakKaybedildi += OdakKaybedildiginde;

        // 4) Oyun dongusu (CK10: zamanlayici hep calisir; Hazir/Duraklatildi'da sadece cizim).
        Guvenli(() =>
        {
            _zamanlayici.AralikMs = _ayarlar.ZamanlayiciAraligiMs;
            _zamanlayici.Tik += TikGeldiginde;
            _zamanlayici.Baslat();
        }, Gorunurluk.EkranVeGunluk, "Oyun döngüsü başlatılamadı.");

        Guvenli(GoruntuyuGonder, Gorunurluk.SadeceGunluk, string.Empty);
    }

    // Pencere kapanirken: oyun suruyorsa rekor kaydedilir (K52 b), kayit en fazla 2 sn beklenir (CK6).
    public async Task KapatAsync()
    {
        Guvenli(() =>
        {
            _zamanlayici.Durdur();
            _zamanlayici.Tik -= TikGeldiginde;
        }, Gorunurluk.SadeceGunluk, string.Empty);

        if (_durum is OyunDurumu.Calisiyor or OyunDurumu.Duraklatildi)
        {
            Guvenli(() => _rekorYoneticisi.GerekirseKaydet(_motor.KaydedilecekRekorVarMi, _motor.Puan),
                Gorunurluk.SadeceGunluk, string.Empty);
        }

        await _rekorYoneticisi.BekleyenKaydiBekleAsync(_ayarlar.KapanisKayitBeklemeMs);
    }

    // ================= OYUN DONGUSU =================

    private void TikGeldiginde(object? gonderen, EventArgs e)
    {
        try
        {
            if (_durum == OyunDurumu.Calisiyor)
            {
                _kareSuresiOlcer.KareGeldi();   // Adim 9: gercek kare araligi
                KareSonucu sonuc = _motor.Ilerlet();

                if (sonuc.KazanilanPuan > 0)
                {
                    _sesYoneticisi.Cal(SesTuru.Puan);
                }

                if (sonuc.Carpti)
                {
                    OyunuBitir();
                }
            }
            else
            {
                // Hazir / Duraklatildi / OyunBitti: bekleme suresi olcume girmesin.
                _kareSuresiOlcer.AraVer();
            }

            GoruntuyuGonder();
            _kareHataSayaci.BasariliKare();
        }
        catch (Exception hata)
        {
            // CK3: hata sayilir; sinira ulasilip kullaniciya soylendiyse oyun durdurulur.
            bool durdur = _kareHataSayaci.HatayiKaydet(hata);
            if (durdur && _durum == OyunDurumu.Calisiyor)
            {
                Guvenli(OyunuBitir, Gorunurluk.SadeceGunluk, string.Empty);
            }
        }
    }

    // Kus carpti: oyun biter (G3).
    private void OyunuBitir()
    {
        _durum = OyunDurumu.OyunBitti;
        _oyunBittiZamani = _saat.Simdi;

        // K13: carpma ve oyun bitti icin iki ayri ses.
        _sesYoneticisi.Cal(SesTuru.Carpma);
        _sesYoneticisi.Cal(SesTuru.OyunBitti);

        // K63 + CK53: puan ve bu oyunda olculen gercek kare araligi.
        KareOlcumu? olcum = _kareSuresiOlcer.Ozet();
        _gunlukcu.Bilgi(olcum is null
            ? $"Oyun bitti. Puan: {_motor.Puan}"
            : $"Oyun bitti. Puan: {_motor.Puan}. Kare araligi: {olcum}");

        // K52: rekor oyun bitince kaydedilir (gerekiyorsa, arka planda).
        _rekorYoneticisi.GerekirseKaydet(_motor.KaydedilecekRekorVarMi, _motor.Puan);
    }

    // Yeni oyun: temiz motor, Hazir durumu (CK7).
    private void YeniOyunHazirla()
    {
        _motor = _fabrika.Olustur(_rekorYoneticisi.Rekor);
        _durum = OyunDurumu.Hazir;
        _rekorYoneticisi.YeniOyunBasladi();
        _kareHataSayaci.Sifirla();
        _kareSuresiOlcer.Sifirla();
    }

    // ================= TUSLAR (CK1) =================

    private void BoslukBasildiginda(object? gonderen, EventArgs e)
    {
        Guvenli(() =>
        {
            switch (_durum)
            {
                case OyunDurumu.Hazir:
                    // Ilk Space oyunu baslatir ve kusu ziplatir.
                    _durum = OyunDurumu.Calisiyor;
                    _motor.Zipla();
                    _sesYoneticisi.Cal(SesTuru.Zipla);
                    break;

                case OyunDurumu.Calisiyor:
                    _motor.Zipla();
                    _sesYoneticisi.Cal(SesTuru.Zipla);
                    break;

                case OyunDurumu.Duraklatildi:
                    // Bilerek bos: duraklatmadan sadece P ile cikilir.
                    break;

                case OyunDurumu.OyunBitti:
                    // CK2: Game Over'dan sonraki 750 ms icinde basilan Space yok sayilir.
                    TimeSpan gecen = _saat.Simdi - _oyunBittiZamani;
                    if (gecen.TotalMilliseconds >= _ayarlar.YenidenBaslatmaBeklemeMs)
                    {
                        YeniOyunHazirla();
                    }
                    break;
            }
        }, Gorunurluk.SadeceGunluk, string.Empty);
    }

    private void PBasildiginda(object? gonderen, EventArgs e)
    {
        Guvenli(() =>
        {
            // P: oynarken duraklatir, duraklatilmisken devam ettirir.
            if (_durum == OyunDurumu.Calisiyor)
            {
                _durum = OyunDurumu.Duraklatildi;
            }
            else if (_durum == OyunDurumu.Duraklatildi)
            {
                _durum = OyunDurumu.Calisiyor;
            }
        }, Gorunurluk.SadeceGunluk, string.Empty);
    }

    private void MBasildiginda(object? gonderen, EventArgs e)
    {
        // CK5: M her durumda calisir. Karar SesYoneticisi'nde.
        Guvenli(_sesYoneticisi.SessizligiDegistir, Gorunurluk.SadeceGunluk, string.Empty);
    }

    private void YenidenBaslatTiklandiginda(object? gonderen, EventArgs e)
    {
        // Buton 750 ms beklemesinden etkilenmez (CK2).
        Guvenli(() =>
        {
            if (_durum == OyunDurumu.OyunBitti)
            {
                YeniOyunHazirla();
            }
        }, Gorunurluk.SadeceGunluk, string.Empty);
    }

    private void OdakKaybedildiginde(object? gonderen, EventArgs e)
    {
        // K11: odak kaybinda oyun duraklar; kendiliginden devam ETMEZ (CK1).
        if (_durum == OyunDurumu.Calisiyor)
        {
            _durum = OyunDurumu.Duraklatildi;
        }
    }

    // ================= YARDIMCILAR =================

    // Form'a bu karenin goruntusunu gonderir (presenter'in "sunma" isi).
    private void GoruntuyuGonder()
    {
        var goruntu = new OyunGoruntusu(
            _durum,
            _motor.KusAlani,
            _motor.EngelGoruntuleri,
            _motor.Puan,
            _rekorYoneticisi.Rekor,
            _motor.YeniRekorMu,
            _sesYoneticisi.Durum);

        _gorunum.Goster(goruntu);
    }

    // Bir islemi try/catch icinde calistirir (R4). Kod tekrarini onlemek icin tek yerde.
    private void Guvenli(Action islem, Gorunurluk gorunurluk, string kullaniciMesaji)
    {
        try
        {
            islem();
        }
        catch (Exception hata)
        {
            _hataBildirici.Bildir(hata, gorunurluk, kullaniciMesaji);
        }
    }
}
