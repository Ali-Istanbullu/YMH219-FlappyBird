using AliBird.Core.Models;

namespace AliBird.Infrastructure.Audio;

// Sesleri KODLA ureten sozlesme (harici ses dosyasi yok).
// Neden arayuz? NAudioSesCalar ureticiyi "new"lemesin ve testte hata veren
// sahte bir uretici verilebilsin (DIP).
public interface ISesUreteci
{
    // Saniyedeki ornek (sample) sayisi. Ses kartina bu hizla gonderilir.
    int OrnekHizi { get; }

    // Istenen sesin ham ornekleri: -1..1 arasi sayilar, tek kanal (mono).
    float[] Uret(SesTuru tur);
}
