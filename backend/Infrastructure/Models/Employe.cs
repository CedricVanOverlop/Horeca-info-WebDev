namespace Infrastructure.Models;

public class EmployeDb
{
    public int IdEmploye { get; set; }
    public int IdUtilisateur { get; set; }
    public string Acces { get; set; } = string.Empty;
    public bool Actif { get; set; }
    public int? IdCommercePreference { get; set; }
}
