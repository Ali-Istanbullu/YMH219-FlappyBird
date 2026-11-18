namespace AliBird.WinForms.Rendering;

// Olcekleme sonucu: sanal alani pencereye sigdirmak icin ne kadar buyutup nereye kaydiracagimiz.
public readonly record struct OlcekBilgisi(float Olcek, float KaydirmaX, float KaydirmaY);

// 640x480'lik SANAL oyun alanini, oran bozulmadan pencereye sigdirir.
// Pencere daha genisse sagda-solda, daha uzunsa altta-ustte bos kenar kalir.
// Boylece oyun her pencere boyutunda ayni zorlukta kalir (oyun mantigi hep 640x480'de calisir).
//
// Tek sorumlulugu: olcek ve kaydirma hesabi. Hicbir sey cizmez.
public sealed class OlcekHesaplayici : IOlcekHesaplayici
{
    public OlcekBilgisi Hesapla(Size pencere, SizeF sanalAlan)
    {
        // Genislige gore ve yukseklige gore ne kadar buyutulebilir? Kucuk olani secilir ki tasmasin.
        float olcek = Math.Min(pencere.Width / sanalAlan.Width, pencere.Height / sanalAlan.Height);
        olcek = Math.Max(olcek, 0.01f);   // pencere 0 boyuta kucultulurse bolme hatasi olmasin

        // Kalan boslugu iki yana esit dagit -> oyun alani ortada durur.
        float kaydirmaX = (pencere.Width - sanalAlan.Width * olcek) / 2f;
        float kaydirmaY = (pencere.Height - sanalAlan.Height * olcek) / 2f;

        return new OlcekBilgisi(olcek, kaydirmaX, kaydirmaY);
    }

    // Sanal alandaki bir dikdortgenin pencerede nereye denk geldigini hesaplar (buton yerlestirmek icin).
    public Rectangle SanaldanEkrana(RectangleF sanal, OlcekBilgisi bilgi) => Rectangle.Round(new RectangleF(
        bilgi.KaydirmaX + sanal.X * bilgi.Olcek,
        bilgi.KaydirmaY + sanal.Y * bilgi.Olcek,
        sanal.Width * bilgi.Olcek,
        sanal.Height * bilgi.Olcek));
}
