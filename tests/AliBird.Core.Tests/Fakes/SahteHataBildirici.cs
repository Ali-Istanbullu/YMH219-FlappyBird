using AliBird.Core.Abstractions;
using AliBird.Core.Models;

namespace AliBird.Core.Tests.Fakes;

// Bildirilen hatalari listeye yazar; testte kac hatanin hangi gorunurlukle bildirildigine bakariz.
public sealed class SahteHataBildirici : IHataBildirici
{
    public List<(Exception Hata, Gorunurluk Gorunurluk, string Mesaj)> Bildirimler { get; } = [];

    public int EkranaGidenSayisi => Bildirimler.Count(b => b.Gorunurluk == Gorunurluk.EkranVeGunluk);

    public void Bildir(Exception hata, Gorunurluk gorunurluk, string kullaniciMesaji)
        => Bildirimler.Add((hata, gorunurluk, kullaniciMesaji));
}
