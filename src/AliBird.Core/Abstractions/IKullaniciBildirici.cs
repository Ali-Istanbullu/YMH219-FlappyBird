namespace AliBird.Core.Abstractions;

// Kullaniciya mesaj gosterme sozlesmesi (M22).
// Ekranla ilgili oldugu icin uygulamasi WinForms tarafindadir (MesajKutusuBildirici, Adim 8).
// IHataBildirici bu arayuzu kullanir; boylece hata bildirici WinForms'a bagimli olmaz (DIP).
public interface IKullaniciBildirici
{
    void MesajGoster(string mesaj);
}
