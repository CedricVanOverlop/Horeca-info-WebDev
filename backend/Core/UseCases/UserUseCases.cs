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

    /// <summary>
    /// Retourne le profil de l'utilisateur identifié par le token.
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur (extrait du JWT).</param>
    /// <returns>Le profil, ou null si l'utilisateur est introuvable ou désactivé.</returns>
    public Task<User?> GetProfile(int id) => userGateway.GetProfile(id);

    /// <summary>
    /// Met à jour les informations personnelles de l'utilisateur identifié par le token.
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur (extrait du JWT, jamais du body).</param>
    /// <param name="request">Nouvelles informations (nom, prénom, email, téléphone).</param>
    /// <returns>True si mis à jour, false si l'utilisateur est introuvable.</returns>
    /// <exception cref="ValidationException">Si un champ obligatoire est vide.</exception>
    public Task<bool> UpdateInfos(int id, UpdateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nom) ||
            string.IsNullOrWhiteSpace(request.Prenom) ||
            string.IsNullOrWhiteSpace(request.Email))
            throw new ValidationException("Nom, prénom et email sont requis.");

        // L'identité ciblée vient toujours du token, pas du body (anti-usurpation).
        request.Id = id;
        return userGateway.UpdateInfos(request);
    }

    /// <summary>
    /// Change le mot de passe de l'utilisateur identifié par le token.
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur (extrait du JWT).</param>
    /// <param name="request">Nouveau mot de passe en clair (hashé dans le gateway).</param>
    /// <returns>True si mis à jour, false si l'utilisateur est introuvable.</returns>
    /// <exception cref="ValidationException">Si le nouveau mot de passe est vide.</exception>
    public Task<bool> UpdatePassword(int id, ChangePasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.NouveauMotDePasse))
            throw new ValidationException("Le nouveau mot de passe est requis.");

        return userGateway.UpdatePassword(id, request.NouveauMotDePasse);
    }

    /// <summary>
    /// Désactive (soft-delete) le compte de l'utilisateur identifié par le token.
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur (extrait du JWT).</param>
    /// <returns>True si désactivé, false si l'utilisateur est introuvable ou déjà inactif.</returns>
    public Task<bool> DeleteAccount(int id) => userGateway.DeleteAccount(id);
}
