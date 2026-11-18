namespace AliBird.WinForms.Forms;

// Klavye odagini HIC almayan buton.
// Neden? Normal bir buton odaktayken Space'e basilirsa butona tiklanmis sayilir.
// Oyun Bitti ekraninda Space'in 750 ms beklemesi bu yolla atlanmasin diye
// buton sadece fareyle tiklanabilir; Space her zaman forma gider.
public sealed class OdakAlmayanButon : Button
{
    public OdakAlmayanButon()
    {
        SetStyle(ControlStyles.Selectable, false);
        TabStop = false;
    }
}
