using System.Globalization;
using AliBird.Core.Abstractions;
using AliBird.Core.Models;
using AliBird.Infrastructure.FileSystem;
using Microsoft.Data.Sqlite;

namespace AliBird.Infrastructure.Persistence;

// IPuanDeposu'nun SQLite ile calisan uygulamasi (M11a, K10, N7).
// Veritabani: %LocalAppData%\AliBird\data\alibird.db
// Tablo     : Rekor(Id = 1, Puan, Tarih) -> her zaman TEK satir; yeni rekor gelince guncellenir.
//
// Tek sorumlulugu: rekoru diske yazmak ve diskten okumak (S).
// Core bu sinifi tanimaz; sadece IPuanDeposu'yu bilir (DIP).
//
// Hata davranisi:
//  - Dosya ya da klasor silindiyse: bir sonraki istekte sessizce yeniden olusturulur (K16, M24 #3).
//  - Dosya bozuksa: kenara alinir (silinmez), yerine bos veritabani olusur, kullaniciya
//    "rekor sifirlandi" denir (M24 #4, CK23, CK24).
//  - Diger hatalar (izin yok, disk dolu ...): exception cagirana (OyunYoneticisi) gider,
//    o da kullaniciya gosterir (M24 #1, #2, CK4).
public sealed class SqlitePuanDeposu : IPuanDeposu
{
    // SQLite'in "dosya bozuk" hata kodlari (sqlite.org/rescode.html):
    //   11 = SQLITE_CORRUPT : veritabani bozuk
    //   26 = SQLITE_NOTADB  : dosya bir SQLite veritabani degil
    private const int SqliteBozuk = 11;
    private const int SqliteVeritabaniDegil = 26;

    // Tablo yoksa olusturur. "CHECK (Id = 1)": tabloya ikinci bir satir eklenmesi imkansiz (tek rekor, K10).
    private const string TabloOlusturSql =
        "CREATE TABLE IF NOT EXISTS Rekor (" +
        " Id    INTEGER PRIMARY KEY CHECK (Id = 1)," +
        " Puan  INTEGER NOT NULL," +
        " Tarih TEXT    NOT NULL)";

    private const string OkuSql = "SELECT Puan, Tarih FROM Rekor WHERE Id = 1";

    // UPSERT: satir yoksa ekler, varsa gunceller. Boylece tablo hep tek satir kalir.
    private const string YazSql =
        "INSERT INTO Rekor (Id, Puan, Tarih) VALUES (1, $puan, $tarih) " +
        "ON CONFLICT(Id) DO UPDATE SET Puan = excluded.Puan, Tarih = excluded.Tarih";

    // CK25: tarih "o" bicimiyle saklanir, ornek: 2026-11-17T15:08:30.1230000+03:00
    // (saat dilimi bilgisi kaybolmaz, geri okurken ayni an elde edilir).
    private const string TarihBicimi = "o";

    private readonly IUygulamaYollari _yollar;
    private readonly IHataBildirici _hataBildirici;
    private readonly ISaat _saat;

    public SqlitePuanDeposu(IUygulamaYollari yollar, IHataBildirici hataBildirici, ISaat saat)
    {
        ArgumentNullException.ThrowIfNull(yollar);
        ArgumentNullException.ThrowIfNull(hataBildirici);
        ArgumentNullException.ThrowIfNull(saat);
        _yollar = yollar;
        _hataBildirici = hataBildirici;
        _saat = saat;
    }

    public async Task<RekorKaydi?> RekoruOkuAsync(CancellationToken iptal = default)
    {
        try
        {
            return await OkuAsync(iptal);
        }
        catch (Exception hata) when (BozukMu(hata))
        {
            // M24 #4: bozuk dosya kenara alinir, rekor yokmus gibi devam edilir.
            BozukDosyayiKenaraAl(hata);
            return null;
        }
        // Diger hatalar bilerek yakalanmaz: OyunYoneticisi yakalayip kullaniciya gosterir (CK4).
    }

    public async Task RekoruKaydetAsync(RekorKaydi rekor, CancellationToken iptal = default)
    {
        ArgumentNullException.ThrowIfNull(rekor);

        try
        {
            await YazAsync(rekor, iptal);
        }
        catch (Exception hata) when (BozukMu(hata))
        {
            // Bozuk dosyayi kenara al ve yeni (bos) veritabanina BIR KEZ daha yazmayi dene;
            // boylece kullanicinin az once kirdigi rekor kaybolmaz.
            BozukDosyayiKenaraAl(hata);
            await YazAsync(rekor, iptal);
        }
    }

    // ================= ICERIDEKI ADIMLAR =================

    private async Task<RekorKaydi?> OkuAsync(CancellationToken iptal)
    {
        // "await using": blok bitince baglanti HER durumda kapanir (hata olsa bile).
        await using SqliteConnection baglanti = await BaglantiAcAsync(iptal);
        await using SqliteCommand komut = baglanti.CreateCommand();
        komut.CommandText = OkuSql;

        await using SqliteDataReader okuyucu = await komut.ExecuteReaderAsync(iptal);
        if (!await okuyucu.ReadAsync(iptal))
        {
            return null;   // D3a: hic rekor yok
        }

        int puan = okuyucu.GetInt32(0);

        // Tarih metni bozuksa (elle degistirilmis vb.) FormatException olusur -> BozukMu yakalar.
        DateTime tarih = DateTime.Parse(okuyucu.GetString(1), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

        return new RekorKaydi(puan, tarih);
    }

    private async Task YazAsync(RekorKaydi rekor, CancellationToken iptal)
    {
        await using SqliteConnection baglanti = await BaglantiAcAsync(iptal);
        await using SqliteCommand komut = baglanti.CreateCommand();
        komut.CommandText = YazSql;

        // Parametre kullaniyoruz (metin birlestirme yok): SQL enjeksiyonuna ve bicim hatalarina karsi guvenli.
        komut.Parameters.AddWithValue("$puan", rekor.Puan);
        komut.Parameters.AddWithValue("$tarih", rekor.Tarih.ToString(TarihBicimi, CultureInfo.InvariantCulture));

        await komut.ExecuteNonQueryAsync(iptal);
    }

    // Veritabanina baglanti acar ve tablo yoksa olusturur.
    private async Task<SqliteConnection> BaglantiAcAsync(CancellationToken iptal)
    {
        // VeritabaniDosyasi(): data klasoru silinmisse burada yeniden olusur (K16).
        string dosya = _yollar.VeritabaniDosyasi();

        string baglantiMetni = new SqliteConnectionStringBuilder
        {
            DataSource = dosya,
            Mode = SqliteOpenMode.ReadWriteCreate,   // dosya yoksa olustur (K16)
            Pooling = false                          // CK22: is bitince dosya kilitli kalmasin
        }.ToString();

        // SqliteConnection kutuphanenin kendi nesnesi; her islemde yenisi acilip kapatilir (CK22).
        var baglanti = new SqliteConnection(baglantiMetni);
        try
        {
            await baglanti.OpenAsync(iptal);

            await using SqliteCommand komut = baglanti.CreateCommand();
            komut.CommandText = TabloOlusturSql;
            await komut.ExecuteNonQueryAsync(iptal);   // bozuk dosyada hata genelde burada cikar

            return baglanti;
        }
        catch
        {
            // Acilamadiysa kapat ve hatayi yukari ilet (R4: kaynak sizmasin).
            await baglanti.DisposeAsync();
            throw;
        }
    }

    // Hata "veritabani bozuk" turunden mi?
    private static bool BozukMu(Exception hata) =>
        hata is FormatException ||
        hata is SqliteException { SqliteErrorCode: SqliteBozuk or SqliteVeritabaniDegil };

    // CK23: bozuk dosyayi silmeden yeniden adlandirir (veri kurtarilabilsin), kullaniciya bildirir.
    private void BozukDosyayiKenaraAl(Exception bozuklukHatasi)
    {
        try
        {
            string dosya = _yollar.VeritabaniDosyasi();
            if (File.Exists(dosya))
            {
                string yeniAd = Path.Combine(
                    Path.GetDirectoryName(dosya)!,
                    $"alibird.corrupt-{_saat.Simdi:yyyyMMddHHmmss}.db");
                File.Move(dosya, yeniAd, overwrite: true);
            }
        }
        catch (Exception kenaraAlmaHatasi)
        {
            // Yeniden adlandirilamazsa (kilit, izin ...) silmeyi dene; o da olmazsa loga yaz.
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
