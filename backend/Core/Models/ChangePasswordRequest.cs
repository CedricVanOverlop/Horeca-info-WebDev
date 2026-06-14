namespace Core.Models;

public class ChangePasswordRequest
{
    public string AncienMotDePasse { get; set; } = string.Empty;
    public string NouveauMotDePasse { get; set; } = string.Empty;
}
