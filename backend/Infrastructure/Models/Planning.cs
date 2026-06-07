namespace Infrastructure.Models;

public class PlanningDb
{
    public string Id { get; set; } = string.Empty;
    public string EmployeId { get; set; } = string.Empty;
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
}
