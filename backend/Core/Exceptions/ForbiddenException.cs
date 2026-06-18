namespace Core.Exceptions;

/// <summary>
/// Levée quand l'utilisateur n'a pas le droit d'agir sur la ressource ciblée
/// (ex: un client tente d'annuler une réservation qui n'est pas la sienne).
/// Traduite en HTTP 403 par le middleware.
/// </summary>
public class ForbiddenException(string message) : Exception(message);
