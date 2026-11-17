namespace AliBird.Core.Models;

// Veritabaninda tutulan tek en yuksek skoru temsil eder (K10).
// "record" kullandik: olusturulduktan sonra degistirilemez (immutable),
// iki RekorKaydi ayni degerleri tasiyorsa esit sayilir (testlerde kolaylik).
//
// Puan  : rekor olan puan (gecilen engel sayisi, K9).
// Tarih : rekorun kirildigi an. Ekranda gg.aa.yyyy olarak gosterilir.
//
// Hic rekor yoksa (ilk acilis) IPuanDeposu bu tipi degil null dondurur (D3a).
public sealed record RekorKaydi(int Puan, DateTime Tarih);
