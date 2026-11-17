using AliBird.Core.Abstractions;
using AliBird.Core.Configuration;
using AliBird.Core.Models;

namespace AliBird.Core.Gameplay;

// IOyunMotoruFabrikasi'nin gercek uygulamasi.
// Her oyuna ozel (durum tutan) parcalari burada "new" ile olusturur: Kus, EngelAlani, HizDenetcisi, PuanTablosu.
// Neden "new" burada serbest? Fabrikanin tek isi zaten nesne uretmek (S); diger siniflar hicbir
// yerde "new" kullanmadan parcalarini disaridan alir (R2). Durum tutmayan CarpismaDenetcisi ise
// DI'dan tek bir ornek olarak gelir ve her oyunda paylasilir.
public sealed class OyunMotoruFabrikasi : IOyunMotoruFabrikasi
{
    private readonly OyunAyarlari _ayarlar;
    private readonly IRastgele _rastgele;
    private readonly CarpismaDenetcisi _carpismaDenetcisi;

    public OyunMotoruFabrikasi(OyunAyarlari ayarlar, IRastgele rastgele, CarpismaDenetcisi carpismaDenetcisi)
    {
        ArgumentNullException.ThrowIfNull(ayarlar);
        ArgumentNullException.ThrowIfNull(rastgele);
        ArgumentNullException.ThrowIfNull(carpismaDenetcisi);
        _ayarlar = ayarlar;
        _rastgele = rastgele;
        _carpismaDenetcisi = carpismaDenetcisi;
    }

    public OyunMotoru Olustur(RekorKaydi? oncekiRekor)
    {
        return new OyunMotoru(
            new Kus(_ayarlar),
            new EngelAlani(_ayarlar, _rastgele),
            _carpismaDenetcisi,
            new HizDenetcisi(_ayarlar),
            new PuanTablosu(oncekiRekor));
    }
}
