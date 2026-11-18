using AliBird.Core.Abstractions;
using AliBird.Core.Models;

namespace AliBird.WinForms.Platform;

// Uygulamanin "son savunma hatti": kodumuzdaki (bug) ve gozden kacan hatalar buraya duser.
// Program.cs'ten ayrildi: Program sadece uygulamayi kurar; gozden kacan hatalari yakalamak bu sinifin isi.
//
// Neden DI'dan gelmiyor? Bu yakalayicilar DI kurulmadan ONCE devreye girmeli (DI kurulurken bile
// hata olabilir). Bu yuzden Program.cs (kompozisyon koku) onu ilk is olarak olusturur;
// hata bildirici hazir olunca BildiriciyiAta ile verilir (method injection).
public sealed class GenelHataYakalayici
{
    private const string Baslik = "AliBird";

    // DI kurulana kadar null. Kurulunca Program.cs atar.
    private IHataBildirici? _hataBildirici;

    // Mesaj kutusu en fazla bir kez acilir; sonrakiler sadece loga gider.
    private bool _ekrandaGosterildi;

    // Ayni hata her karede (saniyede ~60 kez) tekrar ederse log dolmasin: art arda ayni hata bir kez yazilir.
    private string? _sonHata;

    // Yakalayicilari Windows/NET olaylarina baglar. Uygulamada HER SEYDEN ONCE cagrilmali.
    public void Kur()
    {
        // Arka plan is parcaciklarindaki ve her turlu yakalanmamis hata en azindan loga yazilsin.
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            Bildir(e.ExceptionObject as Exception ?? new InvalidOperationException("Bilinmeyen hata"));

        if (DerlemeModu.HataAyiklamaMi)
        {
            // Debug: arayuz hatalari YAKALANMAZ. Debugger hatanin oldugu satirda durur.
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
            return;
        }

        // Release: arayuz is parcacigindaki hata oyunu kapatmaz; loga yazilir, kullanici bilgilendirilir.
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => Bildir(e.Exception);

        // Beklenmeyen (await edilmemis) Task hatalari: loga yaz ve "goruldu" isaretle.
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Bildir(e.Exception);
            e.SetObserved();
        };
    }

    // DI hazir olunca Program.cs cagirir.
    public void BildiriciyiAta(IHataBildirici hataBildirici)
    {
        ArgumentNullException.ThrowIfNull(hataBildirici);
        _hataBildirici = hataBildirici;
    }

    private void Bildir(Exception hata)
    {
        try
        {
            if (_hataBildirici is null)
            {
                // DI henuz kurulmadi: dogrudan mesaj kutusu.
                MessageBox.Show("Beklenmeyen bir hata oluştu.", Baslik, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string hataOzeti = hata.ToString();
            if (hataOzeti == _sonHata)
            {
                return;
            }
            _sonHata = hataOzeti;

            // Ilk hata ekranda gosterilir, sonrakiler sadece loga (mesaj kutusu yagmuru olmasin).
            Gorunurluk gorunurluk = _ekrandaGosterildi ? Gorunurluk.SadeceGunluk : Gorunurluk.EkranVeGunluk;
            _ekrandaGosterildi = true;
            _hataBildirici.Bildir(hata, gorunurluk,
                "Beklenmeyen bir hata oluştu. Oyun çalışmaya devam ediyor; sorun sürerse oyunu yeniden başlatın.");
        }
        catch
        {
            // bilerek bos: son savunma hattinda hata cikarsa gidecek baska yer yok.
        }
    }
}
