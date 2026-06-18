namespace Core.Models;

/// <summary>
/// Créneau déjà réservé d'un terrain, sans aucune donnée nominative. Sert à griser
/// les cases occupées dans la grille de réservation côté client (respect de la vie privée :
/// l'identité du réservant n'est jamais exposée aux autres utilisateurs).
/// </summary>
public class CreneauOccupe
{
    public DateTime Date { get; set; }
    public TimeSpan HeureDebut { get; set; }
    public TimeSpan HeureFin { get; set; }
}
