namespace Core.Models;

/// <summary>
/// Vue administrateur d'un utilisateur : profil + rôle résolu + état actif/bloqué.
/// </summary>
public class UserAdmin
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal PointsSolde { get; set; }
    public string Role { get; set; } = "Client";
    /// <summary>FALSE = compte bloqué (ne peut plus se connecter).</summary>
    public bool Actif { get; set; } = true;
}
