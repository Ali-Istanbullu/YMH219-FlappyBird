using System.Drawing;
using AliBird.Core.Abstractions;

namespace AliBird.Core.Tests.Fakes;

// Fizik yapmayan kus: cagrilari ortak listeye yazar (K66).
public sealed class SahteKus(List<string> cagrilar) : IKus
{
    public float X => 100f;
    public RectangleF Alan => new(100f, 200f, 30f, 24f);
    public RectangleF CarpismaKutusu => new(103f, 203f, 24f, 18f);
    public int ZiplamaSayisi { get; private set; }
    public float SonCarpan { get; private set; }

    public void Zipla() => ZiplamaSayisi++;
    public void Guncelle(float carpan)
    {
        SonCarpan = carpan;
        cagrilar.Add("kus");
    }
}
