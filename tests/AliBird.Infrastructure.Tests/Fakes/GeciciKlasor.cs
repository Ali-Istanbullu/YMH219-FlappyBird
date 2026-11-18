namespace AliBird.Infrastructure.Tests.Fakes;

// Her test icin benzersiz, bos bir gecici klasor.
// Testler paralel calisirken ayni dosyaya yazip birbirini bozmasin diye her test kendi klasorunu kullanir.
// "using var klasor = new GeciciKlasor();" ile kullanilir; test bitince klasor silinir.
public sealed class GeciciKlasor : IDisposable
{
    public string Yol { get; } = Path.Combine(Path.GetTempPath(), "AliBirdTest_" + Guid.NewGuid().ToString("N"));

    public GeciciKlasor()
    {
        Directory.CreateDirectory(Yol);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(Yol, recursive: true);
        }
        catch
        {
            // Temizlik basarisiz olursa test sonucu etkilenmesin (dosya kilidi vb.).
        }
    }
}
