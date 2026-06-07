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
        catch (Exception ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            var error = new { message = ex.Message };
            await context.Response.WriteAsync(JsonSerializer.Serialize(error));
        }
    }
}
