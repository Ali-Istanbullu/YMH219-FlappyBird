using NAudio.Wave;

namespace AliBird.Infrastructure.Audio;

// Hazir bir ses dizisinden NAudio'nun calabilecegi bir "saglayici" ureten sozlesme (K65).
// NAudioSesCalar her calmada yeni saglayiciyi kendisi "new"lemez; bu arayuzden ister (R2, D).
public interface ISesSaglayiciUretici
{
    ISampleProvider Olustur(float[] ornekler, WaveFormat bicim);
}
