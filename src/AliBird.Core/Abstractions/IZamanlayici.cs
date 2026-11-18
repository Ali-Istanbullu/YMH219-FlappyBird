namespace AliBird.Core.Abstractions;

// Oyun dongusunun zamanlayicisi (PDF Timer istiyor).
// Core, WinForms'un Timer'ini dogrudan kullanamaz (Core hicbir projeye bagimli degil);
// bu yuzden presenter bu arayuzu kullanir. Uygulamasi: WinForms'taki FormsZamanlayici,
// icinde System.Windows.Forms.Timer calisir.
public interface IZamanlayici
{
    // Her "tik"te (her karede) tetiklenir; presenter bu olayda bir kare ilerletir.
    event EventHandler? Tik;

    // Iki tik arasindaki sure (milisaniye).
    int AralikMs { get; set; }

    void Baslat();

    void Durdur();
}
