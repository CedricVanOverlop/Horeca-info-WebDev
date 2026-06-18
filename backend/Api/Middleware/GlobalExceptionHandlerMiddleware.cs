using Core.Exceptions;
using System.Net;
using System.Text.Json;

namespace Api.Middleware;

public class GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            await WriteResponse(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (NotFoundException ex)
        {
            await WriteResponse(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (ForbiddenException ex)
        {
            await WriteResponse(context, HttpStatusCode.Forbidden, ex.Message);
        }
        catch (ConflictException ex)
        {
            await WriteResponse(context, HttpStatusCode.Conflict, ex.Message);
        }
        catch (Exception ex)
        {
            // On journalise le détail technique côté serveur, mais on ne renvoie jamais
            // le message brut (SQL, stack…) au client : message FR générique.
            logger.LogError(ex, "Erreur non gérée sur {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteResponse(context, HttpStatusCode.InternalServerError,
                "Une erreur inattendue est survenue. Réessayez ou contactez le support si le problème persiste.");
        }
    }

    private static async Task WriteResponse(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new { message }));
    }
}
