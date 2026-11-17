namespace AliBird.Infrastructure.Persistence;

// Bozuk veritabani dosyasini taniyip kurtaran sozlesme (M24 #4, CK23, K61).
// SqlitePuanDeposu'ndan ayrildi: depo sadece okur/yazar, kurtarma isini bu sinif yapar (S).
public interface IBozukVeritabaniKurtarici
{
    // Bu hata "dosya bozuk" turunden mi?
    bool BozukMu(Exception hata);

    // Bozuk dosyayi kenara alir (silmez), kullaniciya "rekor sifirlandi" der.
    // Bir sonraki erisimde yerine bos veritabani olusur. Asla exception firlatmaz (R6).
    void KenaraAl(Exception bozuklukHatasi);
}
