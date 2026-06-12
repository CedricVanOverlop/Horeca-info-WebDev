using Core.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Services;

public static class JwtTokenService
{
    /// <summary>
    /// Génère un JWT signé contenant les claims utilisateur.
    /// </summary>
    /// <param name="user">Utilisateur authentifié.</param>
    /// <param name="config">Configuration contenant les paramètres JWT.</param>
    /// <returns>Token JWT signé.</returns>
    public static string GenerateToken(User user, IConfiguration config)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            Environment.GetEnvironmentVariable("JWT_KEY") ?? config["Jwt:Key"]!));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role,               user.Role),
            new Claim("nom",                         user.Nom),
            new Claim("prenom",                      user.Prenom)
        };

        var token = new JwtSecurityToken(
            issuer:             config["Jwt:Issuer"],
            audience:           config["Jwt:Audience"],
            claims:             claims,
            expires:            DateTime.UtcNow.AddMinutes(double.Parse(config["Jwt:ExpireTimeInMinutes"] ?? "480")),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
