namespace AliBird.Infrastructure.FileSystem;

// Uygulamanin veri ve log dosyalarinin nerede durdugunu soyleyen sozlesme.
// Neden arayuz? Testlerde gercek AppData yerine gecici bir klasor verebilmek icin (DIP).
// Core bu arayuzu bilmez; sadece Infrastructure siniflari (log, veritabani) kullanir.
public interface IUygulamaYollari
{
    // %LocalAppData%\AliBird\data (yoksa olusturulur)
    string VeriKlasoru();

    // %LocalAppData%\AliBird\logs (yoksa olusturulur)
    string GunlukKlasoru();

    // data\alibird.db
    string VeritabaniDosyasi();

    // logs\alibird.log
    string GunlukDosyasi();
}
