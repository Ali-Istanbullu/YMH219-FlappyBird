using AliBird.Core.Abstractions;

namespace AliBird.WinForms.Platform;

// IZamanlayici'nin WinForms uygulamasi (PDF Timer istiyor).
// Icinde System.Windows.Forms.Timer calisir: tiklari arayuzun (UI) is parcaciginda uretir,
// bu yuzden presenter ve form ayni is parcaciginda calisir, kilit gerekmez.
// Tek sorumlulugu: WinForms Timer'ini Core'un anladigi arayuze baglamak (DIP).
public sealed class FormsZamanlayici : IZamanlayici, IDisposable
{
    // WinForms'un kendi zamanlayicisi; bu sinifin tek isi onu sarmak.
    private readonly System.Windows.Forms.Timer _timer = new();

    public FormsZamanlayici()
    {
        // Timer'in her tikini disariya Tik olayi olarak ilet.
        _timer.Tick += (_, _) => Tik?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? Tik;

    public int AralikMs
    {
        get => _timer.Interval;
        // Timer 0 ve eksi degeri kabul etmez (exception verir); en az 1 ms.
        set => _timer.Interval = Math.Max(1, value);
    }

    public void Baslat() => _timer.Start();

    public void Durdur() => _timer.Stop();

    public void Dispose()
    {
        _timer.Stop();
        _timer.Dispose();
    }
}
