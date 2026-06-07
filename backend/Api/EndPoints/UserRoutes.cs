using Core.Models;
using Core.UseCases.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.EndPoints;

public static class UserRoutes
{
    public static void MapUserRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/api/users");

        group.MapPost("/auth", async ([FromBody] AuthenticationRequest request, IUserUseCases useCases, IConfiguration config) =>
        {
            var user = await useCases.Authenticate(request);
            if (user is null) return Results.Unauthorized();
            return Results.Ok(new { token = GenerateToken(user, config) });
        });

        group.MapPost("/register", async ([FromBody] RegisterRequest request, IUserUseCases useCases) =>
        {
            var user = await useCases.Register(request);
            return Results.Created($"/api/users/{user.Id}", new { user.Id, user.Nom, user.Prenom, user.Email, user.Role });
        });

        group.MapGet("/", async (IUserUseCases useCases) =>
        {
            var users = await useCases.GetAll();
            return Results.Ok(users);
        }).RequireAuthorization();
    }

    internal static string GenerateToken(User user, IConfiguration config)
    {
        var jwt = config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("nom", user.Nom),
            new Claim("prenom", user.Prenom)
        };
        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(jwt["ExpireTimeInMinutes"] ?? "480")),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
