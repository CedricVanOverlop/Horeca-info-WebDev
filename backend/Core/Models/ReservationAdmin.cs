namespace Core.Models;

/// <summary>
/// Réservation d'un utilisateur, vue par l'administrateur.
/// </summary>
public class ReservationAdmin
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan HeureDebut { get; set; }
    public TimeSpan HeureFin { get; set; }
    public decimal PrixPaye { get; set; }
    public string Terrain { get; set; } = string.Empty;
}
