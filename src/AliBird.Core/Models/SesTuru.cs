namespace AliBird.Core.Models;

// Oyunda calinabilecek 4 ayri sesin listesi.
// ISesCalar.Cal(...) metoduna hangi sesin calinacagini bu enum ile soyleriz.
public enum SesTuru
{
    // Space ile kus zipladiginda.
    Zipla,

    // Bir engel basariyla gecildiginde, puan arttiginda.
    Puan,

    // Kus engele ya da zemine carptigi an.
    Carpma,

    // Carpmanin hemen ardindan, oyun bitti ekrani gelirken (carpma sesinden ayri).
    OyunBitti
}
