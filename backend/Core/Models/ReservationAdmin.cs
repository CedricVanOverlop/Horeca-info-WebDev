namespace Core.Models;

/// <summary>
/// Réservation enrichie pour la vue staff (administrateur/cuisine) : inclut le nom
/// du terrain et l'identité du client, afin d'afficher et d'annuler n'importe quelle
/// réservation, y compris celles créées par un client.
/// </summary>
public class ReservationAdmin
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
