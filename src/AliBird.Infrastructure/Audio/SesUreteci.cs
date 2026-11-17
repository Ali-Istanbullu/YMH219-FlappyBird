using AliBird.Core.Models;

namespace AliBird.Infrastructure.Audio;

// ================================================================================
//  SES URETIMI (K13: sesler kodla uretilir ve kodda ISARETLENIR)
//
//  Ses = havadaki titresim. Bilgisayarda bunu saniyede 44100 kez olculmus sayilarla
//  (ornek / sample) temsil ederiz. Her ornek -1 ile 1 arasinda bir sayidir.
//
//  Bu sinif dort sesi matematikle uretir; hepsi SINUS dalgasidir (yumusak ton, S1b):
//      ornek = sin(2 * PI * frekans * zaman) * genlik
//  - frekans (Hz): sesin inceligi. 220 kalin, 1320 ince.
//  - genlik      : sesin yuksekligi. 0.3 = %30 (Ali, Adim 7 S2a).
//
//  Tek sorumlulugu: sayi dizisi uretmek (S). Ses kartini hic bilmez; calma isi NAudioSesCalar'in.
// ================================================================================
public sealed class SesUreteci : ISesUreteci
{
    // CD kalitesi: saniyede 44100 ornek.
    private const int Hiz = 44100;

    // Ses seviyesi %30 (Ali, Adim 7 S2a).
    private const float Genlik = 0.3f;

    // Her notanin basinda ve sonunda 5 ms'lik yumusak gecis.
    // Olmazsa ses aniden baslayip bittigi icin "cit" diye bir tik duyulur.
    private const double KenarYumusatmaSaniye = 0.005;

    public int OrnekHizi => Hiz;

    public float[] Uret(SesTuru tur) => tur switch
    {
        // ---- SES 1: ZIPLAMA ----------------------------------------------------
        // 80 ms boyunca frekansi 400 Hz'den 800 Hz'e kayan kisa, yukselen bip.
        SesTuru.Zipla => KayanNota(baslangicHz: 400, bitisHz: 800, saniye: 0.080),

        // ---- SES 2: PUAN -------------------------------------------------------
        // Iki notali "ding": once 880 Hz (70 ms), hemen ardindan 1320 Hz (70 ms).
        SesTuru.Puan => Birlestir(
            Nota(frekansHz: 880, saniye: 0.070),
            Nota(frekansHz: 1320, saniye: 0.070)),

        // ---- SES 3: CARPMA -----------------------------------------------------
        // 120 Hz'lik kalin ses; hizla sonumlenerek (azalarak) biten kisa bir "tok" (150 ms).
        SesTuru.Carpma => SonumlenenNota(frekansHz: 120, saniye: 0.150),

        // ---- SES 4: OYUN BITTI -------------------------------------------------
        // 150 ms sessizlik (carpma sesi bitsin diye) + alcalan uc nota: 660 -> 440 -> 220 Hz.
        SesTuru.OyunBitti => Birlestir(
            Sessizlik(saniye: 0.150),
            Nota(frekansHz: 660, saniye: 0.150),
            Nota(frekansHz: 440, saniye: 0.150),
            Nota(frekansHz: 220, saniye: 0.150)),

        _ => throw new ArgumentOutOfRangeException(nameof(tur), tur, "Bilinmeyen ses türü.")
    };

    // ================= YAPI TASLARI =================

    // Saniye cinsinden sureyi ornek sayisina cevirir. Ornek: 0.080 sn x 44100 = 3528 ornek.
    private static int OrnekSayisi(double saniye) => (int)Math.Round(saniye * Hiz);

    // Sabit frekansli tek nota.
    private static float[] Nota(double frekansHz, double saniye)
    {
        int n = OrnekSayisi(saniye);
        var ornekler = new float[n];
        for (int i = 0; i < n; i++)
        {
            double zaman = (double)i / Hiz;   // bu ornegin saniye cinsinden zamani
            double dalga = Math.Sin(2 * Math.PI * frekansHz * zaman);
            ornekler[i] = (float)(dalga * Genlik * Zarf(i, n));
        }
        return ornekler;
    }

    // Frekansi bastan sona dogrusal kayan nota (ziplama sesi).
    // Frekans her ornekte degistigi icin "faz"i biriktiriyoruz; yoksa dalga kopar ve ses bozulur.
    private static float[] KayanNota(double baslangicHz, double bitisHz, double saniye)
    {
        int n = OrnekSayisi(saniye);
        var ornekler = new float[n];
        double faz = 0;
        for (int i = 0; i < n; i++)
        {
            double oran = (double)i / n;                                  // 0 -> 1
            double frekans = baslangicHz + (bitisHz - baslangicHz) * oran;
            faz += 2 * Math.PI * frekans / Hiz;                           // bir orneklik ilerleme
            ornekler[i] = (float)(Math.Sin(faz) * Genlik * Zarf(i, n));
        }
        return ornekler;
    }

    // Hizla sonumlenen nota (carpma sesi): genlik e^(-20t) ile azalir.
    private static float[] SonumlenenNota(double frekansHz, double saniye)
    {
        int n = OrnekSayisi(saniye);
        var ornekler = new float[n];
        for (int i = 0; i < n; i++)
        {
            double zaman = (double)i / Hiz;
            double sonum = Math.Exp(-20 * zaman);   // 0 sn'de 1, 150 ms'de ~0.05
            ornekler[i] = (float)(Math.Sin(2 * Math.PI * frekansHz * zaman) * Genlik * sonum * Zarf(i, n));
        }
        return ornekler;
    }

    // Sessizlik: hepsi 0 olan ornekler.
    private static float[] Sessizlik(double saniye) => new float[OrnekSayisi(saniye)];

    // Parcalari arka arkaya ekler.
    private static float[] Birlestir(params float[][] parcalar) => parcalar.SelectMany(p => p).ToArray();

    // Yumusak baslangic/bitis carpani (0..1): ilk ve son 5 ms'de dogrusal artar/azalir.
    private static double Zarf(int i, int n)
    {
        int kenar = OrnekSayisi(KenarYumusatmaSaniye);   // 5 ms = 220 ornek
        if (i < kenar)
        {
            return (double)i / kenar;               // baslangicta 0'dan 1'e
        }
        if (i >= n - kenar)
        {
            return (double)(n - 1 - i) / kenar;     // sonda 1'den 0'a
        }
        return 1.0;
    }
}
