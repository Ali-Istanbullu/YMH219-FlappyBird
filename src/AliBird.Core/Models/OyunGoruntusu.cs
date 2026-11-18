using System.Drawing;

namespace AliBird.Core.Models;

// Presenter'in (OyunYoneticisi) her karede Form'a verdigi "anlik fotograf".
// Form bu nesneye bakarak ekrani cizer; kendisi hicbir hesap yapmaz (pasif view).
// Tum koordinatlar 640x480'lik SANAL oyun alanina goredir;
// pencereye olcekleme isini Form tarafindaki OlcekHesaplayici yapar.
public sealed record OyunGoruntusu(
    // Oyunun o anki asamasi: baslangic yazisi mi, oyun mu, duraklatma mi, game over mi cizilecek.
    OyunDurumu Durum,

    // Kusun kapladigi dikdortgen.
    RectangleF KusAlani,

    // Ekrandaki engellerin listesi (her biri ust + alt parca).
    // IReadOnlyList: Form listeyi okuyabilir ama degistiremez.
    IReadOnlyList<EngelGoruntusu> Engeller,

    // Bu oyundaki guncel puan (sol ust).
    int Puan,

    // Kayitli en yuksek skor; hic yoksa null. Sol ustte tarihiyle gosterilir.
    RekorKaydi? Rekor,

    // Bu oyunda rekor kirildi mi? true ise "Yeni Rekor" vurgusu cizilir.
    bool YeniRekorMu,

    // Sag alttaki ses gostergesi icin.
    SesDurumu SesDurumu);
