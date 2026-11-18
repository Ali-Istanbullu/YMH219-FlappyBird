using AliBird.Core.Abstractions;
using AliBird.Core.Configuration;
using AliBird.Core.Gameplay;
using AliBird.Core.Models;
using AliBird.Core.Services;

namespace AliBird.Core.Presentation;

// MVP'deki Presenter. Oyun mantigi ile ekran arasindaki koordinator.
//
// Tek sorumlulugu: KOORDINASYON
//   - Form'dan gelen ham tuslari oyunun durumuna gore yorumlamak
//   - Her tikte oyunu bir kare ilerletmek ve Form'a cizilecek goruntuyu vermek
// Kendisi YAPMAZ, ilgili servise birakir:
//   - Rekor okuma/kaydetme          -> IRekorYoneticisi
//   - Ses kapali mi / ses calma     -> ISesYoneticisi
//   - Kare araligi olcumu           -> IKareSuresiOlcer
//   - Sure -> kare carpani          -> IKareCarpaniHesaplayici
//   - Fizik, carpisma, puan         -> OyunMotoru (fabrikadan)
//
// Butun bagimliliklar constructor'dan arayuz olarak gelir. Container'i bilmez.
// Burada try/catch yok: beklenen hatalari (rekor dosyasi, ses cihazi) ilgili servis yakalar.
// Kodumuzdaki bir hata (bug) yutulmaz: Debug'da debugger durur, Release'te genel yakalayici loglar.
public sealed class OyunYoneticisi
{
    private readonly IOyunGorunumu _gorunum;
    private readonly IOyunMotoruFabrikasi _fabrika;
    private readonly IRekorYoneticisi _rekorYoneticisi;
    private readonly ISesYoneticisi _sesYoneticisi;
    private readonly IKareSuresiOlcer _kareSuresiOlcer;
    private readonly IKareCarpaniHesaplayici _kareCarpani;
    private readonly IZamanlayici _zamanlayici;
    private readonly IGunlukcu _gunlukcu;
    private readonly ISaat _saat;
    private readonly OyunAyarlari _ayarlar;

    // Su anki oyunun motoru. Her yeni oyunda fabrikadan yenisi alinir.
    private OyunMotoru _motor = null!;   // BaslatAsync'te atanir

    private OyunDurumu _durum = OyunDurumu.Hazir;

    // Game Over aninin zamani: 750 ms boyunca Space dikkate alinmaz.
    private DateTime _oyunBittiZamani;

    public OyunYoneticisi(
        IOyunGorunumu gorunum,
        IOyunMotoruFabrikasi fabrika,
        IRekorYoneticisi rekorYoneticisi,
        ISesYoneticisi sesYoneticisi,
        IKareSuresiOlcer kareSuresiOlcer,
        IKareCarpaniHesaplayici kareCarpani,
        IZamanlayici zamanlayici,
        IGunlukcu gunlukcu,
        ISaat saat,
        OyunAyarlari ayarlar)
    {
        ArgumentNullException.ThrowIfNull(gorunum);
        ArgumentNullException.ThrowIfNull(fabrika);
        ArgumentNullException.ThrowIfNull(rekorYoneticisi);
        ArgumentNullException.ThrowIfNull(sesYoneticisi);
        ArgumentNullException.ThrowIfNull(kareSuresiOlcer);
        ArgumentNullException.ThrowIfNull(kareCarpani);
        ArgumentNullException.ThrowIfNull(zamanlayici);
        ArgumentNullException.ThrowIfNull(gunlukcu);
        ArgumentNullException.ThrowIfNull(saat);
        ArgumentNullException.ThrowIfNull(ayarlar);

        _gorunum = gorunum;
        _fabrika = fabrika;
        _rekorYoneticisi = rekorYoneticisi;
        _sesYoneticisi = sesYoneticisi;
        _kareSuresiOlcer = kareSuresiOlcer;
        _kareCarpani = kareCarpani;
        _zamanlayici = zamanlayici;
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
        // 1) Rekor (hatalari RekorYoneticisi kendisi yakalar ve bildirir).
        await _rekorYoneticisi.YukleAsync();

        // 2) Ilk oyun (Hazir: kus sabit bekler).
        YeniOyunHazirla();

        // 3) Form olaylarina abone ol.
        _gorunum.BoslukBasildi += BoslukBasildiginda;
        _gorunum.PBasildi += PBasildiginda;
        _gorunum.MBasildi += MBasildiginda;
        _gorunum.YenidenBaslatTiklandi += YenidenBaslatTiklandiginda;
        _gorunum.OdakKaybedildi += OdakKaybedildiginde;

        // 4) Oyun dongusu (zamanlayici hep calisir; Hazir/Duraklatildi'da sadece cizim).
        _zamanlayici.AralikMs = _ayarlar.ZamanlayiciAraligiMs;
        _zamanlayici.Tik += TikGeldiginde;
        _zamanlayici.Baslat();

        GoruntuyuGonder();
    }

    // Pencere kapanirken: oyun suruyorsa rekor kaydedilir, kayit en fazla 2 sn beklenir.
    public async Task KapatAsync()
    {
        _zamanlayici.Durdur();
        _zamanlayici.Tik -= TikGeldiginde;

        if (_durum is OyunDurumu.Calisiyor or OyunDurumu.Duraklatildi)
        {
            _rekorYoneticisi.GerekirseKaydet(_motor.KaydedilecekRekorVarMi, _motor.Puan);
        }

        await _rekorYoneticisi.BekleyenKaydiBekleAsync(_ayarlar.KapanisKayitBeklemeMs);
    }

    // ================= OYUN DONGUSU =================

    private void TikGeldiginde(object? gonderen, EventArgs e)
    {
        if (_durum == OyunDurumu.Calisiyor)
        {
            // Gercek gecen sure -> kac kare -> oyun her bilgisayarda ayni hizda akar.
            double gecenMs = _kareSuresiOlcer.KareGeldi();
            KareSonucu sonuc = _motor.Ilerlet(_kareCarpani.Hesapla(gecenMs));

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
    }

    // Kus carpti: oyun biter.
    private void OyunuBitir()
    {
        _durum = OyunDurumu.OyunBitti;
        _oyunBittiZamani = _saat.Simdi;

        // carpma ve oyun bitti icin iki ayri ses.
        _sesYoneticisi.Cal(SesTuru.Carpma);
        _sesYoneticisi.Cal(SesTuru.OyunBitti);

        // puan ve bu oyunda olculen gercek kare araligi.
        KareOlcumu? olcum = _kareSuresiOlcer.Ozet();
        _gunlukcu.Bilgi(olcum is null
            ? $"Oyun bitti. Puan: {_motor.Puan}"
            : $"Oyun bitti. Puan: {_motor.Puan}. Kare araligi: {olcum}");

        // rekor oyun bitince kaydedilir (gerekiyorsa, arka planda).
        _rekorYoneticisi.GerekirseKaydet(_motor.KaydedilecekRekorVarMi, _motor.Puan);
    }

    // Yeni oyun: temiz motor, Hazir durumu.
    private void YeniOyunHazirla()
    {
        _motor = _fabrika.Olustur(_rekorYoneticisi.Rekor);
        _durum = OyunDurumu.Hazir;
        _rekorYoneticisi.YeniOyunBasladi();
        _kareSuresiOlcer.Sifirla();
    }

    // ================= TUSLAR =================

    private void BoslukBasildiginda(object? gonderen, EventArgs e)
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
                // Game Over'dan sonraki 750 ms icinde basilan Space yok sayilir.
                TimeSpan gecen = _saat.Simdi - _oyunBittiZamani;
                if (gecen.TotalMilliseconds >= _ayarlar.YenidenBaslatmaBeklemeMs)
                {
                    YeniOyunHazirla();
                }
                break;
        }
    }

    private void PBasildiginda(object? gonderen, EventArgs e)
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
    }

    private void MBasildiginda(object? gonderen, EventArgs e)
    {
        // M her durumda calisir. Karar SesYoneticisi'nde.
        _sesYoneticisi.SessizligiDegistir();
    }

    private void YenidenBaslatTiklandiginda(object? gonderen, EventArgs e)
    {
        // Buton 750 ms beklemesinden etkilenmez.
        if (_durum == OyunDurumu.OyunBitti)
        {
            YeniOyunHazirla();
        }
    }

    private void OdakKaybedildiginde(object? gonderen, EventArgs e)
    {
        // odak kaybinda oyun duraklar; kendiliginden devam ETMEZ.
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
}
