namespace AliBird.Core.Abstractions;

// Kayma hizinin OyunMotoru'na gorunen yuzu (K66, K8).
public interface IHizDenetcisi
{
    float Hiz { get; }

    // Hizi bir kare icin artirir (tavanda durur).
    void KareIlerlet();
}
