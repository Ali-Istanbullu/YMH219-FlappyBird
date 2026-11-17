namespace AliBird.Core.Models;

// Bir hatanin kullaniciya gosterilip gosterilmeyecegini belirtir (K14, M24 hata haritasi).
// Her catch blogu IHataBildirici.Bildir(...) cagirirken bu degerlerden birini secer.
// Iki durumda da hata MUTLAKA log dosyasina yazilir.
public enum Gorunurluk
{
    // Hata hem ekranda kullaniciya gosterilir hem de loga yazilir.
    EkranVeGunluk,

    // Hata sadece loga yazilir, kullanici hicbir sey gormez.
    SadeceGunluk
}
