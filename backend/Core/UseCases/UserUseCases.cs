using Core.Exceptions;
using Core.IGateways;
using Core.Models;
using Core.UseCases.Abstractions;

namespace Core.UseCases;

public class UserUseCases(IUserGateway userGateway) : IUserUseCases
{
    /// <summary>
    /// Authentifie un utilisateur et retourne son profil si les credentials sont valides.
    /// </summary>
    /// <param name="request">Email et mot de passe.</param>
    /// <returns>L'utilisateur authentifié, ou null si credentials invalides.</returns>
    /// <exception cref="ValidationException">Si email ou mot de passe est vide.</exception>
    public Task<User?> Authenticate(AuthenticationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.MotDePasse))
            throw new ValidationException("Email et mot de passe sont requis.");
        return userGateway.Authenticate(request.Email, request.MotDePasse);
    }

    /// <summary>
    /// Inscrit un nouvel utilisateur avec le rôle Client.
    /// </summary>
    /// <param name="request">Données d'inscription.</param>
    /// <returns>Le profil créé.</returns>
    /// <exception cref="ValidationException">Si un champ obligatoire est vide.</exception>
    public Task<User> Register(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nom) ||
            string.IsNullOrWhiteSpace(request.Prenom) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.MotDePasse))
            throw new ValidationException("Tous les champs sont requis.");
        return userGateway.Register(request);
    }

    /// <summary>
    /// Retourne tous les utilisateurs sans mot de passe.
    /// </summary>
    /// <returns>Liste des utilisateurs.</returns>
    public Task<IEnumerable<User>> GetAll() => userGateway.GetAll();
}
