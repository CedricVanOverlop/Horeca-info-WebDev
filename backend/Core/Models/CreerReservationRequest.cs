namespace Core.Models;

/// <summary>
/// Données d'entrée pour créer une réservation (client ou manuelle admin/cuisine).
/// Le prix payé et le tarif appliqué ne sont pas fournis par le client : ils sont
/// calculés et figés côté serveur (RG-08, RG-09). Remarques est optionnel et sert
/// notamment au blocage saisi par le personnel.
/// </summary>
public class CreerReservationRequest
{
    public string TerrainId { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeSpan HeureDebut { get; set; }
    public TimeSpan HeureFin { get; set; }
    public string MoyenPaiement { get; set; } = string.Empty;  // 'EnLigne' | 'SurPlace'
    public string? Remarques { get; set; }
}
