using AliBird.Core.Abstractions;
using AliBird.Core.Models;

namespace AliBird.Infrastructure.Errors;

// IHataBildirici'nin gercek uygulamasi.
// Tek sorumlulugu: hatanin hangi kanala gidecegine karar vermek.
//   - HER hata loga yazilir (IGunlukcu)
//   - EkranVeGunluk ise kullaniciya da gosterilir (IKullaniciBildirici)
// Logu nasil yazacagini ya da mesaji nasil gosterecegini BILMEZ; ikisini de arayuzden alir (DIP).
//
// Butun catch bloklarinin son duragi burasi. Burasi exception firlatirsa uygulama coker;
//     bu yuzden bu sinifin hicbir yolu disari exception sizdirmaz.
public sealed class HataBildirici : IHataBildirici
{
    // kullanici mesaji bos gelirse gosterilecek genel metin.
    private const string VarsayilanKullaniciMesaji = "Beklenmeyen bir hata oluştu.";

    private readonly IGunlukcu _gunlukcu;
    private readonly IKullaniciBildirici _kullaniciBildirici;

    public HataBildirici(IGunlukcu gunlukcu, IKullaniciBildirici kullaniciBildirici)
    {
        ArgumentNullException.ThrowIfNull(gunlukcu);
        ArgumentNullException.ThrowIfNull(kullaniciBildirici);
        _gunlukcu = gunlukcu;
        _kullaniciBildirici = kullaniciBildirici;
    }

    public void Bildir(Exception hata, Gorunurluk gorunurluk, string kullaniciMesaji)
    {
        // Yanlis kullanimda bile cokmeyelim: hata nesnesi gelmediyse yerine bir tane uret.
        hata ??= new InvalidOperationException("Hata nesnesi verilmeden Bildir cagrildi.");

        bool mesajBos = string.IsNullOrWhiteSpace(kullaniciMesaji);

        // 1) Her durumda loga yaz.
        try
        {
            _gunlukcu.Hata(hata, mesajBos ? hata.Message : kullaniciMesaji);
        }
        catch
        {
            // log yazilamiyorsa sessizce devam.
        }

        // 2) Sadece EkranVeGunluk ise kullaniciya goster.
        if (gorunurluk != Gorunurluk.EkranVeGunluk)
        {
            return;
        }

        try
        {
            _kullaniciBildirici.MesajGoster(mesajBos ? VarsayilanKullaniciMesaji : kullaniciMesaji);
        }
        catch (Exception gostermeHatasi)
        {
            // mesaj kutusu acilamadiysa bu da loga yazilir, uygulama devam eder.
            try
            {
                _gunlukcu.Hata(gostermeHatasi, "Kullanıcıya hata mesajı gösterilemedi.");
            }
            catch
            {
                // burada da bir sey yapilamiyorsa sessizce devam.
            }
        }
    }
}
