namespace AliBird.Core.Abstractions;

// Oyun dongusunun zamanlayicisi (G8: PDF Timer istiyor).
// Core, WinForms'un Timer'ini dogrudan kullanamaz (Core hicbir projeye bagimli degil);
// bu yuzden presenter bu arayuzu kullanir. Uygulamasi: WinForms'taki FormsZamanlayici (Adim 8),
// icinde System.Windows.Forms.Timer calisir (M16a).
public interface IZamanlayici
{
    // Her "tik"te (her karede) tetiklenir; presenter bu olayda bir kare ilerletir.
    event EventHandler? Tik;

    // Iki tik arasindaki sure (milisaniye). Degeri Adim 9'da olcumle belirlenecek.
    int AralikMs { get; set; }

    void Baslat();

    void Durdur();
}
