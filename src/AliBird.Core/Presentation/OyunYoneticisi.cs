using AliBird.Core.Abstractions;
using AliBird.Core.Configuration;
using AliBird.Core.Gameplay;
using AliBird.Core.Models;

namespace AliBird.Core.Presentation;

// MVP'deki Presenter (M19a, N5a). Oyunun "beyni" ile ekran arasindaki koprudur.
// Gorevleri:
//   - Form'dan gelen ham tuslari oyunun durumuna gore yorumlamak (D5a, CK1)
//   - Her tikte oyunu bir kare ilerletmek ve Form'a cizilecek goruntuyu vermek
//   - Ses calmak (ses kapaliysa hic cagirmamak, D4b)
//   - Rekoru okumak ve kaydetmek (K52: oyun bitince + oyun surerken kapanirsa)
//
// Form'u, SQLite'i, NAudio'yu, Serilog'u TANIMAZ; hepsini arayuzlerden alir (R2, DIP).
// HER dis cagri ve her kare try/catch ile korunur (R4); hatalar IHataBildirici'ye gider (K14).
public sealed class OyunYoneticisi
{
    private readonly IOyunGorunumu _gorunum;
    private readonly IOyunMotoruFabrikasi _fabrika;
    private readonly IPuanDeposu _puanDeposu;
    private readonly ISesCalar _sesCalar;
    private readonly IZamanlayici _zamanlayici;
    private readonly IHataBildirici _hataBildirici;
    private readonly ISaat _saat;
    private readonly OyunAyarlari _ayarlar;

    // Su anki oyunun motoru. Her yeni oyunda fabrikadan yenisi alinir (K19).
    private OyunMotoru _motor = null!;   // BaslatAsync'te atanir

    // Kayitli en yuksek skor (yoksa null, D3a). Yeni rekor kaydedilince guncellenir.
    private RekorKaydi? _rekor;

    private OyunDurumu _durum = OyunDurumu.Hazir;

    // M tusu ile kullanici sesi kapatti mi? (D4b: karari presenter verir)
    private bool _sesKapali;

    // Game Over aninin zamani: 750 ms boyunca Space dikkate alinmaz (CK2).
    private DateTime _oyunBittiZamani;

    // Bu oyunun rekoru zaten kaydedildi mi? (oyun bitince + kapanista iki kez yazilmasin)
    private bool _rekorKaydedildi;

    // Devam eden rekor kaydi (kapanista bitmesini beklemek icin, CK6).
    private Task _bekleyenKayit = Task.CompletedTask;

    // Ust uste kac karede hata olustu (CK3) ve bu hata kullaniciya gosterildi mi (M24 #7).
    private int _ardisikHataSayisi;
    private bool _kareHatasiGosterildi;

    public OyunYoneticisi(
        IOyunGorunumu gorunum,
        IOyunMotoruFabrikasi fabrika,
        IPuanDeposu puanDeposu,
        ISesCalar sesCalar,
        IZamanlayici zamanlayici,
        IHataBildirici hataBildirici,
        ISaat saat,
        OyunAyarlari ayarlar)
    {
        ArgumentNullException.ThrowIfNull(gorunum);
        ArgumentNullException.ThrowIfNull(fabrika);
        ArgumentNullException.ThrowIfNull(puanDeposu);
        ArgumentNullException.ThrowIfNull(sesCalar);
        ArgumentNullException.ThrowIfNull(zamanlayici);
        ArgumentNullException.ThrowIfNull(hataBildirici);
        ArgumentNullException.ThrowIfNull(saat);
        ArgumentNullException.ThrowIfNull(ayarlar);

        _gorunum = gorunum;
        _fabrika = fabrika;
        _puanDeposu = puanDeposu;
        _sesCalar = sesCalar;
        _zamanlayici = zamanlayici;
        _hataBildirici = hataBildirici;
        _saat = saat;
        _ayarlar = ayarlar;
    }

    // Disaridan durumu okumak icin (testler ve gerekirse Form).
    public OyunDurumu Durum => _durum;

    // ================= YASAM DONGUSU =================

    // Uygulama acilinca bir kez cagrilir: rekoru okur, ilk oyunu hazirlar, zamanlayiciyi baslatir.
    public async Task BaslatAsync()
    {
        // 1) Rekoru oku. Okunamazsa oyun yine acilir, rekor yokmus gibi davranilir (CK4, M24 #1/#4).
        try
        {
            _rekor = await _puanDeposu.RekoruOkuAsync();
        }
        catch (Exception hata)
        {
            _rekor = null;
            _hataBildirici.Bildir(hata, Gorunurluk.EkranVeGunluk,
                "Rekor okunamadı. Oyun rekorsuz başlatılıyor.");
        }

        // 2) Ilk oyunu hazirla (Hazir durumu: kus sabit bekler, E2a).
        Guvenli(YeniOyunHazirla, Gorunurluk.EkranVeGunluk, "Oyun hazırlanamadı.");

        // 3) Form olaylarina abone ol.
        _gorunum.BoslukBasildi += BoslukBasildiginda;
        _gorunum.PBasildi += PBasildiginda;
        _gorunum.MBasildi += MBasildiginda;
        _gorunum.YenidenBaslatTiklandi += YenidenBaslatTiklandiginda;
        _gorunum.OdakKaybedildi += OdakKaybedildiginde;

        // 4) Oyun dongusunu baslat. Zamanlayici hep calisir; Hazir/Duraklatildi'da sadece cizim yapilir.
        Guvenli(() =>
        {
            _zamanlayici.AralikMs = _ayarlar.ZamanlayiciAraligiMs;
            _zamanlayici.Tik += TikGeldiginde;
            _zamanlayici.Baslat();
        }, Gorunurluk.EkranVeGunluk, "Oyun döngüsü başlatılamadı.");

        Guvenli(GoruntuyuGonder, Gorunurluk.SadeceGunluk, string.Empty);
    }

    // Pencere kapanirken cagrilir. Oyun suruyorsa rekor kaydedilir (K52 b).
    public async Task KapatAsync()
    {
        Guvenli(() =>
        {
            _zamanlayici.Durdur();
            _zamanlayici.Tik -= TikGeldiginde;
        }, Gorunurluk.SadeceGunluk, string.Empty);

        // Oyun hala suruyorsa (oynaniyor ya da duraklatilmis) rekoru kaydet.
        if (_durum is OyunDurumu.Calisiyor or OyunDurumu.Duraklatildi)
        {
            _bekleyenKayit = RekoruKaydetAsync();
        }

        // Kayit bitene kadar bekle ama en fazla 2 sn (CK6); pencere sonsuza kadar asili kalmasin.
        try
        {
            await Task.WhenAny(_bekleyenKayit, Task.Delay(_ayarlar.KapanisKayitBeklemeMs));
        }
        catch (Exception hata)
        {
            _hataBildirici.Bildir(hata, Gorunurluk.SadeceGunluk, string.Empty);
        }
    }

    // ================= OYUN DONGUSU =================

    // Zamanlayicinin her tikinde (her karede) calisir.
    private void TikGeldiginde(object? gonderen, EventArgs e)
    {
        try
        {
            if (_durum == OyunDurumu.Calisiyor)
            {
                KareSonucu sonuc = _motor.Ilerlet();

                if (sonuc.KazanilanPuan > 0)
                {
                    SesCal(SesTuru.Puan);
                }

                if (sonuc.Carpti)
                {
                    OyunuBitir();
                }
            }

            GoruntuyuGonder();

            // Bu kare sorunsuz gecti: hata sayacini sifirla.
            _ardisikHataSayisi = 0;
        }
        catch (Exception hata)
        {
            // CK3: hata loga yazilir, bu kare atlanir.
            _ardisikHataSayisi++;

            // Ayni hata her karede (saniyede ~60 kez) tekrarlanirsa log dosyasini doldurmasin:
            // ilk 2 tekrar buradan, 3. tekrar asagidaki ekran bildirimiyle yazilir; sonra basarili
            // bir kare gelene kadar sessiz kalinir.
            if (_ardisikHataSayisi < _ayarlar.ArdisikHataSiniri)
            {
                _hataBildirici.Bildir(hata, Gorunurluk.SadeceGunluk, string.Empty);
            }

            // Ust uste 3 hata: oyunu durdur ve kullaniciya BIR KEZ soyle (M24 #7, #8).
            if (_ardisikHataSayisi >= _ayarlar.ArdisikHataSiniri && !_kareHatasiGosterildi)
            {
                _kareHatasiGosterildi = true;
                _hataBildirici.Bildir(hata, Gorunurluk.EkranVeGunluk,
                    "Oyunda beklenmeyen bir hata oluştu ve oyun durduruldu. Yeniden başlatmayı deneyin.");

                if (_durum == OyunDurumu.Calisiyor)
                {
                    Guvenli(OyunuBitir, Gorunurluk.SadeceGunluk, string.Empty);
                }
            }
        }
    }

    // Kus carpti: oyun biter (G3).
    private void OyunuBitir()
    {
        _durum = OyunDurumu.OyunBitti;
        _oyunBittiZamani = _saat.Simdi;

        // K13: carpma ve oyun bitti icin iki ayri ses.
        SesCal(SesTuru.Carpma);
        SesCal(SesTuru.OyunBitti);

        // K52: rekor oyun bitince kaydedilir. Beklenmez (await yok); oyun donmasin diye arka planda yazilir.
        _bekleyenKayit = RekoruKaydetAsync();
    }

    // Yeni bir oyun hazirlar: temiz motor, Hazir durumu.
    // CK7: yeniden baslatinca once "baslamak icin tusa bas" ekrani gelir.
    private void YeniOyunHazirla()
    {
        _motor = _fabrika.Olustur(_rekor);
        _durum = OyunDurumu.Hazir;
        _rekorKaydedildi = false;
        _ardisikHataSayisi = 0;
        _kareHatasiGosterildi = false;
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
                    SesCal(SesTuru.Zipla);
                    break;

                case OyunDurumu.Calisiyor:
                    _motor.Zipla();
                    SesCal(SesTuru.Zipla);
                    break;

                case OyunDurumu.Duraklatildi:
                    // Bilerek bos: duraklatmadan sadece P ile cikilir (yanlislikla devam edilmesin).
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
            // P: oynarken duraklatir, duraklatilmisken devam ettirir. Diger durumlarda etkisiz.
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
        // CK5: M her durumda calisir.
        _sesKapali = !_sesKapali;
    }

    private void YenidenBaslatTiklandiginda(object? gonderen, EventArgs e)
    {
        // Buton 750 ms beklemesinden etkilenmez (CK2); sadece Game Over'da anlamlidir.
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
        // K11: pencere odagi kaybedince oyun duraklar; geri gelince kendiliginden devam ETMEZ (CK1).
        if (_durum == OyunDurumu.Calisiyor)
        {
            _durum = OyunDurumu.Duraklatildi;
        }
    }

    // ================= YARDIMCILAR =================

    // Ses calar. Kullanici sesi kapattiysa ya da ses cihazi yoksa hic cagirmaz (D4b, K13).
    private void SesCal(SesTuru tur)
    {
        if (_sesKapali || !_sesCalar.KullanilabilirMi)
        {
            return;
        }

        try
        {
            _sesCalar.Cal(tur);
        }
        catch (Exception hata)
        {
            // Ses hatasi oyunu ASLA durdurmaz. Kullanici "Ses yok" gostergesini
            // ISesCalar.KullanilabilirMi uzerinden gorur (M24 #6); burada sadece loga yazilir.
            _hataBildirici.Bildir(hata, Gorunurluk.SadeceGunluk, string.Empty);
        }
    }

    // Yeni rekor varsa kaydeder. Kendi icinde butun hatalari yakalar; disari exception sizdirmaz.
    private async Task RekoruKaydetAsync()
    {
        if (_rekorKaydedildi || !_motor.KaydedilecekRekorVarMi)
        {
            return;
        }

        _rekorKaydedildi = true;
        var yeniRekor = new RekorKaydi(_motor.Puan, _saat.Simdi);

        // CK12: kayit basarisiz olsa bile bu oturumda yeni rekor gecerli sayilir
        // (bir sonraki oyun bu puani gecmeye calisir).
        _rekor = yeniRekor;

        try
        {
            await _puanDeposu.RekoruKaydetAsync(yeniRekor);
        }
        catch (Exception hata)
        {
            // M24 #2: rekor kaybolacagi icin kullanici bilmeli.
            _hataBildirici.Bildir(hata, Gorunurluk.EkranVeGunluk,
                "Rekor kaydedilemedi. Bu rekor oyun kapanınca kaybolabilir.");
        }
    }

    // Form'a bu karenin goruntusunu gonderir.
    private void GoruntuyuGonder()
    {
        var goruntu = new OyunGoruntusu(
            _durum,
            _motor.KusAlani,
            _motor.EngelGoruntuleri,
            _motor.Puan,
            _rekor,
            _motor.YeniRekorMu,
            SesDurumunuHesapla());

        _gorunum.Goster(goruntu);
    }

    // Sag alttaki ses gostergesi (K12): cihaz yoksa "Yok", kullanici kapattiysa "Kapali".
    private SesDurumu SesDurumunuHesapla()
    {
        if (!_sesCalar.KullanilabilirMi)
        {
            return SesDurumu.Yok;
        }
        return _sesKapali ? SesDurumu.Kapali : SesDurumu.Acik;
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
