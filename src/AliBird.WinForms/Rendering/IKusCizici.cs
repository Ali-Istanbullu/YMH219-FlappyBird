namespace AliBird.WinForms.Rendering;

// Kusu verilen alana cizen sozlesme (Adim 10, CK55).
// Hem oyun (SahneCizici) hem ikon ureticisi (AliBird.IconGenerator) AYNI kusu cizer.
public interface IKusCizici
{
    // k: kusun govdesinin alani. Gaga bu alanin sagindan disari %28 tasar.
    void Ciz(Graphics g, RectangleF k);
}
