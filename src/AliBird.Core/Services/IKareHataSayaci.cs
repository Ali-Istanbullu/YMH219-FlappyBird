namespace AliBird.Core.Services;

// Ust uste olusan kare hatalarini sayan sozlesme (CK3, M24 #7 #8, K61).
// OyunYoneticisi'nden ayrildi: "kac hata oldu, ne zaman kullaniciya soylenecek" karari burada (S).
public interface IKareHataSayaci
{
    // Kare sorunsuz gecti: sayac sifirlanir.
    void BasariliKare();

    // Bir kare hatasi oldu. Hata loga yazilir.
    // true donerse: sinira ulasildi ve kullaniciya ilk kez soylendi -> oyun durdurulmali.
    bool HatayiKaydet(Exception hata);

    // Yeni oyun: sayac ve "gosterildi" bilgisi sifirlanir.
    void Sifirla();
}
