namespace AliBird.Core.Models;

// Oyunun o anda hangi asamada oldugunu gosteren durum listesi (enum).
// OyunYoneticisi (presenter) Space tusunun ne yapacagina bu duruma bakarak karar verir (K11).
public enum OyunDurumu
{
    // Oyun acildi, "baslamak icin tusa bas" yazisi ekranda bekliyor.
    Hazir,

    // Oyun oynaniyor: kus ucuyor, engeller kayiyor, timer calisiyor.
    Calisiyor,

    // P tusu ya da pencere odagini kaybetmesi ile oyun dondu (K11).
    Duraklatildi,

    // Kus engele veya zemine carpti (G3). Space ya da buton ile yeniden baslanir (G5).
    OyunBitti
}
