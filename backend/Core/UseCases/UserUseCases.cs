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
    /// Indique si l'utilisateur est actif. Utilisé par le middleware pour rejeter
    /// les requêtes d'un compte soft-deleted dont le token est encore valide.
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur (extrait du JWT).</param>
    /// <returns>True si actif, false sinon.</returns>
    public Task<bool> IsActive(int id) => userGateway.IsActive(id);

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
        if (string.IsNullOrWhiteSpace(request.AncienMotDePasse))
            throw new ValidationException("Le mot de passe actuel est requis.");
        if (string.IsNullOrWhiteSpace(request.NouveauMotDePasse))
            throw new ValidationException("Le nouveau mot de passe est requis.");

        return userGateway.UpdatePassword(id, request.AncienMotDePasse, request.NouveauMotDePasse);
    }

    /// <summary>
    /// Désactive (soft-delete) le compte de l'utilisateur identifié par le token.
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur (extrait du JWT).</param>
    /// <returns>True si désactivé, false si l'utilisateur est introuvable ou déjà inactif.</returns>
    public Task<bool> DeleteAccount(int id) => userGateway.DeleteAccount(id);

    // ── Administration ────────────────────────────────────────────

    /// <summary>
    /// Retourne tous les utilisateurs (actifs et bloqués) avec leur rôle, pour l'administration.
    /// </summary>
    /// <returns>La liste complète des utilisateurs.</returns>
    public Task<IEnumerable<UserAdmin>> GetAllForAdmin() => userGateway.GetAllForAdmin();

    /// <summary>
    /// Change le niveau d'accès d'un utilisateur. Empêche de rétrograder le dernier administrateur actif.
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur ciblé.</param>
    /// <param name="request">Niveau d'accès cible.</param>
    /// <exception cref="ValidationException">Si le niveau d'accès est invalide.</exception>
    /// <exception cref="ConflictException">Si l'opération retirerait le dernier administrateur actif.</exception>
    public async Task ChangeRole(int id, ChangeRoleRequest request)
    {
        var validRoles = new[] { "Client", "Employe", "Cuisine", "Administrateur" };
        if (string.IsNullOrWhiteSpace(request.Acces) || !validRoles.Contains(request.Acces))
            throw new ValidationException("Niveau d'accès invalide.");

        // On ne peut pas retirer le rôle Administrateur au dernier admin actif.
        if (request.Acces != "Administrateur" && await userGateway.IsLastActiveAdmin(id))
            throw new ConflictException("Impossible de rétrograder le dernier administrateur.");

        await userGateway.ChangeRole(id, request.Acces);
    }

    /// <summary>
    /// Ajuste le solde de points d'un utilisateur (RG-03). Motif obligatoire, montant non nul.
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur ciblé.</param>
    /// <param name="request">Montant signé et motif de l'ajustement.</param>
    /// <returns>True si appliqué.</returns>
    /// <exception cref="ValidationException">Si le montant est nul, le motif vide, ou le solde deviendrait négatif.</exception>
    public async Task<bool> AdjustPoints(int id, AjustementPointsRequest request)
    {
        if (request.Montant == 0)
            throw new ValidationException("Le montant de l'ajustement ne peut pas être nul.");
        if (string.IsNullOrWhiteSpace(request.Motif))
            throw new ValidationException("Le motif de l'ajustement est requis.");

        var applied = await userGateway.AdjustPoints(id, request.Montant, request.Motif);
        if (!applied)
            throw new ValidationException("Solde de points insuffisant pour cet ajustement.");
        return true;
    }

    /// <summary>
    /// Bloque un compte (empêche la connexion). Empêche de bloquer le dernier administrateur actif.
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur à bloquer.</param>
    /// <returns>True si bloqué, false si introuvable ou déjà bloqué.</returns>
    /// <exception cref="ConflictException">Si l'utilisateur est le dernier administrateur actif.</exception>
    public async Task<bool> Block(int id)
    {
        if (await userGateway.IsLastActiveAdmin(id))
            throw new ConflictException("Impossible de bloquer le dernier administrateur.");
        return await userGateway.Block(id);
    }

    /// <summary>
    /// Débloque un compte (réautorise la connexion).
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur à débloquer.</param>
    /// <returns>True si débloqué.</returns>
    public Task<bool> Unblock(int id) => userGateway.Unblock(id);

    /// <summary>
    /// Supprime (soft-delete) le compte d'un utilisateur par un administrateur :
    /// annule les réservations futures, désactive l'éventuelle ligne EMPLOYE et le compte.
    /// Empêche la suppression du dernier administrateur actif.
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur à supprimer.</param>
    /// <returns>True si supprimé, false si introuvable ou déjà inactif.</returns>
    /// <exception cref="ConflictException">Si l'utilisateur est le dernier administrateur actif.</exception>
    public async Task<bool> DeleteByAdmin(int id)
    {
        if (await userGateway.IsLastActiveAdmin(id))
            throw new ConflictException("Impossible de supprimer le dernier administrateur.");
        return await userGateway.DeleteAccount(id);
    }

    /// <summary>
    /// Retourne les réservations d'un utilisateur (vue administrateur).
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur.</param>
    /// <returns>Les réservations de l'utilisateur.</returns>
    public Task<IEnumerable<ReservationAdmin>> GetReservations(int id) => userGateway.GetReservations(id);

    /// <summary>
    /// Retourne les horaires de travail d'un utilisateur employé (vue administrateur).
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur.</param>
    /// <returns>Les horaires de l'utilisateur.</returns>
    public Task<IEnumerable<HoraireAdmin>> GetHoraires(int id) => userGateway.GetHoraires(id);
}
