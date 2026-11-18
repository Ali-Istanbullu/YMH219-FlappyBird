using AliBird.Core.Abstractions;
using AliBird.Core.Configuration;

namespace AliBird.Core.Gameplay;

// IEngelUretici'nin uygulamasi.
// Tek sorumlulugu: ayarlarla birlikte yeni bir Engel olusturmak.
// "new" kullanmasi bu yuzden serbest: nesne uretmek bu sinifin tek isi (fabrika).
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
