using AliBird.Core.Models;

namespace AliBird.WinForms.Rendering;

// Ekranin bir "katmanini" cizen sozlesme (K65).
// OyunCizici katmanlari sirayla cagirir: sahne -> bilgi yazilari -> durum ekrani.
// Yeni bir katman (ornegin bulutlar) eklemek icin mevcut kod degismez; yeni bir sinif yazilip
// Program.cs'te kaydedilir (O: genisletmeye acik, degisiklige kapali).
//
// Katmanlar 640x480 SANAL alanda cizer; olcekleme OyunCizici'de yapilmis olur.
public interface IKatmanCizici
{
    void Ciz(Graphics g, OyunGoruntusu goruntu);
}
