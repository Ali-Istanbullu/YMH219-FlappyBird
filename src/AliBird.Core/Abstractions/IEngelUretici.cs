using AliBird.Core.Gameplay;

namespace AliBird.Core.Abstractions;

// Yeni bir Engel nesnesi ureten sozlesme.
// EngelAlani engellerini kendisi "new"lemez; bu arayuzden ister.
public interface IEngelUretici
{
    // x: engelin yatay konumu, boslukUstY: boslugun ust kenari
    Engel Olustur(float x, float boslukUstY);
}
