using AliBird.Core.Abstractions;
using AliBird.Core.Models;

namespace AliBird.Core.Tests.Fakes;

// Ses calmak yerine hangi seslerin istendigini listeye yazar.
public sealed class SahteSesCalar : ISesCalar
{
    public bool KullanilabilirMi { get; set; } = true;
    public List<SesTuru> CalinanSesler { get; } = [];

    // true: Cal() kod hatasi (bug) firlatir.
    public bool BugOlsun { get; set; }

    public void Cal(SesTuru tur)
    {
        if (BugOlsun)
        {
            throw new InvalidOperationException("Sahte bug");
        }
        CalinanSesler.Add(tur);
    }
}
