namespace AliBird.Core.Configuration;

// Oyunun butun sayisal ayarlari tek yerde.
// Bu bir "record": DI container'a Singleton olarak kaydedilir ve ihtiyac duyan siniflar
// constructor'dan alir. Boylece testlerde farkli degerlerle yeni bir OyunAyarlari verilebilir
// (ornegin hiz = 0) ve hicbir sinifin kodunu degistirmek gerekmez (DIP + OCP).
//
// "init" : deger sadece nesne olusturulurken verilebilir, sonra degistirilemez.
// Varsayilan degerler Karar Defteri'ndeki onayli degerlerdir; kaynak her satirda yazili.
// Birimler: uzunluklar piksel (640x480 sanal alanda), hizlar piksel/kare.
public sealed record OyunAyarlari
{
    // ---- Sanal oyun alani ----
    public float AlanGenisligi { get; init; } = 640f;
    public float AlanYuksekligi { get; init; } = 480f;

    // Alttaki zemin seridinin yuksekligi; zemine degmek oyunu bitirir.
    public float ZeminYuksekligi { get; init; } = 40f;

    // ---- Kus ve fizik ----
    public float KusGenisligi { get; init; } = 30f;
    public float KusYuksekligi { get; init; } = 24f;

    // Kusun sabit yatay konumu: ekranin yaklasik 1/6'si.
    public float KusX { get; init; } = 100f;

    // Kusun oyun basindaki yuksekligi: oynanabilir alanin ortasi.
    // Hesap: (480 - 40 zemin) / 2 - 24 / 2 = 208
    public float KusBaslangicY { get; init; } = 208f;

    // Carpisma kutusu kusun cizilen dikdortgeninden her kenardan bu kadar kucuktur.
    // 30x24 kus -> 24x18 carpisma kutusu.
    public float CarpismaPayi { get; init; } = 3f;

    // Her karede dikey hiza eklenen yer cekimi (Chrome Dino: GRAVITY 0.6).
    public float YerCekimi { get; init; } = 0.6f;

    // Space'e basinca dikey hiz bu degere AYARLANIR (eklenmez). Eksi = yukari.
    // (Chrome Dino: INITIAL_JUMP_VELOCITY 12)
    public float ZiplamaHizi { get; init; } = -12f;

    // Kusun asagi dogru ulasabilecegi en yuksek hiz.
    public float MaksimumDusmeHizi { get; init; } = 12f;

    // ---- Engeller ----
    public float EngelGenisligi { get; init; } = 60f;

    // Ust ve alt boru arasindaki, kusun gectigi bosluk.
    public float BoslukYuksekligi { get; init; } = 150f;

    // Iki engel arasindaki yatay mesafe.
    public float EngelAraligi { get; init; } = 320f;

    // Boslugun tavana ve zemine en az bu kadar uzak olmasi gerekir.
    public float KenarPayi { get; init; } = 60f;

    // Ilk engelin oyun basladiginda bulundugu yatay konum.
    // Ekranin bir engel araligi kadar disinda baslar; hiz 6 ile kusa ~2,4 sn'de ulasir.
    public float IlkEngelX { get; init; } = 960f;

    // ---- Hiz (Chrome offline.js: SPEED 6, MAX_SPEED 13, ACCELERATION 0.001) ----
    public float BaslangicHizi { get; init; } = 6f;
    public float MaksimumHiz { get; init; } = 13f;

    // 1 karede (1/60 sn) hiza eklenen artis (skora degil sureye bagli).
    public float Ivme { get; init; } = 0.001f;

    // ---- Oyun dongusu ve yonetim ----

    // Zamanlayicinin iki tik arasindaki istenen suresi (ms). Olcum: Windows'ta gercekte ~28 ms.
    // Bu yuzden fizik tik sayisina degil gecen sureye baglidir.
    public int ZamanlayiciAraligiMs { get; init; } = 16;

    // Fizik degerlerinin 1 karesi (ms): 1000 / 60. Dino degerleri 60 FPS icindir.
    public float HedefKareSuresiMs { get; init; } = 1000f / 60f;

    // Bir tikte en fazla kac kare ilerlenir (takilmada kus borunun icinden gecmesin).
    public float EnCokKareCarpani { get; init; } = 3f;

    // Game Over'dan sonra Space'in dikkate alinmadigi sure.
    // Kaynak: Chrome offline.js GAMEOVER_CLEAR_TIME: 750.
    public int YenidenBaslatmaBeklemeMs { get; init; } = 750;

    // Pencere kapanirken rekor kaydinin bitmesi icin beklenecek en uzun sure.
    public int KapanisKayitBeklemeMs { get; init; } = 2000;

    // ---- Hesaplanan degerler (ayar degil, yukaridakilerden turetilir) ----

    // Zeminin ust kenarinin Y koordinati: 480 - 40 = 440. Kus bunun altina inerse oyun biter.
    public float ZeminUstY => AlanYuksekligi - ZeminYuksekligi;
}
