using AliBird.Core.Abstractions;
using AliBird.Core.Models;
using AliBird.Infrastructure.FileSystem;
using Microsoft.Data.Sqlite;

namespace AliBird.Infrastructure.Persistence;

// IBozukVeritabaniKurtarici'nin uygulamasi.
// Tek sorumlulugu: bozuk veritabani dosyasini tanimak ve kenara almak (S).
public sealed class BozukVeritabaniKurtarici : IBozukVeritabaniKurtarici
{
    // SQLite'in "dosya bozuk" hata kodlari (sqlite.org/rescode.html):
    //   11 = SQLITE_CORRUPT : veritabani bozuk
    //   26 = SQLITE_NOTADB  : dosya bir SQLite veritabani degil
    private const int SqliteBozuk = 11;
    private const int SqliteVeritabaniDegil = 26;

    private readonly IUygulamaYollari _yollar;
    private readonly IHataBildirici _hataBildirici;
    private readonly IGunlukcu _gunlukcu;
    private readonly ISaat _saat;

    public BozukVeritabaniKurtarici(IUygulamaYollari yollar, IHataBildirici hataBildirici, IGunlukcu gunlukcu, ISaat saat)
    {
        ArgumentNullException.ThrowIfNull(yollar);
        ArgumentNullException.ThrowIfNull(hataBildirici);
        ArgumentNullException.ThrowIfNull(gunlukcu);
        ArgumentNullException.ThrowIfNull(saat);
        _yollar = yollar;
        _hataBildirici = hataBildirici;
        _gunlukcu = gunlukcu;
        _saat = saat;
    }

    // FormatException: tarih metni bozuk (elle degistirilmis vb.).
    // SqliteException 11/26: dosyanin kendisi bozuk ya da veritabani degil.
    public bool BozukMu(Exception hata) =>
        hata is FormatException ||
        hata is SqliteException { SqliteErrorCode: SqliteBozuk or SqliteVeritabaniDegil };

    public void KenaraAl(Exception bozuklukHatasi)
    {
        try
        {
            string dosya = _yollar.VeritabaniDosyasi();
            if (File.Exists(dosya))
            {
                // CK23: silmek yerine yeniden adlandir -> eski veri kurtarilabilir.
                string yeniAd = Path.Combine(
                    Path.GetDirectoryName(dosya)!,
                    $"alibird.corrupt-{_saat.Simdi:yyyyMMddHHmmss}.db");
                File.Move(dosya, yeniAd, overwrite: true);
                _gunlukcu.Bilgi($"Bozuk veritabanı kenara alındı: {yeniAd}");   // K63
            }
        }
        catch (Exception kenaraAlmaHatasi)
        {
            // Yeniden adlandirilamazsa silmeyi dene; o da olmazsa loga yaz (R6).
            _hataBildirici.Bildir(kenaraAlmaHatasi, Gorunurluk.SadeceGunluk, string.Empty);
            try
            {
                File.Delete(_yollar.VeritabaniDosyasi());
            }
            catch (Exception silmeHatasi)
            {
                _hataBildirici.Bildir(silmeHatasi, Gorunurluk.SadeceGunluk, string.Empty);
            }
        }

        // M24 #4: kullanici bilmeli (E).
        _hataBildirici.Bildir(bozuklukHatasi, Gorunurluk.EkranVeGunluk,
            "Rekor verisi bozuktu ve sıfırlandı. Yeni rekorlar normal şekilde kaydedilecek.");
    }
}
