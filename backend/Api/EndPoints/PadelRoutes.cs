using Api.Models;
using Core.Models;
using Core.UseCases.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.EndPoints;

public static class PadelRoutes
{
    public static void MapPadelRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/api/padel");

        // ── Terrains ──────────────────────────────────────────────
        // Liste des terrains (le front filtre les inactifs côté réservation client).
        group.MapGet("/terrains", async (IPadelUseCases useCases) =>
        {
            var terrains = await useCases.GetTerrains();
            return Results.Ok(terrains);
        }).RequireAuthorization();

        // Grille tarifaire d'un terrain (consultée par le client pour le prix, par l'admin pour la gestion).
        group.MapGet("/terrains/{id}/tarifs", async (string id, IPadelUseCases useCases) =>
        {
            var tarifs = await useCases.GetTarifs(id);
            return Results.Ok(tarifs);
        }).RequireAuthorization();

        // Créneaux occupés d'un terrain sur une plage de dates (grille de disponibilité, sans données nominatives).
        group.MapGet("/terrains/{id}/creneaux", async (string id, DateTime from, DateTime to, IPadelUseCases useCases) =>
        {
            var creneaux = await useCases.GetCreneauxOccupes(id, from, to);
            return Results.Ok(creneaux);
        }).RequireAuthorization();

        // Création d'un terrain : administrateur uniquement.
        group.MapPost("/terrains", async ([FromBody] CreerTerrainRequest request, IPadelUseCases useCases) =>
        {
            var terrain = await useCases.CreateTerrain(new Terrain
            {
                Nom            = request.Nom,
                HeureOuverture = request.HeureOuverture,
                HeureFermeture = request.HeureFermeture,
                IdCommerce     = request.IdCommerce,
                Disponible     = true
            });
            return Results.Created($"/api/padel/terrains/{terrain.Id}", terrain);
        }).RequireAuthorization("AdminOnly");

        // Modification d'un terrain (nom + horaires) : administrateur uniquement.
        group.MapPut("/terrains/{id}", async (string id, [FromBody] ModifierTerrainRequest request, IPadelUseCases useCases) =>
        {
            var terrain = await useCases.UpdateTerrain(id, request.Nom, request.HeureOuverture, request.HeureFermeture);
            return Results.Ok(terrain);
        }).RequireAuthorization("AdminOnly");

        // Activation/désactivation d'un terrain : admin + cuisine.
        group.MapPut("/terrains/{id}/actif", async (string id, [FromBody] ToggleActifRequest request, IPadelUseCases useCases) =>
        {
            await useCases.ToggleTerrainActif(id, request.Actif);
            return Results.Ok();
        }).RequireAuthorization("CuisineOrAdmin");

        // ── Tarifs (administrateur uniquement) ────────────────────
        group.MapPost("/tarifs", async ([FromBody] TarifRequest request, IPadelUseCases useCases) =>
        {
            var tarif = await useCases.CreateTarif(MapTarif(request));
            return Results.Created($"/api/padel/tarifs/{tarif.Id}", tarif);
        }).RequireAuthorization("AdminOnly");

        group.MapPut("/tarifs/{id}", async (string id, [FromBody] TarifRequest request, IPadelUseCases useCases) =>
        {
            var tarif = MapTarif(request);
            tarif.Id = id;
            await useCases.UpdateTarif(tarif);
            return Results.Ok();
        }).RequireAuthorization("AdminOnly");

        group.MapDelete("/tarifs/{id}", async (string id, IPadelUseCases useCases) =>
        {
            await useCases.DeleteTarif(id);
            return Results.NoContent();
        }).RequireAuthorization("AdminOnly");

        // ── Réservations staff (admin + cuisine) ──────────────────
        // Toutes les réservations (terrain + client) : consultation et annulation par le staff.
        group.MapGet("/reservations/admin", async (IPadelUseCases useCases) =>
        {
            var reservations = await useCases.GetAllReservationsAdmin();
            return Results.Ok(reservations);
        }).RequireAuthorization("CuisineOrAdmin");

        // ── Réservations client ───────────────────────────────────
        group.MapGet("/reservations/me", async (ClaimsPrincipal principal, IPadelUseCases useCases) =>
        {
            var id = GetUserId(principal);
            if (id is null) return Results.Unauthorized();

            var reservations = await useCases.GetReservations(id.Value);
            return Results.Ok(reservations);
        }).RequireAuthorization();

        group.MapPost("/reservations", async ([FromBody] CreerReservationRequest request, ClaimsPrincipal principal, IPadelUseCases useCases) =>
        {
            var id = GetUserId(principal);
            if (id is null) return Results.Unauthorized();

            var reservation = await useCases.CreateReservationClient(id.Value, request);
            return Results.Created($"/api/padel/reservations/{reservation.Id}", reservation);
        }).RequireAuthorization();

        // Annulation : client (la sienne, > 12h) ou staff (libre). La distinction se fait via le rôle.
        group.MapDelete("/reservations/{id}", async (string id, ClaimsPrincipal principal, IPadelUseCases useCases) =>
        {
            var userId = GetUserId(principal);
            if (userId is null) return Results.Unauthorized();

            await useCases.CancelReservation(id, userId.Value, EstStaff(principal));
            return Results.NoContent();
        }).RequireAuthorization();

        // ── Réservation manuelle (admin + cuisine) ────────────────
        // Recherche d'un utilisateur existant à qui rattacher la réservation.
        group.MapGet("/utilisateurs/recherche", async (string q, IPadelUseCases useCases) =>
        {
            var utilisateurs = await useCases.SearchUtilisateurs(q);
            return Results.Ok(utilisateurs);
        }).RequireAuthorization("CuisineOrAdmin");

        group.MapPost("/reservations/manuelle/{idUtilisateur:int}", async (int idUtilisateur, [FromBody] CreerReservationRequest request, IPadelUseCases useCases) =>
        {
            var reservation = await useCases.CreateReservationManuelle(idUtilisateur, request);
            return Results.Created($"/api/padel/reservations/{reservation.Id}", reservation);
        }).RequireAuthorization("CuisineOrAdmin");
    }

    /// <summary>
    /// Mappe le DTO de requête tarif vers le modèle métier.
    /// </summary>
    /// <param name="request">Données du tarif reçues du client.</param>
    /// <returns>Le tarif métier (sans identifiant).</returns>
    private static Tarif MapTarif(TarifRequest request) => new()
    {
        Type        = request.Type,
        PrixHeure   = request.PrixHeure,
        HeureDebut  = request.HeureDebut,
        HeureFin    = request.HeureFin,
        JourSemaine = request.JourSemaine,
        TerrainId   = request.TerrainId
    };

    /// <summary>
    /// Extrait l'identifiant de l'utilisateur courant depuis les claims du JWT.
    /// </summary>
    /// <param name="principal">Identité de l'utilisateur authentifié.</param>
    /// <returns>L'identifiant de l'utilisateur, ou null si absent/invalide.</returns>
    private static int? GetUserId(ClaimsPrincipal principal)
    {
        var raw = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(raw, out var id) ? id : null;
    }

    /// <summary>
    /// Indique si l'utilisateur courant est membre du personnel autorisé à annuler librement
    /// (administrateur ou cuisine). Les autres rôles sont traités comme des clients.
    /// </summary>
    /// <param name="principal">Identité de l'utilisateur authentifié.</param>
    /// <returns>True si administrateur ou cuisine.</returns>
    private static bool EstStaff(ClaimsPrincipal principal) =>
        principal.IsInRole("Administrateur") || principal.IsInRole("Cuisine");
}
