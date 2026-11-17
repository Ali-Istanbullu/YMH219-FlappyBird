using AliBird.Core.Gameplay;

namespace AliBird.Core.Abstractions;

// Yeni bir Engel nesnesi ureten sozlesme (K65).
// EngelAlani engellerini kendisi "new"lemez; bu arayuzden ister (R2, D).
public interface IEngelUretici
{
    // x: engelin yatay konumu, boslukUstY: boslugun ust kenari
    Engel Olustur(float x, float boslukUstY);
}
