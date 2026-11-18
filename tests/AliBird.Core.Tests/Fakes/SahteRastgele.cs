using AliBird.Core.Abstractions;

namespace AliBird.Core.Tests.Fakes;

// Testler icin elle yazilmis sahte rastgele uretici (mock kutuphanesi yok).
// Her cagrida ayni, onceden bilinen sayiyi dondurur; boylece test sonucu her seferinde ayni olur.
// Ayrica son cagrida hangi sinirlarin istendigini saklar; testte bunu kontrol edebiliriz.
public sealed class SahteRastgele : IRastgele
{
    private readonly int _donecekDeger;

    public int SonEnAz { get; private set; }
    public int SonEnCok { get; private set; }
    public int CagrilmaSayisi { get; private set; }

    public SahteRastgele(int donecekDeger)
    {
        _donecekDeger = donecekDeger;
    }

    public int Sonraki(int enAz, int enCok)
    {
        SonEnAz = enAz;
        SonEnCok = enCok;
        CagrilmaSayisi++;
        return _donecekDeger;
    }
}
