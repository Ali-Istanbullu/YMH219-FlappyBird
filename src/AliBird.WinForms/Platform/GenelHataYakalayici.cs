using AliBird.Core.Abstractions;
using AliBird.Core.Models;

namespace AliBird.WinForms.Platform;

// Uygulamanin "son savunma hatti" (R6, M24 #8, K61).
// Program.cs'ten ayrildi: Program sadece uygulamayi kurar; gozden kacan hatalari yakalamak bu sinifin isi (S).
//
// Neden DI'dan gelmiyor? Bu yakalayicilar DI kurulmadan ONCE devreye girmeli (DI kurulurken bile
// hata olabilir). Bu yuzden Program.cs (kompozisyon koku) onu ilk is olarak olusturur;
// hata bildirici hazir olunca BildiriciyiAta ile verilir (method injection).
public sealed class GenelHataYakalayici
{
    private const string Baslik = "AliBird";

    // DI kurulana kadar null. Kurulunca Program.cs atar.
    private IHataBildirici? _hataBildirici;

    // CK40: mesaj kutusu en fazla bir kez acilir; sonrakiler sadece loga gider.
    private bool _ekrandaGosterildi;

    // Yakalayicilari Windows/NET olaylarina baglar. Uygulamada HER SEYDEN ONCE cagrilmali.
    public void Kur()
    {
        // Arayuz is parcacigindaki yakalanmamis hatalar Application.ThreadException'a gelsin;
        // boylece uygulama kapanmaz, kullanici bilgilendirilir ve oyun devam eder.
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => Bildir(e.Exception);

        // Arka plan is parcaciklarindaki hatalar: .NET bu durumda uygulamayi kapatir;
        // en azindan loga yazilir (CK41).
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            Bildir(e.ExceptionObject as Exception ?? new InvalidOperationException("Bilinmeyen hata"));

        // Beklenmeyen (await edilmemis) Task hatalari: loga yaz ve "goruldu" isaretle ki surec kapanmasin.
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

            // CK40: ilk hata ekranda gosterilir, sonrakiler sadece loga (mesaj kutusu yagmuru olmasin).
            Gorunurluk gorunurluk = _ekrandaGosterildi ? Gorunurluk.SadeceGunluk : Gorunurluk.EkranVeGunluk;
            _ekrandaGosterildi = true;
            _hataBildirici.Bildir(hata, gorunurluk,
                "Beklenmeyen bir hata oluştu. Oyun çalışmaya devam ediyor; sorun sürerse oyunu yeniden başlatın.");
        }
        catch
        {
            // R6: son savunma hattinda da exception disari cikmaz.
        }
    }
}
