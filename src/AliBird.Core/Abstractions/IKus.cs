using System.Drawing;

namespace AliBird.Core.Abstractions;

// Kusun OyunMotoru'na gorunen yuzu (K66).
// Sadece motorun kullandigi uyeler var (I: arayuz ayrimi).
public interface IKus
{
    // Kusun sol kenari; gecilen engeli saymak icin (E3a).
    float X { get; }

    // Ekranda cizilecek alan.
    RectangleF Alan { get; }

    // Carpisma kontrolunde kullanilan, her kenardan kucultulmus kutu (E4b).
    RectangleF CarpismaKutusu { get; }

    void Zipla();

    // carpan kare kadar yer cekimi + hareket (K68: 1 = 1/60 sn).
    void Guncelle(float carpan);
}
