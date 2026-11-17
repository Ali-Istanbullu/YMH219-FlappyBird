using AliBird.Core.Abstractions;

namespace AliBird.Core.Tests.Fakes;

// Zamani testin kontrol ettigi saat. Ileri() ile zamani istedigimiz kadar ilerletiriz.
public sealed class SahteSaat : ISaat
{
    public DateTime Simdi { get; set; } = new(2026, 11, 17, 12, 0, 0);

    public void Ileri(int milisaniye) => Simdi = Simdi.AddMilliseconds(milisaniye);
}
