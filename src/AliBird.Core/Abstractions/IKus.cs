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

    // Bir karelik yer cekimi + hareket.
    void Guncelle();
}
