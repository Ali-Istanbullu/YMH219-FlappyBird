using NAudio.Wave;

namespace AliBird.Infrastructure.Audio;

// Onceden uretilmis bir ses dizisini NAudio'ya parca parca veren kucuk yardimci.
// NAudio sesi calarken "bana su kadar ornek ver" diye Read'i tekrar tekrar cagirir;
// dizi bitince 0 doneriz ve NAudio bu sesi karistiricidan kendiliginden cikarir.
// Her Cal() cagrisinda yenisi olusturulur; boylece ayni ses ust uste calinabilir.
public sealed class DiziSesSaglayici : ISampleProvider
{
    private readonly float[] _ornekler;
    private int _konum;   // simdiye kadar verilen ornek sayisi

    public DiziSesSaglayici(float[] ornekler, WaveFormat bicim)
    {
        ArgumentNullException.ThrowIfNull(ornekler);
        ArgumentNullException.ThrowIfNull(bicim);
        _ornekler = ornekler;
        WaveFormat = bicim;
    }

    public WaveFormat WaveFormat { get; }

    // NAudio 3: Read tek bir Span<float> alir (eski float[]/offset/count bicimi kalkti).
    // Span = bellekteki bir dizinin bir bolumune bakan "pencere"; kopyalamayi azaltir.
    public int Read(Span<float> buffer)
    {
        int kalan = _ornekler.Length - _konum;
        int verilecek = Math.Min(buffer.Length, kalan);
        _ornekler.AsSpan(_konum, verilecek).CopyTo(buffer);
        _konum += verilecek;
        return verilecek;   // 0 = ses bitti
    }
}
