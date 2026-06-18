namespace Api.Models;

/// <summary>
/// Corps de requête pour modifier un terrain (admin uniquement) : nom et plage horaire
/// réservable. Le statut actif se gère via la route dédiée /terrains/{id}/actif.
/// </summary>
public class ModifierTerrainRequest
{
    public string Nom { get; set; } = string.Empty;
    public TimeSpan HeureOuverture { get; set; }
    public TimeSpan HeureFermeture { get; set; }
}
