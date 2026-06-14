using Core.UseCases.Abstractions;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace Api.Middleware;

/// <summary>
/// Vérifie, à chaque requête authentifiée, que l'utilisateur du token est toujours actif
/// (UTILISATEUR.actif = TRUE). Empêche un compte soft-deleted d'agir avec un token encore
/// valide (le JWT reste signé jusqu'à son expiration). Les requêtes anonymes passent : c'est
/// l'autorisation des endpoints qui décide d'exiger un token ou non.
/// </summary>
public class ActiveUserMiddleware(RequestDelegate next)
{
    /// <summary>
    /// Intercepte la requête : si l'appelant est authentifié mais inactif, renvoie 401.
    /// </summary>
    /// <param name="context">Contexte HTTP courant.</param>
    /// <param name="userUseCases">UseCases utilisateur (résolu par requête, scoped).</param>
    public async Task InvokeAsync(HttpContext context, IUserUseCases userUseCases)
    {
        // Requête anonyme (login, register, routes publiques) : rien à vérifier.
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var raw = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Token authentifié mais id absent/illisible, ou compte inactif -> 401.
            if (!int.TryParse(raw, out var id) || !await userUseCases.IsActive(id))
            {
                await WriteUnauthorized(context);
                return;
            }
        }

        await next(context);
    }

    private static async Task WriteUnauthorized(HttpContext context)
    {
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(
            JsonSerializer.Serialize(new { message = "Compte inactif ou session invalide." }));
    }
}
