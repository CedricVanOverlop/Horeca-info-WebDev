namespace Core.Models;

public class Tarif
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal PrixHeure { get; set; }
    public TimeSpan HeureDebut { get; set; }
    public TimeSpan HeureFin { get; set; }
    public int JourSemaine { get; set; }
    public string TerrainId { get; set; } = string.Empty;
}
