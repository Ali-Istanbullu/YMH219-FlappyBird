using System.Drawing;

namespace AliBird.Core.Abstractions;

// Kusun OyunMotoru'na gorunen yuzu.
// Sadece motorun kullandigi uyeler var (I: arayuz ayrimi).
public interface IKus
{
    // Kusun sol kenari; gecilen engeli saymak icin.
    float X { get; }

    // Ekranda cizilecek alan.
    RectangleF Alan { get; }

    // Carpisma kontrolunde kullanilan, her kenardan kucultulmus kutu.
    RectangleF CarpismaKutusu { get; }

    void Zipla();

    // carpan kare kadar yer cekimi + hareket (1 = 1/60 sn).
    void Guncelle(float carpan);
}
