using Api.Services;
using Core.Models;
using Core.UseCases.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

        group.MapGet("/me", async (ClaimsPrincipal principal, IUserUseCases useCases) =>
        {
            var id = GetUserId(principal);
            if (id is null) return Results.Unauthorized();

            var profile = await useCases.GetProfile(id.Value);
            return profile is null ? Results.NotFound() : Results.Ok(profile);
        }).RequireAuthorization();

        group.MapPut("/me", async ([FromBody] UpdateUserRequest request, ClaimsPrincipal principal, IUserUseCases useCases) =>
        {
            var id = GetUserId(principal);
            if (id is null) return Results.Unauthorized();

            var updated = await useCases.UpdateInfos(id.Value, request);
            return updated ? Results.Ok() : Results.NotFound();
        }).RequireAuthorization();

        group.MapPut("/me/password", async ([FromBody] ChangePasswordRequest request, ClaimsPrincipal principal, IUserUseCases useCases) =>
        {
            var id = GetUserId(principal);
            if (id is null) return Results.Unauthorized();

            var updated = await useCases.UpdatePassword(id.Value, request);
            return updated ? Results.Ok() : Results.NotFound();
        }).RequireAuthorization();

        group.MapDelete("/me", async (ClaimsPrincipal principal, IUserUseCases useCases) =>
        {
            var id = GetUserId(principal);
            if (id is null) return Results.Unauthorized();

            var deleted = await useCases.DeleteAccount(id.Value);
            return deleted ? Results.NoContent() : Results.NotFound();
        }).RequireAuthorization();

        // ── Administration (réservé au rôle Administrateur) ───────────

        group.MapGet("/admin", async (IUserUseCases useCases) =>
        {
            var users = await useCases.GetAllForAdmin();
            return Results.Ok(users);
        }).RequireAuthorization("AdminOnly");

        group.MapPut("/{id:int}/role", async (int id, [FromBody] ChangeRoleRequest request, IUserUseCases useCases) =>
        {
            await useCases.ChangeRole(id, request);
            return Results.Ok();
        }).RequireAuthorization("AdminOnly");

        group.MapPost("/{id:int}/points", async (int id, [FromBody] AjustementPointsRequest request, IUserUseCases useCases) =>
        {
            await useCases.AdjustPoints(id, request);
            return Results.Ok();
        }).RequireAuthorization("AdminOnly");

        group.MapPut("/{id:int}/bloquer", async (int id, IUserUseCases useCases) =>
        {
            var blocked = await useCases.Block(id);
            return blocked ? Results.Ok() : Results.NotFound();
        }).RequireAuthorization("AdminOnly");

        group.MapPut("/{id:int}/debloquer", async (int id, IUserUseCases useCases) =>
        {
            var unblocked = await useCases.Unblock(id);
            return unblocked ? Results.Ok() : Results.NotFound();
        }).RequireAuthorization("AdminOnly");

        group.MapDelete("/{id:int}", async (int id, IUserUseCases useCases) =>
        {
            var deleted = await useCases.DeleteByAdmin(id);
            return deleted ? Results.NoContent() : Results.NotFound();
        }).RequireAuthorization("AdminOnly");

        group.MapGet("/{id:int}/reservations", async (int id, IUserUseCases useCases) =>
        {
            var reservations = await useCases.GetReservations(id);
            return Results.Ok(reservations);
        }).RequireAuthorization("AdminOnly");

        group.MapGet("/{id:int}/horaires", async (int id, IUserUseCases useCases) =>
        {
            var horaires = await useCases.GetHoraires(id);
            return Results.Ok(horaires);
        }).RequireAuthorization("AdminOnly");
    }

    /// <summary>
    /// Extrait l'identifiant de l'utilisateur courant depuis les claims du JWT.
    /// Le claim "sub" est remappé sur ClaimTypes.NameIdentifier par défaut.
    /// </summary>
    /// <param name="principal">Identité de l'utilisateur authentifié.</param>
    /// <returns>L'identifiant de l'utilisateur, ou null si absent/invalide.</returns>
    private static int? GetUserId(ClaimsPrincipal principal)
    {
        var raw = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(raw, out var id) ? id : null;
    }
}
