using System.Globalization;
using AliBird.Core.Abstractions;
using AliBird.Core.Errors;
using AliBird.Core.Models;
using AliBird.Infrastructure.FileSystem;
using Microsoft.Data.Sqlite;

namespace AliBird.Infrastructure.Persistence;

// IPuanDeposu'nun SQLite ile calisan uygulamasi.
// Veritabani: %LocalAppData%\AliBird\data\alibird.db
// Tablo     : Rekor(Id = 1, Puan, Tarih) -> her zaman TEK satir; yeni rekor gelince guncellenir.
//
// Tek sorumlulugu: rekoru diske yazmak ve diskten okumak.
// Bozuk dosyayi kurtarma isi IBozukVeritabaniKurtarici'da.
//
// Hata davranisi:
//  - Dosya ya da klasor silindiyse: bir sonraki istekte yeniden olusturulur ve loga yazilir.
//  - Dosya bozuksa: kurtariciya devredilir.
//  - Diger hatalar (izin yok, disk dolu ...): cagirana (RekorYoneticisi) gider; o kullaniciya gosterir.
public sealed class SqlitePuanDeposu : IPuanDeposu
{
    // Tablo yoksa olusturur. "CHECK (Id = 1)": ikinci satir eklenemez (tek rekor).
    private const string TabloOlusturSql =
        "CREATE TABLE IF NOT EXISTS Rekor (" +
        " Id    INTEGER PRIMARY KEY CHECK (Id = 1)," +
        " Puan  INTEGER NOT NULL," +
        " Tarih TEXT    NOT NULL)";

    private const string OkuSql = "SELECT Puan, Tarih FROM Rekor WHERE Id = 1";

    // UPSERT: satir yoksa ekler, varsa gunceller.
    private const string YazSql =
        "INSERT INTO Rekor (Id, Puan, Tarih) VALUES (1, $puan, $tarih) " +
        "ON CONFLICT(Id) DO UPDATE SET Puan = excluded.Puan, Tarih = excluded.Tarih";

    // tarih "o" bicimiyle saklanir (saat dilimi kaybolmaz).
    private const string TarihBicimi = "o";

    private readonly IUygulamaYollari _yollar;
    private readonly IBozukVeritabaniKurtarici _kurtarici;
    private readonly IGunlukcu _gunlukcu;

    public SqlitePuanDeposu(IUygulamaYollari yollar, IBozukVeritabaniKurtarici kurtarici, IGunlukcu gunlukcu)
    {
        ArgumentNullException.ThrowIfNull(yollar);
        ArgumentNullException.ThrowIfNull(kurtarici);
        ArgumentNullException.ThrowIfNull(gunlukcu);
        _yollar = yollar;
        _kurtarici = kurtarici;
        _gunlukcu = gunlukcu;
    }

    public async Task<RekorKaydi?> RekoruOkuAsync(CancellationToken iptal = default)
    {
        try
        {
            return await OkuAsync(iptal);
        }
        catch (Exception hata) when (_kurtarici.BozukMu(hata))
        {
            // bozuk dosya kenara alinir, rekor yokmus gibi devam edilir.
            _kurtarici.KenaraAl(hata);
            return null;
        }
        catch (Exception hata) when (DiskHatasiMi(hata))
        {
            // Kilitli dosya, izin, disk: Core'un tanidigi tipe sarilir; RekorYoneticisi kullaniciya gosterir.
            throw new PuanDeposuHatasi("Rekor dosyası okunamadı.", hata);
        }
    }

    public async Task RekoruKaydetAsync(RekorKaydi rekor, CancellationToken iptal = default)
    {
        ArgumentNullException.ThrowIfNull(rekor);

        try
        {
            try
            {
                await YazAsync(rekor, iptal);
            }
            catch (Exception hata) when (_kurtarici.BozukMu(hata))
            {
                // Bozuk dosyayi kenara al ve yeni (bos) veritabanina BIR KEZ daha yaz; rekor kaybolmasin.
                _kurtarici.KenaraAl(hata);
                await YazAsync(rekor, iptal);
            }
        }
        catch (Exception hata) when (DiskHatasiMi(hata))
        {
            throw new PuanDeposuHatasi("Rekor dosyası yazılamadı.", hata);
        }
    }

    // Beklenen dis dunya hatalari. Bunlarin disindakiler (bug) sarilmaz, oldugu gibi yukari cikar.
    private static bool DiskHatasiMi(Exception hata) =>
        hata is SqliteException or IOException or UnauthorizedAccessException;

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
            return null;   // hic rekor yok
        }

        int puan = okuyucu.GetInt32(0);

        // Tarih metni bozuksa FormatException -> kurtarici "bozuk" sayar.
        DateTime tarih = DateTime.Parse(okuyucu.GetString(1), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

        return new RekorKaydi(puan, tarih);
    }

    private async Task YazAsync(RekorKaydi rekor, CancellationToken iptal)
    {
        await using SqliteConnection baglanti = await BaglantiAcAsync(iptal);
        await using SqliteCommand komut = baglanti.CreateCommand();
        komut.CommandText = YazSql;

        // Parametreli SQL: enjeksiyona ve bicim hatalarina karsi guvenli.
        komut.Parameters.AddWithValue("$puan", rekor.Puan);
        komut.Parameters.AddWithValue("$tarih", rekor.Tarih.ToString(TarihBicimi, CultureInfo.InvariantCulture));

        await komut.ExecuteNonQueryAsync(iptal);
    }

    // Veritabanina baglanti acar ve tablo yoksa olusturur.
    private async Task<SqliteConnection> BaglantiAcAsync(CancellationToken iptal)
    {
        // VeritabaniDosyasi(): data klasoru silinmisse burada yeniden olusur.
        string dosya = _yollar.VeritabaniDosyasi();
        bool dosyaVardi = File.Exists(dosya);

        string baglantiMetni = new SqliteConnectionStringBuilder
        {
            DataSource = dosya,
            Mode = SqliteOpenMode.ReadWriteCreate,   // dosya yoksa olustur
            Pooling = false                          // is bitince dosya kilitli kalmasin
        }.ToString();

        // Kutuphanenin kendi baglanti nesnesi; bu sinifin isi SQLite'a baglanmak.
        var baglanti = new SqliteConnection(baglantiMetni);
        try
        {
            await baglanti.OpenAsync(iptal);

            await using SqliteCommand komut = baglanti.CreateCommand();
            komut.CommandText = TabloOlusturSql;
            await komut.ExecuteNonQueryAsync(iptal);   // bozuk dosyada hata genelde burada cikar

            if (!dosyaVardi)
            {
                // dosya yoktu (ilk acilis ya da kullanici sildi) -> yeniden olusturuldu.
                _gunlukcu.Bilgi($"Veritabanı dosyası bulunamadı, yeniden oluşturuldu: {dosya}");
            }

            return baglanti;
        }
        catch
        {
            // Acilamadiysa kapat ve hatayi yukari ilet (kaynak sizmasin).
            await baglanti.DisposeAsync();
            throw;
        }
    }
}
