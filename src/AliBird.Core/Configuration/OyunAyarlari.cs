namespace AliBird.Core.Configuration;

// Oyunun butun sayisal ayarlari tek yerde (D1b).
// Bu bir "record": DI container'a Singleton olarak kaydedilir ve ihtiyac duyan siniflar
// constructor'dan alir. Boylece testlerde farkli degerlerle yeni bir OyunAyarlari verilebilir
// (ornegin hiz = 0) ve hicbir sinifin kodunu degistirmek gerekmez (DIP + OCP).
//
// "init" : deger sadece nesne olusturulurken verilebilir, sonra degistirilemez.
// Varsayilan degerler Karar Defteri'ndeki onayli degerlerdir; kaynak her satirda yazili.
// Birimler: uzunluklar piksel (640x480 sanal alanda), hizlar piksel/kare.
public sealed record OyunAyarlari
{
    // ---- Sanal oyun alani (K5) ----
    public float AlanGenisligi { get; init; } = 640f;
    public float AlanYuksekligi { get; init; } = 480f;

    // Alttaki zemin seridinin yuksekligi; zemine degmek oyunu bitirir (K6, G3).
    public float ZeminYuksekligi { get; init; } = 40f;

    // ---- Kus ve fizik (K6) ----
    public float KusGenisligi { get; init; } = 30f;
    public float KusYuksekligi { get; init; } = 24f;

    // Kusun sabit yatay konumu: ekranin yaklasik 1/6'si (E1a).
    public float KusX { get; init; } = 100f;

    // Kusun oyun basindaki yuksekligi: oynanabilir alanin ortasi (E1a).
    // Hesap: (480 - 40 zemin) / 2 - 24 / 2 = 208
    public float KusBaslangicY { get; init; } = 208f;

    // Carpisma kutusu kusun cizilen dikdortgeninden her kenardan bu kadar kucuktur (E4b).
    // 30x24 kus -> 24x18 carpisma kutusu.
    public float CarpismaPayi { get; init; } = 3f;

    // Her karede dikey hiza eklenen yer cekimi (Chrome Dino: GRAVITY 0.6).
    public float YerCekimi { get; init; } = 0.6f;

    // Space'e basinca dikey hiz bu degere AYARLANIR (eklenmez). Eksi = yukari.
    // (Chrome Dino: INITIAL_JUMP_VELOCITY 12)
    public float ZiplamaHizi { get; init; } = -12f;

    // Kusun asagi dogru ulasabilecegi en yuksek hiz.
    public float MaksimumDusmeHizi { get; init; } = 12f;

    // ---- Engeller (K7) ----
    public float EngelGenisligi { get; init; } = 60f;

    // Ust ve alt boru arasindaki, kusun gectigi bosluk.
    public float BoslukYuksekligi { get; init; } = 150f;

    // Iki engel arasindaki yatay mesafe.
    public float EngelAraligi { get; init; } = 320f;

    // Boslugun tavana ve zemine en az bu kadar uzak olmasi gerekir.
    public float KenarPayi { get; init; } = 60f;

    // Ilk engelin oyun basladiginda bulundugu yatay konum (E5b).
    // Ekranin bir engel araligi kadar disinda baslar; hiz 6 ile kusa ~2,4 sn'de ulasir.
    public float IlkEngelX { get; init; } = 960f;

    // ---- Hiz (K8, Chrome offline.js: SPEED 6, MAX_SPEED 13, ACCELERATION 0.001) ----
    public float BaslangicHizi { get; init; } = 6f;
    public float MaksimumHiz { get; init; } = 13f;

    // Her karede hiza eklenen artis (skora degil kareye bagli).
    public float Ivme { get; init; } = 0.001f;

    // ---- Hesaplanan degerler (ayar degil, yukaridakilerden turetilir) ----

    // Zeminin ust kenarinin Y koordinati: 480 - 40 = 440. Kus bunun altina inerse oyun biter.
    public float ZeminUstY => AlanYuksekligi - ZeminYuksekligi;
}
