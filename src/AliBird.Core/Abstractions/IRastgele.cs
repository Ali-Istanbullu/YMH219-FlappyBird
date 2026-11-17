namespace AliBird.Core.Abstractions;

// Rastgele sayi uretme sozlesmesi (bosluk yuksekligi rastgele, K7).
// Testte hep ayni sayiyi veren sahte bir sinif verip sonucu onceden bilebilmek icin (DIP).
// Uygulamasi: SistemRastgele (Adim 5).
public interface IRastgele
{
    // enAz dahil, enCok HARIC bir tam sayi dondurur (System.Random.Next ile ayni kural).
    int Sonraki(int enAz, int enCok);
}
