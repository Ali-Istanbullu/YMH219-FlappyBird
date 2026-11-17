using AliBird.Core.Abstractions;

namespace AliBird.Core.Tests.Fakes;

// Zamani testin ilerlettigi kronometre (CK53).
public sealed class SahteKronometre : IKronometre
{
    public TimeSpan Gecen { get; private set; }

    public void Ilerlet(double ms) => Gecen += TimeSpan.FromMilliseconds(ms);
}
