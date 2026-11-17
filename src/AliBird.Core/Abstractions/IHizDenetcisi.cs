namespace AliBird.Core.Abstractions;

// Kayma hizinin OyunMotoru'na gorunen yuzu (K66, K8).
public interface IHizDenetcisi
{
    float Hiz { get; }

    // Hizi carpan kare kadar artirir (tavanda durur, K68).
    void KareIlerlet(float carpan);
}
