using Core.Exceptions;
using Core.IGateways;
using Core.Models;
using Infrastructure.Repositories.Abstractions;
using MySql.Data.MySqlClient;

namespace Infrastructure.Gateways;

public class UserGateway(
    IUserRepository userRepository,
    IEmployeRepository employeRepository,
    IReservationRepository reservationRepository) : IUserGateway
{
    /// <summary>
    /// Authentifie un utilisateur et retourne son profil métier si les credentials sont valides.
    /// </summary>
    /// <param name="email">Adresse email de l'utilisateur.</param>
    /// <param name="motDePasse">Mot de passe en clair (vérifié contre le hash BCrypt).</param>
    /// <returns>
    /// L'utilisateur authentifié, ou null si credentials invalides ou compte client désactivé.
    /// Un employé désactivé (EMPLOYE.actif = FALSE) conserve son compte client et se connecte
    /// avec le rôle Client : seul son rôle staff est perdu, pas l'accès au compte.
    /// </returns>
    public async Task<User?> Authenticate(string email, string motDePasse)
    {
        var userDb = await userRepository.FindByEmail(email);
        if (userDb is null) return null;

        // Compte soft-deleted : connexion refusée (message générique côté endpoint -> 401).
        if (!userDb.Actif) return null;

        bool passwordValid;
        try
        {
            passwordValid = BCrypt.Net.BCrypt.Verify(motDePasse, userDb.MotDePasse);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // Hash stocké invalide (mot de passe non hashé en base) : credentials invalides.
            return null;
        }
        if (!passwordValid) return null;

        // Rôle staff uniquement si la ligne EMPLOYE existe ET est active.
        // Employé désactivé -> rôle dégradé en Client (le login reste autorisé).
        var employeDb = await employeRepository.FindByIdUtilisateur(userDb.Id);
        var role = (employeDb is not null && employeDb.Actif) ? employeDb.Acces : "Client";

        return new User
        {
            Id          = userDb.Id,
            Nom         = userDb.Nom,
            Prenom      = userDb.Prenom,
            Email       = userDb.Email,
            Telephone   = userDb.Telephone,
            PointsSolde = userDb.PointsSolde,
            Role        = role
        };
    }

    /// <summary>
    /// Inscrit un nouvel utilisateur avec le mot de passe hashé en BCrypt.
    /// Si l'email correspond à un compte soft-deleted (actif = FALSE), ce compte est
    /// réactivé avec les nouvelles informations et le nouveau mot de passe (l'historique
    /// et le solde de points sont conservés). Si le compte est actif, l'email est en conflit.
    /// </summary>
    /// <param name="request">Données d'inscription.</param>
    /// <returns>Le profil de l'utilisateur créé ou réactivé, avec le rôle Client.</returns>
    /// <exception cref="ConflictException">Si l'email est déjà utilisé par un compte actif.</exception>
    public async Task<User> Register(RegisterRequest request)
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.MotDePasse);

        // Email déjà présent : conflit si actif, réactivation si soft-deleted.
        var existing = await userRepository.FindByEmail(request.Email);
        if (existing is not null)
        {
            if (existing.Actif)
                throw new ConflictException("Email déjà utilisé.");

            await userRepository.UpdateInfos(new UpdateUserRequest
            {
                Id        = existing.Id,
                Nom       = request.Nom,
                Prenom    = request.Prenom,
                Email     = request.Email,
                Telephone = request.Telephone
            });
            await userRepository.UpdatePassword(existing.Id, hashedPassword);
            await userRepository.Reactivate(existing.Id);

            return new User
            {
                Id          = existing.Id,
                Nom         = request.Nom,
                Prenom      = request.Prenom,
                Email       = request.Email,
                Telephone   = request.Telephone,
                PointsSolde = existing.PointsSolde,
                Role        = "Client"
            };
        }

        try
        {
            var id = await userRepository.Create(request, hashedPassword);
            return new User
            {
                Id          = id,
                Nom         = request.Nom,
                Prenom      = request.Prenom,
                Email       = request.Email,
                Telephone   = request.Telephone,
                PointsSolde = 0,
                Role        = "Client"
            };
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            // Filet de sécurité contre une race condition (création concurrente du même email).
            throw new ConflictException("Email déjà utilisé.");
        }
    }

    /// <summary>
    /// Retourne tous les utilisateurs sans leur mot de passe.
    /// </summary>
    /// <returns>Liste des utilisateurs.</returns>
    public async Task<IEnumerable<User>> GetAll()
    {
        var dbs = await userRepository.GetAll();
        return dbs.Select(db => new User
        {
            Id          = db.Id,
            Nom         = db.Nom,
            Prenom      = db.Prenom,
            Email       = db.Email,
            Telephone   = db.Telephone,
            PointsSolde = db.PointsSolde,
            Role        = "Client"
        });
    }

    /// <summary>
    /// Met à jour les informations personnelles d'un utilisateur (nom, prénom, email, téléphone).
    /// </summary>
    /// <param name="request">Nouvelles données de l'utilisateur, identifié par son Id.</param>
    /// <returns>True si l'utilisateur a été mis à jour, false s'il est introuvable.</returns>
    /// <exception cref="ConflictException">Si le nouvel email est déjà utilisé par un autre utilisateur.</exception>
    public async Task<bool> UpdateInfos(UpdateUserRequest request)
    {
        try
        {
            var rows = await userRepository.UpdateInfos(request);
            return rows > 0;
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            throw new ConflictException("Email déjà utilisé.");
        }
    }

    /// <summary>
    /// Hash le nouveau mot de passe en BCrypt puis le persiste pour l'utilisateur ciblé.
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur.</param>
    /// <param name="nouveauMotDePasse">Nouveau mot de passe en clair (hashé ici avant insertion).</param>
    /// <returns>True si le mot de passe a été mis à jour, false si l'utilisateur est introuvable.</returns>
    public async Task<bool> UpdatePassword(int id, string nouveauMotDePasse)
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(nouveauMotDePasse);
        var rows = await userRepository.UpdatePassword(id, hashedPassword);
        return rows > 0;
    }

    /// <summary>
    /// Supprime le compte d'un utilisateur en soft-delete : l'utilisateur est désactivé
    /// (actif = FALSE) au lieu d'être supprimé physiquement, ce qui préserve l'historique
    /// (réservations passées, transactions fidélité) et évite toute violation de contrainte FK.
    /// Les réservations futures sont annulées (créneaux libérés) et la ligne EMPLOYE éventuelle
    /// est désactivée (employé non planifiable, RG-07).
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur à désactiver.</param>
    /// <returns>True si l'utilisateur a été désactivé, false s'il est introuvable ou déjà inactif.</returns>
    public async Task<bool> DeleteAccount(int id)
    {
        // Annuler les réservations futures pour libérer les créneaux (les passées restent pour l'historique).
        await reservationRepository.DeleteFutureByUserId(id);

        // Désactiver la ligne EMPLOYE éventuelle pour le rendre non planifiable.
        var employeDb = await employeRepository.FindByIdUtilisateur(id);
        if (employeDb is not null)
            await employeRepository.Deactivate(employeDb.IdEmploye);

        // Soft-delete : désactivation du compte.
        var rows = await userRepository.Deactivate(id);
        return rows > 0;
    }
}
