namespace AliBird.WinForms.Rendering;

// Ekrandaki sabit yerlesimler (640x480 SANAL alanda).
// Bagimlilik degil, sabit deger: hem cizici hem form ayni konumu kullansin diye tek yerde (S).
public static class EkranYerlesimi
{
    // Oyun Bitti ekranindaki "Yeniden Baslat" butonunun yeri (CK32).
    // DurumKatmaniCizici yazilari buna gore dizer, AnaForm gercek butonu buraya koyar.
    public static readonly RectangleF YenidenBaslatButonAlani = new(245f, 270f, 150f, 40f);
}
