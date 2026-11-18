namespace AliBird.WinForms.Rendering;

// Sanal oyun alanini pencereye oranli sigdirma hesabinin sozlesmesi.
// AnaForm ve OyunCizici somut sinifi degil bu arayuzu bilir.
public interface IOlcekHesaplayici
{
    OlcekBilgisi Hesapla(Size pencere, SizeF sanalAlan);

    Rectangle SanaldanEkrana(RectangleF sanal, OlcekBilgisi bilgi);
}
