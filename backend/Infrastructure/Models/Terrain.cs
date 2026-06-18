namespace Infrastructure.Models;

public class TerrainDb
{
    public string Id { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public bool Disponible { get; set; } = true;
    public TimeSpan HeureOuverture { get; set; }
    public TimeSpan HeureFermeture { get; set; }
    public int IdCommerce { get; set; }
}
