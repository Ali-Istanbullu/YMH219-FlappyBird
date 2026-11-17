using AliBird.Core.Abstractions;
using AliBird.Core.Configuration;
using AliBird.Core.Models;

namespace AliBird.Core.Services;

// IKareHataSayaci'nin uygulamasi.
// Tek sorumlulugu: ardisik kare hatalarini saymak ve ne zaman kullaniciya soylenecegine karar vermek (S).
//  - Ilk 2 hata sadece loga yazilir.
//  - 3. ardisik hatada kullaniciya BIR KEZ gosterilir ve oyunun durdurulmasi istenir (CK3).
//  - Sonraki ayni hatalar log dosyasini doldurmasin diye yazilmaz (saniyede ~60 kare).
public sealed class KareHataSayaci : IKareHataSayaci
{
    private readonly IHataBildirici _hataBildirici;
    private readonly int _sinir;

    private int _ardisikHataSayisi;
    private bool _kullaniciyaGosterildi;

    public KareHataSayaci(IHataBildirici hataBildirici, OyunAyarlari ayarlar)
    {
        ArgumentNullException.ThrowIfNull(hataBildirici);
        ArgumentNullException.ThrowIfNull(ayarlar);
        _hataBildirici = hataBildirici;
        _sinir = ayarlar.ArdisikHataSiniri;   // 3
    }

    public void BasariliKare()
    {
        _ardisikHataSayisi = 0;
    }

    public bool HatayiKaydet(Exception hata)
    {
        _ardisikHataSayisi++;

        // Sinira kadar: sadece log.
        if (_ardisikHataSayisi < _sinir)
        {
            _hataBildirici.Bildir(hata, Gorunurluk.SadeceGunluk, string.Empty);
            return false;
        }

        // Sinira ulasildi ve daha once gosterilmedi: kullaniciya bir kez soyle, oyunu durdurt.
        if (!_kullaniciyaGosterildi)
        {
            _kullaniciyaGosterildi = true;
            _hataBildirici.Bildir(hata, Gorunurluk.EkranVeGunluk,
                "Oyunda beklenmeyen bir hata oluştu ve oyun durduruldu. Yeniden başlatmayı deneyin.");
            return true;
        }

        // Zaten gosterildi: sessiz kal (log dolmasin).
        return false;
    }

    public void Sifirla()
    {
        _ardisikHataSayisi = 0;
        _kullaniciyaGosterildi = false;
    }
}
