namespace Infrastructure.Models;

public class ReservationDb
{
    public string Id { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string TerrainId { get; set; } = string.Empty;
    public string TarifId { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeSpan HeureDebut { get; set; }
    public TimeSpan HeureFin { get; set; }
    public decimal PrixPaye { get; set; }
    public string MoyenPaiement { get; set; } = string.Empty;
    public string? Remarques { get; set; }
    public DateTime DateReservation { get; set; }
}
