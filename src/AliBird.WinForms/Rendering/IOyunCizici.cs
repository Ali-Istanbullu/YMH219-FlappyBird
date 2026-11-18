using AliBird.Core.Models;

namespace AliBird.WinForms.Rendering;

// Oyun ekranini cizen sozlesme. AnaForm somut cizicinin yerine bunu bilir.
public interface IOyunCizici
{
    // goruntu null ise (oyun henuz hazirlanmadi) sadece arka plan cizilir.
    void Ciz(Graphics g, OyunGoruntusu? goruntu, Size pencereBoyutu);
}
