using Api.Services;
using Core.Models;
using Core.UseCases.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Api.EndPoints;

public static class UtilisateurRoutes
{
    public static void MapUtilisateurRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/api/utilisateurs");

        group.MapPost("/login", async ([FromBody] AuthenticationRequest request, IUserUseCases useCases, IConfiguration config) =>
        {
            var user = await useCases.Authenticate(request);
            if (user is null) return Results.Unauthorized();
            return Results.Ok(new { token = JwtTokenService.GenerateToken(user, config) });
        });

        group.MapPost("/register", async ([FromBody] RegisterRequest request, IUserUseCases useCases) =>
        {
            var user = await useCases.Register(request);
            return Results.Created($"/api/utilisateurs/{user.Id}", new { user.Id, user.Nom, user.Prenom, user.Email, user.Role });
        });

        group.MapGet("/", async (IUserUseCases useCases) =>
        {
            var users = await useCases.GetAll();
            return Results.Ok(users);
        }).RequireAuthorization("AdminOnly");
    }
}
