namespace Core.Models;

public class AuthenticationRequest
{
    public string Email { get; set; } = string.Empty;
    public string MotDePasse { get; set; } = string.Empty;
}
