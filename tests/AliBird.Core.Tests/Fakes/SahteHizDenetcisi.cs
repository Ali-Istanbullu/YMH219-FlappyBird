using AliBird.Core.Abstractions;

namespace AliBird.Core.Tests.Fakes;

// Sabit hiz verir, artirma cagrisini sayar (K66).
public sealed class SahteHizDenetcisi(List<string> cagrilar) : IHizDenetcisi
{
    public float Hiz => 7f;
    public void KareIlerlet(float carpan) => cagrilar.Add("hiz");
}
