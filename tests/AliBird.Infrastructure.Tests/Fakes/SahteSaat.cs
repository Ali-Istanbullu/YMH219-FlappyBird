using AliBird.Core.Abstractions;

namespace AliBird.Infrastructure.Tests.Fakes;

// Testin kontrol ettigi sabit saat.
public sealed class SahteSaat : ISaat
{
    public DateTime Simdi { get; set; } = new(2026, 11, 17, 15, 0, 0, DateTimeKind.Local);
}
