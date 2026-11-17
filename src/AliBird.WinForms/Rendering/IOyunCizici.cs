using AliBird.Core.Models;

namespace AliBird.WinForms.Rendering;

// Oyun ekranini cizen sozlesme (K4, K65). AnaForm somut cizicinin yerine bunu bilir (D).
public interface IOyunCizici
{
    // goruntu null ise (oyun henuz hazirlanmadi) sadece arka plan cizilir.
    void Ciz(Graphics g, OyunGoruntusu? goruntu, Size pencereBoyutu);
}
