namespace AliBird.Core.Errors;

// Rekor dosyasi okunamadi ya da yazilamadi (disk, izin, kilitli dosya...).
// Bu BEKLENEN bir hatadir: kullaniciya sebebiyle gosterilir, oyun devam eder.
// Infrastructure, SQLite/dosya hatalarini bu tipe sarar; Core SQLite'i bilmeden sadece bunu yakalar.
public sealed class PuanDeposuHatasi : Exception
{
    public PuanDeposuHatasi(string mesaj, Exception icHata) : base(mesaj, icHata)
    {
    }
}
