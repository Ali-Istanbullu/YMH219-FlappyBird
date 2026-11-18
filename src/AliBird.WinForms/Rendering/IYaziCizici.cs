namespace AliBird.WinForms.Rendering;

// Golgeli yazi yazma sozlesmesi. Uc katman da ayni sekilde yazi yazdigi icin tek yerde.
public interface IYaziCizici
{
    // bicim null ise metin konumun sagina dogru yazilir; ortalamak/saga yaslamak icin bicim verilir.
    void Yaz(Graphics g, string metin, Font yaziTipi, Brush firca, PointF konum, StringFormat? bicim = null);
}
