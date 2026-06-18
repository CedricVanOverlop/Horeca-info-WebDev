namespace Infrastructure.Models;

public class ReservationAdminDb
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan HeureDebut { get; set; }
    public TimeSpan HeureFin { get; set; }
    public decimal PrixPaye { get; set; }
    public string Terrain { get; set; } = string.Empty;
    public string Client { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public string MoyenPaiement { get; set; } = string.Empty;
    public string? Remarques { get; set; }
}
