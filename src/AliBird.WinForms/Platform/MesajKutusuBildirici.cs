using AliBird.Core.Abstractions;

namespace AliBird.WinForms.Platform;

// IKullaniciBildirici'nin WinForms uygulamasi: kullaniciya mesaj kutusu gosterir.
// HataBildirici "bu hata ekrana da gitsin" dediginde burasi calisir.
// Hata olursa HataBildirici yakalayip loga yazar; burada ayrica try/catch'e gerek yok.
public sealed class MesajKutusuBildirici : IKullaniciBildirici
{
    private const string Baslik = "AliBird";

    public void MesajGoster(string mesaj)
    {
        MessageBox.Show(mesaj, Baslik, MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
