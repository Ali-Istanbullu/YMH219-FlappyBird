using NAudio.Wave;

namespace AliBird.Infrastructure.Audio;

// ISesSaglayiciUretici'nin uygulamasi (K65).
// Tek sorumlulugu: her calma icin yeni bir DiziSesSaglayici uretmek (S).
// Her seferinde yeni nesne gerekir, cunku saglayici kendi okuma konumunu tutar; boylece
// ayni ses ust uste calinabilir (M14b).
public sealed class DiziSesSaglayiciUretici : ISesSaglayiciUretici
{
    public ISampleProvider Olustur(float[] ornekler, WaveFormat bicim) => new DiziSesSaglayici(ornekler, bicim);
}
