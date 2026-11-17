using System.Drawing;
using AliBird.Core.Configuration;

namespace AliBird.Core.Gameplay;

// Kusun bir seye carpip carpmadigini kontrol eder (G3, G9).
// Tek sorumlulugu: carpisma var mi yok mu cevabini vermek. Oyunu durdurmaz;
// sonuca gore ne yapilacagina OyunMotoru karar verir (S).
public sealed class CarpismaDenetcisi
{
    private readonly OyunAyarlari _ayarlar;

    public CarpismaDenetcisi(OyunAyarlari ayarlar)
    {
        ArgumentNullException.ThrowIfNull(ayarlar);
        _ayarlar = ayarlar;
    }

    // kusKutusu : kusun carpisma kutusu (3 px kucultulmus, E4b)
    // engeller  : ekrandaki engeller
    // true donerse oyun bitmeli.
    public bool CarptiMi(RectangleF kusKutusu, IEnumerable<Engel> engeller)
    {
        ArgumentNullException.ThrowIfNull(engeller);

        // 1) Zemin: kutunun alt kenari zeminin ust kenarina ulastiysa carpti (K6).
        if (kusKutusu.Bottom >= _ayarlar.ZeminUstY)
        {
            return true;
        }

        // 2) Engeller: kutu ust ya da alt boruyla kesisiyorsa carpti.
        //    IntersectsWith sadece kenarlarin degmesini carpisma saymaz, ic ice gecmeyi sayar.
        foreach (Engel engel in engeller)
        {
            if (kusKutusu.IntersectsWith(engel.UstAlan) || kusKutusu.IntersectsWith(engel.AltAlan))
            {
                return true;
            }
        }

        // Tavan carpisma DEGILDIR: kus tavanda durur (K6). Bunu Kus sinifi saglar.
        return false;
    }
}
