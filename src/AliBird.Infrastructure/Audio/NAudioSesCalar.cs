using AliBird.Core.Abstractions;
using AliBird.Core.Models;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace AliBird.Infrastructure.Audio;

// ISesCalar'in NAudio ile calisan uygulamasi (M14b, K13).
// Tek sorumlulugu: hazir sesleri ses kartina gondermek (S). Sesleri ISesUreteci uretir.
//
// Nasil calisir?
//   - Acilista dort ses BIR KEZ uretilip bellekte tutulur (her Cal'da yeniden hesaplanmaz).
//   - Bir "karistirici" (MixingSampleProvider) surekli ses kartina bagli calisir.
//   - Cal() karistiriciya yeni bir ses ekler; ayni anda birden cok ses ust uste calabilir (M14b).
//
// R6: Bu sinif ASLA exception firlatmaz.
//   - Ses cihazi yoksa / acilamazsa: KullanilabilirMi = false, oyun sessiz devam eder,
//     sag altta kirmizi "Ses yok" gorunur (K12, K13, M24 #6). Hata sadece loga yazilir.
//   - Oyun sirasinda cihaz cikarilirsa (kulaklik vb.): ayni sekilde "Ses yok"a gecilir.
public sealed class NAudioSesCalar : ISesCalar, IDisposable
{
    // Ses kartina giden HER bir tamponun suresi (ms). Kucuk = gecikme az.
    // (NAudio 3: eski DesiredLatency toplam sureydi; BufferMilliseconds tek tamponun suresidir.)
    private const int TamponMs = 50;

    private readonly IHataBildirici _hataBildirici;
    private readonly Dictionary<SesTuru, float[]> _sesler = [];
    private readonly MixingSampleProvider _karistirici;
    private readonly WaveOut? _cikis;

    // volatile: ses kartinin kendi is parcacigi (thread) da bu degeri degistirebilir.
    private volatile bool _kullanilabilir;

    public NAudioSesCalar(ISesUreteci uretici, IHataBildirici hataBildirici, IGunlukcu gunlukcu)
    {
        ArgumentNullException.ThrowIfNull(uretici);
        ArgumentNullException.ThrowIfNull(hataBildirici);
        ArgumentNullException.ThrowIfNull(gunlukcu);
        _hataBildirici = hataBildirici;

        // 32 bit ondalikli, tek kanal, 44100 Hz ses bicimi.
        WaveFormat bicim = WaveFormat.CreateIeeeFloatWaveFormat(uretici.OrnekHizi, 1);

        // ReadFully = true: hic ses yokken bile sessizlik vererek calmaya devam eder,
        // boylece her Cal()'da cihazi yeniden baslatmak gerekmez.
        _karistirici = new MixingSampleProvider(bicim) { ReadFully = true };

        try
        {
            // 1) Dort sesi bir kez uret ve sakla.
            foreach (SesTuru tur in Enum.GetValues<SesTuru>())
            {
                _sesler[tur] = uretici.Uret(tur);
            }

            // 2) Ses cihazini ac ve karistiriciyi calmaya basla.
            //    NAudio'nun kendi cikis nesnesi; bu sinifin tek isi NAudio'ya baglanmak oldugu icin burada kurulur.
            _cikis = new WaveOut { BufferMilliseconds = TamponMs };   // NAudio 3: eski adi WaveOutEvent
            _cikis.PlaybackStopped += CalmaDurdugunda;
            _cikis.Init(_karistirici);
            _cikis.Play();

            _kullanilabilir = true;
            gunlukcu.Bilgi("Ses cihazı hazır.");   // K63: ses cihazinin durumu
        }
        catch (Exception hata)
        {
            // Ses cihazi yok, surucu hatasi vb. -> sessiz mod (M24 #6, R6).
            _kullanilabilir = false;
            CikisiKapat(_cikis);
            _cikis = null;
            _hataBildirici.Bildir(hata, Gorunurluk.SadeceGunluk, string.Empty);
            gunlukcu.Uyari("Ses cihazı açılamadı, oyun sessiz devam ediyor.");   // K63
        }
    }

    public bool KullanilabilirMi => _kullanilabilir;

    public void Cal(SesTuru tur)
    {
        if (!_kullanilabilir)
        {
            return;
        }

        try
        {
            if (_sesler.TryGetValue(tur, out float[]? ornekler))
            {
                // Her calmada yeni bir saglayici: ayni ses ust uste binebilir.
                _karistirici.AddMixerInput(new DiziSesSaglayici(ornekler, _karistirici.WaveFormat));
            }
        }
        catch (Exception hata)
        {
            // R6: ses hatasi oyunu durdurmaz.
            _hataBildirici.Bildir(hata, Gorunurluk.SadeceGunluk, string.Empty);
        }
    }

    // Calma beklenmedik sekilde durdu (cihaz cikarildi vb.).
    // ReadFully = true oldugu icin normalde hic durmaz; durursa sebebi bir hatadir.
    private void CalmaDurdugunda(object? gonderen, StoppedEventArgs e)
    {
        if (e.Exception is null)
        {
            return;   // Dispose ile bilerek durduruldu
        }

        _kullanilabilir = false;   // sag altta "Ses yok" gorunsun (K12)
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

    // Cihazi durdurup birakir; hata olsa bile disari sizdirmaz (R6).
    private static void CikisiKapat(WaveOut? cikis)
    {
        try
        {
            cikis?.Stop();
            cikis?.Dispose();
        }
        catch
        {
            // R6: kapanista bile exception disari cikmaz.
        }
    }
}
