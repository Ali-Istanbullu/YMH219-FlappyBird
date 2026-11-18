namespace AliBird.Core.Abstractions;

// Kullaniciya mesaj gosterme sozlesmesi.
// Ekranla ilgili oldugu icin uygulamasi WinForms tarafindadir (MesajKutusuBildirici).
// IHataBildirici bu arayuzu kullanir; boylece hata bildirici WinForms'a bagimli olmaz (DIP).
public interface IKullaniciBildirici
{
    void MesajGoster(string mesaj);
}
