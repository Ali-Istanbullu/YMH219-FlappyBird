using System.Drawing;

namespace AliBird.Core.Models;

// Ekranda cizilecek TEK bir engelin (boru ciftinin) seklini tasir.
// Bir engel iki parcadir: ustten sarkan boru ve alttan yukselen boru;
// aradaki bosluktan kus gecer (K7).
//
// RectangleF: System.Drawing'in geometri tipi (sadece x, y, genislik, yukseklik).
// Core'da yalnizca bu tur geometri tiplerine izin var, Graphics/Bitmap yok (M8b).
public sealed record EngelGoruntusu(RectangleF UstAlan, RectangleF AltAlan);
