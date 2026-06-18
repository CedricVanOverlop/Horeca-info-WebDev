namespace Core.Models;

/// <summary>
/// Terrain de padel. Disponible reflète TERRAIN.actif. HeureOuverture /
/// HeureFermeture bornent la plage réservable (identiques tous les jours).
/// </summary>
public class Terrain
{
    public string Id { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool Disponible { get; set; } = true;
    public TimeSpan HeureOuverture { get; set; }
    public TimeSpan HeureFermeture { get; set; }
    public int IdCommerce { get; set; }
}
