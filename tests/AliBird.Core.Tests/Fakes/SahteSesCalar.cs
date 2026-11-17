using AliBird.Core.Abstractions;
using AliBird.Core.Models;

namespace AliBird.Core.Tests.Fakes;

// Ses calmak yerine hangi seslerin istendigini listeye yazar.
public sealed class SahteSesCalar : ISesCalar
{
    public bool KullanilabilirMi { get; set; } = true;
    public List<SesTuru> CalinanSesler { get; } = [];

    public void Cal(SesTuru tur) => CalinanSesler.Add(tur);
}
