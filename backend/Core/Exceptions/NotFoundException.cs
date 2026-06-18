namespace Core.Exceptions;

/// <summary>
/// Levée quand une ressource demandée n'existe pas. Traduite en HTTP 404 par le middleware.
/// </summary>
public class NotFoundException(string message) : Exception(message);
