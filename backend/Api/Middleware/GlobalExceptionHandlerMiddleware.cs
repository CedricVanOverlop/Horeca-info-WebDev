using Core.Exceptions;
using System.Net;
using System.Text.Json;

namespace Api.Middleware;

public class GlobalExceptionHandlerMiddleware(RequestDelegate next)
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
        catch (ConflictException ex)
        {
            await WriteResponse(context, HttpStatusCode.Conflict, ex.Message);
        }
        catch (Exception ex)
        {
            await WriteResponse(context, HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    private static async Task WriteResponse(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new { message }));
    }
}
