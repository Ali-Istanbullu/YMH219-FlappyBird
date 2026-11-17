using System.Drawing;
using AliBird.Core.Configuration;
using AliBird.Core.Models;

namespace AliBird.Core.Gameplay;

// Tek bir engel: ustten sarkan boru + alttan yukselen boru, arada kusun gectigi bosluk (K7).
// Tek sorumlulugu: kendi konumunu ve "gecildi mi" bilgisini tutmak (S).
// Engeli ne zaman yeniden konumlayacagina EngelAlani karar verir.
public sealed class Engel
{
    private readonly OyunAyarlari _ayarlar;

    // Engelin sol kenarinin yatay konumu. Her karede hiz kadar azalir (sola kayar, G2).
    public float X { get; private set; }

    // Boslugun ust kenarinin Y koordinati. Rastgele secilir (K7).
    public float BoslukUstY { get; private set; }

    // Kus bu engeli gecti ve puan verildi mi? Ayni engel icin iki kez puan verilmesin diye.
    public bool GecildiMi { get; private set; }

    public Engel(OyunAyarlari ayarlar, float x, float boslukUstY)
    {
        ArgumentNullException.ThrowIfNull(ayarlar);
        _ayarlar = ayarlar;
        X = x;
        BoslukUstY = boslukUstY;
        GecildiMi = false;
    }

    // Engeli verilen hiz kadar sola kaydirir (G2).
    public void Kaydir(float hiz)
    {
        X -= hiz;
    }

    // Engelin sag kenari ekranin sol kenarini gecti mi? Gectiyse yeniden konumlanmali (G2).
    public bool EkrandanCiktiMi => X + _ayarlar.EngelGenisligi < 0f;

    // Ekrandan cikan engeli saga, yeni bir bosluk yuksekligiyle tasir.
    // Yeni nesne YARATILMAZ, ayni engel yeniden kullanilir (G2: "yeniden konumlandirilmali").
    public void YenidenKonumla(float yeniX, float yeniBoslukUstY)
    {
        X = yeniX;
        BoslukUstY = yeniBoslukUstY;
        GecildiMi = false;   // Yeni tur: bu engelden tekrar puan alinabilir.
    }

    // Kusun sol kenari engelin sag kenarini gectiyse engeli "gecildi" isaretler (E3a).
    // Sadece ILK gecis aninda true doner; sonraki karelerde false doner (puan bir kez artsin).
    public bool GecisiKontrolEt(float kusSolKenari)
    {
        if (GecildiMi)
        {
            return false;
        }

        float engelSagKenari = X + _ayarlar.EngelGenisligi;
        if (kusSolKenari > engelSagKenari)
        {
            GecildiMi = true;
            return true;
        }

        return false;
    }

    // Ust boru: ekranin tepesinden (y = 0) boslugun ust kenarina kadar.
    public RectangleF UstAlan => new(X, 0f, _ayarlar.EngelGenisligi, BoslukUstY);

    // Alt boru: boslugun alt kenarindan zeminin ust kenarina kadar.
    public RectangleF AltAlan
    {
        get
        {
            float altBoruUstY = BoslukUstY + _ayarlar.BoslukYuksekligi;
            return new RectangleF(X, altBoruUstY, _ayarlar.EngelGenisligi, _ayarlar.ZeminUstY - altBoruUstY);
        }
    }

    // Form'a gidecek cizim verisi (K37).
    public EngelGoruntusu Goruntu => new(UstAlan, AltAlan);
}
