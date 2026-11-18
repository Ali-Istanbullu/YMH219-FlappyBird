using AliBird.Core.Models;
using AliBird.Infrastructure.Audio;

namespace AliBird.Infrastructure.Tests.Fakes;

// Ses uretirken hata (bug) veren sahte uretici.
public sealed class HataVerenSesUreteci : ISesUreteci
{
    public int OrnekHizi => 44100;

    public float[] Uret(SesTuru tur) => throw new InvalidOperationException("Sahte ses uretim hatasi");
}
