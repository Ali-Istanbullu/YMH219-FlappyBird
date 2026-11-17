using System.Runtime.InteropServices;
using AliBird.Core.Abstractions;

namespace AliBird.Infrastructure.Platform;

// IZamanlayiciCozunurlugu'nun Windows uygulamasi: winmm.dll timeBeginPeriod / timeEndPeriod.
// Microsoft belgesi (timeapi.h):
//  - timeBeginPeriod basarida TIMERR_NOERROR (0) dondurur.
//  - Her timeBeginPeriod ayni degerle bir timeEndPeriod ile eslenmelidir.
//  - Windows 10 2004'ten beri ayar global degil, sadece cagiran islemi etkiler.
//  - Windows 11'de pencere tamamen gizli/simge durumundaysa yuksek cozunurluk garanti edilmez
//    (o sirada oyun zaten duraklatilmis olur, K11).
// Tek sorumlulugu: cozunurlugu istemek ve kapanista geri vermek (S).
public sealed class WindowsZamanlayiciCozunurlugu : IZamanlayiciCozunurlugu
{
    // Istenen en kucuk zamanlayici adimi (ms).
    private const uint IstenenMs = 1;
    private const uint TimerrNoError = 0;

    private readonly IGunlukcu _gunlukcu;
    private bool _aktif;   // timeEndPeriod sadece basarili timeBeginPeriod'dan sonra cagrilir

    public WindowsZamanlayiciCozunurlugu(IGunlukcu gunlukcu)
    {
        ArgumentNullException.ThrowIfNull(gunlukcu);
        _gunlukcu = gunlukcu;
    }

    public void Artir()
    {
        if (_aktif)
        {
            return;   // iki kez istenmesin (esleme bozulmasin)
        }

        try
        {
            uint sonuc = timeBeginPeriod(IstenenMs);
            if (sonuc == TimerrNoError)
            {
                _aktif = true;
                _gunlukcu.Bilgi($"Zamanlayıcı çözünürlüğü {IstenenMs} ms olarak istendi.");
            }
            else
            {
                _gunlukcu.Uyari($"Zamanlayıcı çözünürlüğü artırılamadı (kod {sonuc}). Oyun varsayılan çözünürlükle çalışacak.");
            }
        }
        catch (Exception hata)
        {
            // winmm.dll bulunamazsa vb. (R4, R6): oyun yine calisir, sadece daha yavas.
            _gunlukcu.Hata(hata, "Zamanlayıcı çözünürlüğü artırılamadı.");
        }
    }

    public void Dispose()
    {
        if (!_aktif)
        {
            return;
        }
        _aktif = false;

        try
        {
            timeEndPeriod(IstenenMs);
        }
        catch
        {
            // Kapanis sirasinda; uygulama zaten cikiyor (R6).
        }
    }

    [DllImport("winmm.dll")]
    private static extern uint timeBeginPeriod(uint uPeriod);

    [DllImport("winmm.dll")]
    private static extern uint timeEndPeriod(uint uPeriod);
}
