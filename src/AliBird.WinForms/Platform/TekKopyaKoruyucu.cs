namespace AliBird.WinForms.Platform;

// Oyunun ayni anda yalnizca TEK kopyasinin calismasini saglar (K17, M25b).
// Nasil? Windows'ta isimli bir "Mutex" (kilit) olusturur. Ilk acilan kopya kilidin sahibi olur;
// ikinci kopya ayni isimli kilidi zaten var bulur ve kapanir.
// Neden onemli? Iki kopya ayni anda ayni veritabanina ve log dosyasina yazmasin.
//
// R6: Kilit olusturulurken hata olursa oyun ENGELLENMEZ; ilk kopya kabul edilir ve devam edilir.
public sealed class TekKopyaKoruyucu : IDisposable
{
    private readonly Mutex? _kilit;
    private readonly bool _sahibiz;

    // ad: "Local\..." = sadece bu kullanicinin oturumunda gecerli kilit.
    public TekKopyaKoruyucu(string ad)
    {
        try
        {
            // Windows'un kendi kilit nesnesi; bu sinifin tek isi onu yonetmek.
            _kilit = new Mutex(initiallyOwned: true, name: ad, createdNew: out bool yeniOlusturuldu);
            _sahibiz = yeniOlusturuldu;
        }
        catch
        {
            // Kilit olusturulamadi (izin vb.): oyunu acmayi engellemeyelim (R6).
            _kilit = null;
            _sahibiz = true;
        }
    }

    // true: bu ilk kopya, oyun acilabilir. false: oyun zaten acik.
    public bool IlkKopyaMi => _sahibiz;

    public void Dispose()
    {
        try
        {
            if (_kilit is not null && _sahibiz)
            {
                _kilit.ReleaseMutex();
            }
            _kilit?.Dispose();
        }
        catch
        {
            // R6: kapanista bile exception disari cikmaz.
        }
    }
}
