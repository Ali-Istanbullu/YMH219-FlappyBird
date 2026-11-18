namespace AliBird.Infrastructure.FileSystem;

// IUygulamaYollari'nin gercek uygulamasi.
// Veriler program klasorune DEGIL, %LocalAppData%\AliBird altina yazilir
// (Program Files'a yazilamaz; oyun kaldirilsa da veriler kalir).
// Kullanici klasorleri silerse uygulama hata vermez: her istekte klasor yoksa yeniden olusturulur.
public sealed class UygulamaYollari : IUygulamaYollari
{
    // Dosya adlari. Klasor ve dosya adlari Ingilizce.
    private const string VeriKlasorAdi = "data";
    private const string GunlukKlasorAdi = "logs";
    private const string VeritabaniDosyaAdi = "alibird.db";
    private const string GunlukDosyaAdi = "alibird.log";

    private readonly string _kokKlasor;

    // kokKlasor: gercekte VarsayilanKokKlasor(), testte gecici bir klasor.
    public UygulamaYollari(string kokKlasor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(kokKlasor);
        _kokKlasor = kokKlasor;
    }

    // Gercek konum: C:\Users\<kullanici>\AppData\Local\AliBird
    public static string VarsayilanKokKlasor() =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AliBird");

    public string VeriKlasoru() => KlasoruHazirla(Path.Combine(_kokKlasor, VeriKlasorAdi));

    public string GunlukKlasoru() => KlasoruHazirla(Path.Combine(_kokKlasor, GunlukKlasorAdi));

    public string VeritabaniDosyasi() => Path.Combine(VeriKlasoru(), VeritabaniDosyaAdi);

    public string GunlukDosyasi() => Path.Combine(GunlukKlasoru(), GunlukDosyaAdi);

    // Klasor yoksa olusturur, varsa hicbir sey yapmaz (Directory.CreateDirectory boyle calisir).
    // Boylece kullanici klasoru silse bile bir sonraki istekte geri gelir.
    // Olusturulamazsa (izin yok vb.) exception firlatir; bunu cagiran sinif yakalar.
    private static string KlasoruHazirla(string klasor)
    {
        Directory.CreateDirectory(klasor);
        return klasor;
    }
}
