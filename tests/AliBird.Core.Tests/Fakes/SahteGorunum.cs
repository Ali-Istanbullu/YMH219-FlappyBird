using AliBird.Core.Abstractions;
using AliBird.Core.Models;

namespace AliBird.Core.Tests.Fakes;

// Form yerine kullanilan sahte gorunum. Testte "tusa basildi" olaylarini elle tetikler,
// presenter'in gonderdigi son goruntuyu saklar.
public sealed class SahteGorunum : IOyunGorunumu
{
    public event EventHandler? BoslukBasildi;
    public event EventHandler? PBasildi;
    public event EventHandler? MBasildi;
    public event EventHandler? YenidenBaslatTiklandi;
    public event EventHandler? OdakKaybedildi;

    // Son gosterilen goruntu (testte kontrol edilir).
    public OyunGoruntusu? SonGoruntu { get; private set; }

    // true yapilirsa Goster() hata firlatir (cizim hatasi senaryosu, CK3).
    public bool GosterHataFirlatsin { get; set; }

    public void Goster(OyunGoruntusu goruntu)
    {
        if (GosterHataFirlatsin)
        {
            throw new InvalidOperationException("Sahte cizim hatasi");
        }
        SonGoruntu = goruntu;
    }

    public void BoslugaBas() => BoslukBasildi?.Invoke(this, EventArgs.Empty);
    public void PyeBas() => PBasildi?.Invoke(this, EventArgs.Empty);
    public void MyeBas() => MBasildi?.Invoke(this, EventArgs.Empty);
    public void YenidenBaslatTikla() => YenidenBaslatTiklandi?.Invoke(this, EventArgs.Empty);
    public void OdagiKaybet() => OdakKaybedildi?.Invoke(this, EventArgs.Empty);
}
