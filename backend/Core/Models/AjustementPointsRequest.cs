namespace Core.Models;

/// <summary>
/// Ajustement manuel du solde de points de fidélité par un administrateur.
/// Montant positif = crédit, négatif = débit. Motif obligatoire (traçabilité, RG-03).
/// </summary>
public class AjustementPointsRequest
{
    public decimal Montant { get; set; }
    public string Motif { get; set; } = string.Empty;
}
