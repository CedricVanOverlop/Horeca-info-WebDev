namespace Core.Models;

/// <summary>
/// Demande de changement de niveau d'accès d'un utilisateur.
/// Acces ∈ { 'Client', 'Employe', 'Cuisine', 'Administrateur' }.
/// 'Client' supprime la ligne EMPLOYE ; les autres la créent ou la mettent à jour.
/// </summary>
public class ChangeRoleRequest
{
    public string Acces { get; set; } = string.Empty;
}
