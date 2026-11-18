using AliBird.Core.Abstractions;
using AliBird.Infrastructure.FileSystem;
using Serilog;
using Serilog.Core;   // Logger tipi

namespace AliBird.Infrastructure.Logging;

// IGunlukcu'nun Serilog ile calisan uygulamasi.
// Tek sorumlulugu: Core'dan gelen log isteklerini Serilog'a aktarmak.
//
// 1 MB'a ulasan dosya kapanir, yenisi acilir (alibird_001.log ...); en fazla 5 dosya
//      tutulur, en eskisi Serilog tarafindan OTOMATIK silinir (elle silme kodu yok).
// her satirda tarih ve saat var.
// Bu sinif ASLA exception firlatmaz. Log yazilamazsa uygulama sessizce devam eder.
//
// IDisposable: uygulama kapanirken bekleyen satirlar diske yazilip dosya kapatilsin diye.
// DI container kapanirken Dispose'u kendisi cagirir.
public sealed class SerilogGunlukcu : IGunlukcu, IDisposable
{
    private const long DosyaBoyutuSiniri = 1024 * 1024;   // 1 MB
    private const int TutulacakDosyaSayisi = 5;           // toplam ~5 MB

    // "2026-11-17 14:50:03.123 +03:00 [ERR] mesaj" + alt satirda hata ayrintisi.
    private const string SatirBicimi =
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

    private readonly Logger _logger;

    public SerilogGunlukcu(IUygulamaYollari yollar)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(yollar);

            // Serilog'un kendi "yapilandirma -> logger" kurulumu. Bu sinifin tek isi Serilog'a
            // baglanmak oldugu icin Serilog nesnesini burada kurmak S'ye uygundur.
            _logger = new LoggerConfiguration()
                .MinimumLevel.Information()                     // Bilgi ve ustu
                .WriteTo.File(
                    path: yollar.GunlukDosyasi(),               // logs klasoru yoksa burada olusur
                    outputTemplate: SatirBicimi,
                    fileSizeLimitBytes: DosyaBoyutuSiniri,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: TutulacakDosyaSayisi)
                .CreateLogger();
        }
        catch
        {
            // Log klasoru olusturulamadi ya da dosya acilamadi (izin yok, disk dolu ...).
            // uygulama durmaz; hicbir sey yazmayan "bos" logger kullanilir.
            // Bilerek bos: log yazilamiyorsa bu hatayi yazacak bir yer de yok.
            // Hicbir hedefi (sink) olmayan logger: her cagriyi kabul eder, hicbir yere yazmaz.
            _logger = new LoggerConfiguration().CreateLogger();
        }
    }

    public void Bilgi(string mesaj)
    {
        try
        {
            // "{Mesaj:l}": mesaj icinde { } olsa bile Serilog bunu sablon sanmasin, aynen yazsin.
            _logger.Information("{Mesaj:l}", mesaj);
        }
        catch
        {
            // log hatasi uygulamayi asla durdurmaz.
        }
    }

    public void Uyari(string mesaj)
    {
        try
        {
            _logger.Warning("{Mesaj:l}", mesaj);
        }
        catch
        {
            // log hatasi uygulamayi asla durdurmaz.
        }
    }

    public void Hata(Exception hata, string mesaj)
    {
        try
        {
            // Hata nesnesi verilince Serilog yigin izini (stack trace) de alt satira yazar.
            _logger.Error(hata, "{Mesaj:l}", mesaj);
        }
        catch
        {
            // log hatasi uygulamayi asla durdurmaz.
        }
    }

    public void Dispose()
    {
        try
        {
            // Bekleyen satirlari diske yazar ve dosyayi kapatir.
            _logger.Dispose();
        }
        catch
        {
            // kapanista bile exception disari cikmaz.
        }
    }
}
