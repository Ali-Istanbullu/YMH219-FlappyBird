using System.Drawing;
using AliBird.Core.Gameplay;

namespace AliBird.Core.Abstractions;

// Carpisma kontrolu sozlesmesi (G3, K65).
// OyunMotoru ve fabrika somut CarpismaDenetcisi'ni degil bu arayuzu bilir (D).
public interface ICarpismaDenetcisi
{
    // Kusun carpisma kutusu zemine ya da bir boruya degiyor mu? true ise oyun bitmeli.
    bool CarptiMi(RectangleF kusKutusu, IEnumerable<Engel> engeller);
}
