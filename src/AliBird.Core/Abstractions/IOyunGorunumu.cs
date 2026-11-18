using AliBird.Core.Models;

namespace AliBird.Core.Abstractions;

// MVP'deki "View" sozlesmesi. AnaForm (WinForms) bu arayuzu uygular.
// Form PASIFTIR: sadece ham olaylari haber verir ve kendisine verileni cizer.
// Space'in o an "basla" mi "zipla" mi "yeniden basla" mi demek oldugunu
// Form bilmez; bunu OyunYoneticisi oyunun durumuna bakarak yorumlar.
public interface IOyunGorunumu
{
    // Space tusuna basildi.
    event EventHandler? BoslukBasildi;

    // P tusuna basildi (duraklat / devam).
    event EventHandler? PBasildi;

    // M tusuna basildi (sesi ac / kapat).
    event EventHandler? MBasildi;

    // Game Over ekranindaki "Yeniden Baslat" butonuna tiklandi.
    event EventHandler? YenidenBaslatTiklandi;

    // Pencere kucultuldu ya da baska pencereye gecildi; presenter oyunu duraklatir.
    event EventHandler? OdakKaybedildi;

    // Presenter her karede guncel goruntuyu bu metotla Form'a verir; Form ekrani yeniden cizer.
    void Goster(OyunGoruntusu goruntu);
}
