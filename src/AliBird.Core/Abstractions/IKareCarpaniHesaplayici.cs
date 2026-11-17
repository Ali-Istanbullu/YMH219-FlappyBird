namespace AliBird.Core.Abstractions;

// Gecen sureyi "kac kare ettigine" ceviren sozlesme (K68, delta time).
// Fizik degerleri 60 FPS icin (1 kare = 1/60 sn). 35 FPS'te bir tik ~1.7 kare eder.
public interface IKareCarpaniHesaplayici
{
    // gecenMs: iki tik arasi gercek sure. 0 ise (ilk kare) 1 kare sayilir.
    float Hesapla(double gecenMs);
}
