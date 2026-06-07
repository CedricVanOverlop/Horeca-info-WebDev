namespace Core.Models;

public class Reservation
{
    public string Id { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string TerrainId { get; set; } = string.Empty;
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    public decimal Prix { get; set; }
}
