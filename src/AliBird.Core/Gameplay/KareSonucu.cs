namespace AliBird.Core.Gameplay;

// OyunMotoru.Ilerlet(carpan) her karede bu kucuk sonucu dondurur.
// Presenter buna bakarak hangi sesi calacagina ve oyunun bitip bitmedigine karar verir.
// "readonly record struct": kucuk, degistirilemez, her karede olusturulsa da bellege yuk olmaz.
//
// KazanilanPuan : bu karede gecilen engel sayisi (genelde 0 ya da 1)
// Carpti        : bu karede kus bir seye carpti mi (true ise oyun bitti)
public readonly record struct KareSonucu(int KazanilanPuan, bool Carpti);
