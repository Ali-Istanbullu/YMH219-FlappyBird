using AliBird.Core.Abstractions;

namespace AliBird.WinForms.Platform;

// IKullaniciBildirici'nin WinForms uygulamasi: kullaniciya mesaj kutusu gosterir (M22).
// HataBildirici "bu hata ekrana da gitsin" dediginde burasi calisir (K14, M24).
// Hata olursa HataBildirici yakalayip loga yazar (CK18); burada ayrica try/catch'e gerek yok.
public sealed class MesajKutusuBildirici : IKullaniciBildirici
{
    private const string Baslik = "AliBird";

    public void MesajGoster(string mesaj)
    {
        MessageBox.Show(mesaj, Baslik, MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
