using System.Drawing;
using AliBird.Core.Abstractions;
using AliBird.Core.Configuration;

namespace AliBird.Core.Gameplay;

// Oyuncunun kontrol ettigi kus.
// Tek sorumlulugu: kendi dikey hareketini (ziplama + yer cekimi) hesaplamak.
// Carpip carpmadigina BAKMAZ; o is CarpismaDenetcisi'nin.
public sealed class Kus : IKus
{
    // Tum sayisal degerler buradan okunur; sinifin icinde sabit sayi yok.
    private readonly OyunAyarlari _ayarlar;

    // Kusun sol ust kosesinin yatay konumu. Oyun boyunca degismez (x = 100).
    public float X { get; }

    // Kusun sol ust kosesinin dikey konumu. 0 = ekranin en ustu, buyudukce asagi iner.
    public float Y { get; private set; }

    // Dikey hiz (piksel/kare). Eksi = yukari cikiyor, arti = asagi dusuyor.
    public float DikeyHiz { get; private set; }

    public Kus(OyunAyarlari ayarlar)
    {
        // Ayarlar verilmezse sinif calisamaz; hatayi hemen ve acik sekilde bildir.
        ArgumentNullException.ThrowIfNull(ayarlar);
        _ayarlar = ayarlar;

        X = ayarlar.KusX;
        Y = ayarlar.KusBaslangicY;   // y = 208
        DikeyHiz = 0f;               // Oyun basinda kus hareketsiz
    }

    // Space'e basildiginda cagrilir.
    // Hiz ziplama hizina AYARLANIR, eklenmez. Boylece her ziplama ayni yukseklige cikar.
    public void Zipla()
    {
        DikeyHiz = _ayarlar.ZiplamaHizi;
    }

    // Her tikte bir kez cagrilir: once yer cekimi hizi degistirir, sonra hiz konumu degistirir.
    // carpan: bu tik kac kare (1/60 sn) ediyor. 60 FPS'te 1; degerler Dino ile ayni kalir.
    public void Guncelle(float carpan)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(carpan);

        // 1) Yer cekimi: hiz her karede biraz daha asagi yone artar.
        //    Math.Min: hiz MaksimumDusmeHizi'ni (12) gecemez.
        DikeyHiz = Math.Min(DikeyHiz + _ayarlar.YerCekimi * carpan, _ayarlar.MaksimumDusmeHizi);

        // 2) Konum: hiz kadar hareket et.
        Y += DikeyHiz * carpan;

        // 3) Tavan: kus ekranin ustunden cikamaz, kenarda durur.
        //    Hiz SIFIRLANMAZ: yer cekimi hizi tersine cevirene kadar kus tavanda kalir.
        if (Y < 0f)
        {
            Y = 0f;
        }
    }

    // Ekranda cizilecek dikdortgen (30x24).
    public RectangleF Alan => new(X, Y, _ayarlar.KusGenisligi, _ayarlar.KusYuksekligi);

    // Carpisma kontrolunde kullanilan kutu: cizilen alandan her kenardan 3 px kucuk.
    // Neden? Kus GDI+ ile yuvarlak cizilecek; dikdortgenin bos koselerine degmek oyunu bitirmesin.
    public RectangleF CarpismaKutusu
    {
        get
        {
            float pay = _ayarlar.CarpismaPayi;
            return new RectangleF(
                X + pay,
                Y + pay,
                _ayarlar.KusGenisligi - 2 * pay,
                _ayarlar.KusYuksekligi - 2 * pay);
        }
    }
}
