using System.Drawing.Imaging;
using AliBird.Core.Configuration;
using AliBird.IconGenerator;
using AliBird.WinForms.Rendering;

// ================================================================================
//  ALIBIRD IKON URETICI (Adim 10)
//  Kullanim (repo kokunden):  dotnet run --project src/AliBird.IconGenerator -- assets
//  Uretir: assets/alibird.png (256x256) ve assets/alibird.ico (16..256, 7 boyut)
//  Bu dosya aracin kompozisyon kokudur; nesneler burada baglanir (R2). DI kutuphanesine
//  gerek yok: 4 nesnelik bir arac (CK56).
// ================================================================================
try
{
    string klasor = Path.GetFullPath(args.Length > 0 ? args[0] : "assets");
    Directory.CreateDirectory(klasor);

    int[] boyutlar = [16, 24, 32, 48, 64, 128, 256];

    var ayarlar = new OyunAyarlari();
    using var kaynaklar = new CizimKaynaklari(Tema.Gece());   // oyunla ayni renkler (G1b)
    var cizici = new IkonCizici(new KusCizici(kaynaklar), kaynaklar, ayarlar);

    var resimler = new List<(int Boyut, byte[] Png)>();
    foreach (int boyut in boyutlar)
    {
        using Bitmap resim = cizici.Ciz(boyut);
        using var bellek = new MemoryStream();
        resim.Save(bellek, ImageFormat.Png);
        resimler.Add((boyut, bellek.ToArray()));
    }

    string pngYolu = Path.Combine(klasor, "alibird.png");
    File.WriteAllBytes(pngYolu, resimler[^1].Png);   // en buyugu (256)

    string icoYolu = Path.Combine(klasor, "alibird.ico");
    new IcoYazici().Yaz(icoYolu, resimler);

    Console.WriteLine($"Yazildi: {pngYolu}");
    Console.WriteLine($"Yazildi: {icoYolu} ({string.Join(", ", boyutlar)})");
    return 0;
}
catch (Exception hata)
{
    // R4: arac hata verirse nedeni yazilir, cikis kodu 1 olur.
    Console.Error.WriteLine($"Ikon uretilemedi: {hata}");
    return 1;
}
