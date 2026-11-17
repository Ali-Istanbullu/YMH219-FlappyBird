using System.Globalization;

namespace AliBird.Core.Models;

// Bir oyun boyunca olculen kare araliklari (CK53).
public sealed record KareOlcumu(int AralikSayisi, double OrtalamaMs, double EnAzMs, double EnCokMs)
{
    // Saniyedeki kare sayisi (ortalamadan).
    public double Fps => OrtalamaMs > 0 ? 1000.0 / OrtalamaMs : 0;

    // Log satiri: "ort 15.6 ms (min 14.9 / max 31.3), 640 aralik, ~64 FPS"
    public override string ToString() => string.Format(CultureInfo.InvariantCulture,
        "ort {0:F1} ms (min {1:F1} / max {2:F1}), {3} aralik, ~{4:F0} FPS",
        OrtalamaMs, EnAzMs, EnCokMs, AralikSayisi, Fps);
}
