namespace AliBird.Infrastructure.Persistence;

// Bozuk veritabani dosyasini taniyip kurtaran sozlesme.
// SqlitePuanDeposu'ndan ayrildi: depo sadece okur/yazar, kurtarma isini bu sinif yapar.
public interface IBozukVeritabaniKurtarici
{
    // Bu hata "dosya bozuk" turunden mi?
    bool BozukMu(Exception hata);

    // Bozuk dosyayi kenara alir (silmez), kullaniciya "rekor sifirlandi" der.
    // Bir sonraki erisimde yerine bos veritabani olusur. Asla exception firlatmaz.
    void KenaraAl(Exception bozuklukHatasi);
}
