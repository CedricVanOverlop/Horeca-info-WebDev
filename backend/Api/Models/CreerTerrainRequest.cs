namespace Api.Models;

/// <summary>
/// Corps de requête pour créer un terrain (admin uniquement). Le statut actif et le type
/// (inclus dans le nom) ne sont pas fournis ici ; un terrain est créé actif par défaut.
/// </summary>
public class CreerTerrainRequest
{
    public string Nom { get; set; } = string.Empty;
    public TimeSpan HeureOuverture { get; set; }
    public TimeSpan HeureFermeture { get; set; }
    public int IdCommerce { get; set; }
}
