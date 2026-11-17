using System.Drawing;
using AliBird.Core.Abstractions;
using AliBird.Core.Models;

namespace AliBird.Core.Gameplay;

// TEK bir oyunun (bir turun) motoru. Her "Yeniden Baslat"ta fabrikadan yenisi alinir (K19);
// boylece onceki oyundan kalan hicbir deger yeni oyuna tasinmaz.
//
// Tek sorumlulugu: bir kareyi isletmek, yani parcalari dogru sirayla calistirmak (S).
// Kendisi fizik, carpisma ya da puan hesabi YAPMAZ; bunlari ilgili siniflara devreder.
// Parcalarini kendisi "new"lemez; hepsi constructor'dan ARAYUZ olarak gelir (R2, DIP, K66).
// Boylece testte gercek fizik yerine sahte parcalar verilebilir.
public sealed class OyunMotoru
{
    private readonly IKus _kus;
    private readonly IEngelAlani _engelAlani;
    private readonly ICarpismaDenetcisi _carpismaDenetcisi;
    private readonly IHizDenetcisi _hizDenetcisi;
    private readonly IPuanTablosu _puanTablosu;

    public OyunMotoru(
        IKus kus,
        IEngelAlani engelAlani,
        ICarpismaDenetcisi carpismaDenetcisi,
        IHizDenetcisi hizDenetcisi,
        IPuanTablosu puanTablosu)
    {
        // Eksik parca varsa motor calisamaz: hemen ve acik bir hata ver.
        ArgumentNullException.ThrowIfNull(kus);
        ArgumentNullException.ThrowIfNull(engelAlani);
        ArgumentNullException.ThrowIfNull(carpismaDenetcisi);
        ArgumentNullException.ThrowIfNull(hizDenetcisi);
        ArgumentNullException.ThrowIfNull(puanTablosu);

        _kus = kus;
        _engelAlani = engelAlani;
        _carpismaDenetcisi = carpismaDenetcisi;
        _hizDenetcisi = hizDenetcisi;
        _puanTablosu = puanTablosu;
    }

    // Kus carpti mi? true olduktan sonra motor artik hicbir sey yapmaz.
    public bool BittiMi { get; private set; }

    // ---- Disariya (presenter'a) sadece okunabilir bilgiler ----
    public int Puan => _puanTablosu.Puan;
    public bool YeniRekorMu => _puanTablosu.YeniRekorMu;
    public bool KaydedilecekRekorVarMi => _puanTablosu.KaydedilecekRekorVarMi;
    public float Hiz => _hizDenetcisi.Hiz;
    public RectangleF KusAlani => _kus.Alan;

    // Her engelin cizim verisi (ust + alt boru). Form'a gidecek OyunGoruntusu icin.
    public IReadOnlyList<EngelGoruntusu> EngelGoruntuleri =>
        _engelAlani.Engeller.Select(engel => engel.Goruntu).ToList();

    // Space: kusu ziplatir. Oyun bittiyse etkisizdir.
    public void Zipla()
    {
        if (BittiMi)
        {
            return;
        }
        _kus.Zipla();
    }

    // Bir kare ilerlet. Sira onemlidir:
    //  1) kus hareket eder (yer cekimi)
    //  2) engeller guncel hizla sola kayar
    //  3) gecilen engel varsa puan artar
    //  4) carpisma kontrol edilir
    //  5) hiz bir sonraki kare icin artar (K8)
    public KareSonucu Ilerlet()
    {
        if (BittiMi)
        {
            return new KareSonucu(0, false);
        }

        _kus.Guncelle();
        _engelAlani.Guncelle(_hizDenetcisi.Hiz);

        // E3a: kusun sol kenari (X) engelin sag kenarini gecince puan.
        int kazanilanPuan = _engelAlani.YeniGecilenleriSay(_kus.X);
        if (kazanilanPuan > 0)
        {
            _puanTablosu.PuanEkle(kazanilanPuan);
        }

        bool carpti = _carpismaDenetcisi.CarptiMi(_kus.CarpismaKutusu, _engelAlani.Engeller);
        if (carpti)
        {
            BittiMi = true;   // G3: oyun durur
        }

        _hizDenetcisi.KareIlerlet();

        return new KareSonucu(kazanilanPuan, carpti);
    }
}
