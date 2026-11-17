namespace AliBird.WinForms.Rendering;

// Sanal oyun alanini pencereye oranli sigdirma hesabinin sozlesmesi (K5, K7, K65).
// AnaForm ve OyunCizici somut sinifi degil bu arayuzu bilir (D).
public interface IOlcekHesaplayici
{
    OlcekBilgisi Hesapla(Size pencere, SizeF sanalAlan);

    Rectangle SanaldanEkrana(RectangleF sanal, OlcekBilgisi bilgi);
}
