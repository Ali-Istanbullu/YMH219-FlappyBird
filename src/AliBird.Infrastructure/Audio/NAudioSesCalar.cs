using AliBird.Core.Abstractions;
using AliBird.Core.Models;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace AliBird.Infrastructure.Audio;

// ISesCalar'in NAudio ile calisan uygulamasi.
// Tek sorumlulugu: hazir sesleri ses kartina gondermek. Sesleri ISesUreteci uretir.
//
// Nasil calisir?
//   - Acilista dort ses BIR KEZ uretilip bellekte tutulur (her Cal'da yeniden hesaplanmaz).
//   - Bir "karistirici" (MixingSampleProvider) surekli ses kartina bagli calisir.
//   - Cal() karistiriciya yeni bir ses ekler; ayni anda birden cok ses ust uste calabilir.
//
// Beklenen hatalar (ses cihazi):
//   - Ses cihazi yoksa / acilamazsa: KullanilabilirMi = false, oyun sessiz devam eder,
//     sag altta kirmizi "Ses yok" gorunur. Hata sadece loga yazilir.
//   - Oyun sirasinda cihaz cikarilirsa (kulaklik vb.): ayni sekilde "Ses yok"a gecilir.
public sealed class NAudioSesCalar : ISesCalar, IDisposable
{
    // Ses kartina giden HER bir tamponun suresi (ms). Kucuk = gecikme az.
    // (NAudio 3: eski DesiredLatency toplam sureydi; BufferMilliseconds tek tamponun suresidir.)
    private const int TamponMs = 50;

    private readonly IHataBildirici _hataBildirici;
    private readonly ISesSaglayiciUretici _saglayiciUretici;
    private readonly Dictionary<SesTuru, float[]> _sesler = [];
    private readonly MixingSampleProvider _karistirici;
    private readonly WaveOut? _cikis;

    // volatile: ses kartinin kendi is parcacigi (thread) da bu degeri degistirebilir.
    private volatile bool _kullanilabilir;

    public NAudioSesCalar(ISesUreteci uretici, ISesSaglayiciUretici saglayiciUretici, IHataBildirici hataBildirici, IGunlukcu gunlukcu)
    {
        ArgumentNullException.ThrowIfNull(uretici);
        ArgumentNullException.ThrowIfNull(saglayiciUretici);
        ArgumentNullException.ThrowIfNull(hataBildirici);
        ArgumentNullException.ThrowIfNull(gunlukcu);
        _saglayiciUretici = saglayiciUretici;
        _hataBildirici = hataBildirici;

        // 32 bit ondalikli, tek kanal, 44100 Hz ses bicimi.
        WaveFormat bicim = WaveFormat.CreateIeeeFloatWaveFormat(uretici.OrnekHizi, 1);

        // ReadFully = true: hic ses yokken bile sessizlik vererek calmaya devam eder,
        // boylece her Cal()'da cihazi yeniden baslatmak gerekmez.
        _karistirici = new MixingSampleProvider(bicim) { ReadFully = true };

        // Dort sesi bir kez uret ve sakla. Saf hesap: hata olursa bu bir bug'dir, yakalanmaz.
        foreach (SesTuru tur in Enum.GetValues<SesTuru>())
        {
            _sesler[tur] = uretici.Uret(tur);
        }

        try
        {
            // Ses cihazini ac ve karistiriciyi calmaya basla.
            // Buradaki her hata disaridan gelir (ses cihazi yok, surucu hatasi): beklenen durum.
            _cikis = new WaveOut { BufferMilliseconds = TamponMs };   // NAudio 3: eski adi WaveOutEvent
            _cikis.PlaybackStopped += CalmaDurdugunda;
            _cikis.Init(_karistirici);
            _cikis.Play();

            _kullanilabilir = true;
            gunlukcu.Bilgi("Ses cihazı hazır.");
        }
        catch (Exception hata)
        {
            // Sessiz mod: sag altta "Ses yok" gorunur, oyun devam eder.
            _kullanilabilir = false;
            CikisiKapat(_cikis);
            _cikis = null;
            _hataBildirici.Bildir(hata, Gorunurluk.SadeceGunluk, string.Empty);
            gunlukcu.Uyari("Ses cihazı açılamadı, oyun sessiz devam ediyor.");
        }
    }

    public bool KullanilabilirMi => _kullanilabilir;

    public void Cal(SesTuru tur)
    {
        if (!_kullanilabilir)
        {
            return;
        }

        // Sadece bellekteki karistiriciya eklenir; cihaza dogrudan dokunulmaz.
        // Her calmada yeni bir saglayici: ayni ses ust uste binebilir.
        _karistirici.AddMixerInput(_saglayiciUretici.Olustur(_sesler[tur], _karistirici.WaveFormat));
    }

    // Calma beklenmedik sekilde durdu (cihaz cikarildi vb.).
    // ReadFully = true oldugu icin normalde hic durmaz; durursa sebebi bir hatadir.
    private void CalmaDurdugunda(object? gonderen, StoppedEventArgs e)
    {
        if (e.Exception is null)
        {
            return;   // Dispose ile bilerek durduruldu
        }

        _kullanilabilir = false;   // sag altta "Ses yok" gorunsun
        _hataBildirici.Bildir(e.Exception, Gorunurluk.SadeceGunluk, string.Empty);
    }

    public void Dispose()
    {
        _kullanilabilir = false;
        if (_cikis is not null)
        {
            _cikis.PlaybackStopped -= CalmaDurdugunda;
        }
        CikisiKapat(_cikis);
    }

    // Cihazi durdurup birakir. Cihaz zaten kopmus olabilir; kapanista bu hata onemsizdir.
    private static void CikisiKapat(WaveOut? cikis)
    {
        try
        {
            cikis?.Stop();
            cikis?.Dispose();
        }
        catch (Exception)
        {
            // bilerek bos: kopmus cihazi kapatirken gelen hata kapanisi engellemesin.
        }
    }
}
