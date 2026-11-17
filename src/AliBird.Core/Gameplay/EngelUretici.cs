using AliBird.Core.Abstractions;
using AliBird.Core.Configuration;

namespace AliBird.Core.Gameplay;

// IEngelUretici'nin uygulamasi (K65).
// Tek sorumlulugu: ayarlarla birlikte yeni bir Engel olusturmak (S).
// "new" kullanmasi bu yuzden serbest: nesne uretmek bu sinifin tek isi (fabrika, K60 ile ayni mantik).
public sealed class EngelUretici : IEngelUretici
{
    private readonly OyunAyarlari _ayarlar;

    public EngelUretici(OyunAyarlari ayarlar)
    {
        ArgumentNullException.ThrowIfNull(ayarlar);
        _ayarlar = ayarlar;
    }

    public Engel Olustur(float x, float boslukUstY) => new(_ayarlar, x, boslukUstY);
}
