namespace Core.Models;

/// <summary>
/// Réservation d'un terrain de padel. Aligné sur la table SQL RESERVATION :
/// la date et les heures sont séparées, le tarif appliqué est référencé
/// (TarifId), le moyen de paiement est purement informatif et la remarque
/// est libre. PrixPaye est calculé et figé à l'insertion (RG-09).
/// </summary>
public class Reservation
{
    public string Id { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string TerrainId { get; set; } = string.Empty;
    public string TarifId { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeSpan HeureDebut { get; set; }
    public TimeSpan HeureFin { get; set; }
    public decimal PrixPaye { get; set; }
    public string MoyenPaiement { get; set; } = string.Empty;  // 'EnLigne' | 'SurPlace' (informatif)
    public string? Remarques { get; set; }
    public DateTime DateReservation { get; set; }
}
