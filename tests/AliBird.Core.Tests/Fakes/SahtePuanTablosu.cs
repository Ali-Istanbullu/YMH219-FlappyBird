using AliBird.Core.Abstractions;

namespace AliBird.Core.Tests.Fakes;

// Sadece eklenen puani toplar.
public sealed class SahtePuanTablosu : IPuanTablosu
{
    public int Puan { get; private set; }
    public bool YeniRekorMu => false;
    public bool KaydedilecekRekorVarMi => false;
    public void PuanEkle(int miktar) => Puan += miktar;
}
