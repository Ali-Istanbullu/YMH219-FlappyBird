using System.Drawing;
using AliBird.Core.Abstractions;
using AliBird.Core.Gameplay;

namespace AliBird.Core.Tests.Fakes;

// Istenen cevabi veren carpisma denetcisi (K66).
public sealed class SahteCarpismaDenetcisi(List<string> cagrilar) : ICarpismaDenetcisi
{
    public bool Carpti { get; set; }

    public bool CarptiMi(RectangleF kusKutusu, IEnumerable<Engel> engeller)
    {
        cagrilar.Add("carpisma");
        return Carpti;
    }
}
