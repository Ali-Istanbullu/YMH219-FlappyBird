namespace AliBird.Core.Abstractions;

// Bu oyunun puaninin OyunMotoru'na gorunen yuzu (K66, K9, K10).
public interface IPuanTablosu
{
    int Puan { get; }

    // Ekranda "YENI REKOR!" vurgusu cikmali mi?
    bool YeniRekorMu { get; }

    // Oyun bitince rekor kaydedilmeli mi?
    bool KaydedilecekRekorVarMi { get; }

    void PuanEkle(int miktar);
}
