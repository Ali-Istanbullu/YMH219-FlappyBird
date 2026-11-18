namespace AliBird.Core.Models;

// Sag alt kosedeki ses gostergesinin ne yazacagini belirler.
public enum SesDurumu
{
    // Ses calisiyor: gosterge bos kalir.
    Acik,

    // Kullanici M tusu ile sesi kapatti: gri "Ses kapali (M)" yazisi.
    Kapali,

    // Ses cihazi yok ya da ses calinamiyor: kirmizi "Ses yok" yazisi.
    Yok
}
