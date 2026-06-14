namespace Infrastructure.Models;

public class UserAdminDb
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal PointsSolde { get; set; }
    public string Role { get; set; } = "Client";
    public bool Actif { get; set; } = true;
}
