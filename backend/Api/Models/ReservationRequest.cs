namespace Api.Models;

public class ReservationRequest
{
    public Guid TerrainId { get; set; }
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
}
