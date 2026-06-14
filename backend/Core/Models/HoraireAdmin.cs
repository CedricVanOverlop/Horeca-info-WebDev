namespace Core.Models;

/// <summary>
/// Horaire de travail d'un employé, vu par l'administrateur.
/// </summary>
public class HoraireAdmin
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan HeureDebut { get; set; }
    public TimeSpan HeureFin { get; set; }
    public decimal HeurePayee { get; set; }
    public string Statut { get; set; } = string.Empty;
    public string Commerce { get; set; } = string.Empty;
}
