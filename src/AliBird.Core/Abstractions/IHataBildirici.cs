using AliBird.Core.Models;

namespace AliBird.Core.Abstractions;

// Tum catch bloklarinin kullandigi TEK giris noktasi.
// Gorevi: hatayi HER ZAMAN loga yazmak, gerekiyorsa kullaniciya da gostermek.
// Kendisi log yazmayi da mesaj gostermeyi de bilmez; ikisini IGunlukcu ve
// IKullaniciBildirici'ye devreder (S: tek sorumluluk = hangi kanala gidecegine karar vermek).
public interface IHataBildirici
{
    // hata            : yakalanan exception (ayrintilar loga gider)
    // gorunurluk      : EkranVeGunluk mu SadeceGunluk mu
    // kullaniciMesaji : ekranda gosterilecek Turkce, anlasilir metin
    void Bildir(Exception hata, Gorunurluk gorunurluk, string kullaniciMesaji);
}
