namespace Api.Models;

/// <summary>
/// Corps de requête pour créer ou modifier un tarif d'un terrain. Lors d'une modification,
/// l'identifiant du tarif provient de la route, pas du corps.
/// </summary>
public class TarifRequest
{
    public string Type { get; set; } = string.Empty;
    public decimal PrixHeure { get; set; }
    public TimeSpan HeureDebut { get; set; }
    public TimeSpan HeureFin { get; set; }
    public int JourSemaine { get; set; }
    public string TerrainId { get; set; } = string.Empty;
}
