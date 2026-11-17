namespace AliBird.Core.Abstractions;

// Log (gunluk) yazma sozlesmesi. Core, Serilog'u bilmez; sadece bu arayuzu bilir (DIP).
// Gercek uygulamasi: Infrastructure'daki SerilogGunlukcu (Adim 5).
// Her kayit tarih ve saatle yazilir (K14); bunu uygulama sinifi saglar.
public interface IGunlukcu
{
    // Bilgi amacli kayit (ornek: "veritabani yeniden olusturuldu", M24 #3).
    void Bilgi(string mesaj);

    // Sorun olmayan ama dikkat edilmesi gereken durum.
    void Uyari(string mesaj);

    // Bir hata olustu: hata nesnesi (yigin izi dahil) ve aciklama loga yazilir.
    void Hata(Exception hata, string mesaj);
}
