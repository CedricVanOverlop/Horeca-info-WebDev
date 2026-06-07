namespace Core.Models;

public class Terrain
{
    public string Id { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool Disponible { get; set; } = true;
}
