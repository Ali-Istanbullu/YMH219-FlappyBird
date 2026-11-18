namespace AliBird.Core.Abstractions;

// Kayma hizinin OyunMotoru'na gorunen yuzu.
public interface IHizDenetcisi
{
    float Hiz { get; }

    // Hizi carpan kare kadar artirir (tavanda durur).
    void KareIlerlet(float carpan);
}
