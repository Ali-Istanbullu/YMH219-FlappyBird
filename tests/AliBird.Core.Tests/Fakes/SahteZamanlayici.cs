using AliBird.Core.Abstractions;

namespace AliBird.Core.Tests.Fakes;

// Gercek zaman beklemeden "tik" uretir: testte Tikla() ile istedigimiz kadar kare ilerletiriz.
public sealed class SahteZamanlayici : IZamanlayici
{
    public event EventHandler? Tik;
    public int AralikMs { get; set; }
    public bool CalisiyorMu { get; private set; }

    public void Baslat() => CalisiyorMu = true;
    public void Durdur() => CalisiyorMu = false;

    public void Tikla(int kareSayisi = 1)
    {
        for (int i = 0; i < kareSayisi; i++)
        {
            Tik?.Invoke(this, EventArgs.Empty);
        }
    }
}
