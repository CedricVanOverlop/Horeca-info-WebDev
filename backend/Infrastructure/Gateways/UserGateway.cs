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
    /// Retourne le profil de l'utilisateur actif identifié par son id, rôle staff résolu
    /// (Client si la ligne EMPLOYE est absente ou inactive).
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur.</param>
    /// <returns>Le profil, ou null si l'utilisateur est introuvable ou désactivé.</returns>
    public async Task<User?> GetProfile(int id)
    {
        var userDb = await userRepository.FindById(id);
        if (userDb is null) return null;

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
    /// Indique si l'utilisateur existe et est actif (non soft-deleted).
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur.</param>
    /// <returns>True si actif, false sinon.</returns>
    public Task<bool> IsActive(int id) => userRepository.IsActive(id);

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
    /// Change le mot de passe : vérifie d'abord l'ancien mot de passe via BCrypt, puis hash
    /// et persiste le nouveau. Empêche un changement avec un simple token volé sans connaître
    /// le mot de passe actuel.
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur.</param>
    /// <param name="ancienMotDePasse">Mot de passe actuel en clair (vérifié contre le hash stocké).</param>
    /// <param name="nouveauMotDePasse">Nouveau mot de passe en clair (hashé ici avant insertion).</param>
    /// <returns>True si mis à jour, false si l'utilisateur est introuvable.</returns>
    /// <exception cref="ValidationException">Si l'ancien mot de passe est incorrect.</exception>
    public async Task<bool> UpdatePassword(int id, string ancienMotDePasse, string nouveauMotDePasse)
    {
        var currentHash = await userRepository.GetPasswordHash(id);
        if (currentHash is null) return false;

        bool ancienValide;
        try
        {
            ancienValide = BCrypt.Net.BCrypt.Verify(ancienMotDePasse, currentHash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            ancienValide = false;
        }
        if (!ancienValide)
            throw new ValidationException("Mot de passe actuel incorrect.");

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

    // ── Administration ────────────────────────────────────────────

    /// <summary>
    /// Retourne tous les utilisateurs (actifs et bloqués) avec leur rôle résolu, pour l'administration.
    /// </summary>
    /// <returns>La liste complète des utilisateurs.</returns>
    public async Task<IEnumerable<UserAdmin>> GetAllForAdmin()
    {
        var dbs = await userRepository.GetAllForAdmin();
        return dbs.Select(db => new UserAdmin
        {
            Id          = db.Id,
            Nom         = db.Nom,
            Prenom      = db.Prenom,
            Email       = db.Email,
            PointsSolde = db.PointsSolde,
            Role        = db.Role,
            Actif       = db.Actif
        });
    }

    /// <summary>
    /// Change le niveau d'accès d'un utilisateur. 'Client' supprime la ligne EMPLOYE,
    /// les rôles staff la créent ou la mettent à jour.
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur.</param>
    /// <param name="acces">Niveau d'accès cible.</param>
    public async Task ChangeRole(int id, string acces)
    {
        if (acces == "Client")
            await userRepository.DeleteEmploye(id);
        else
            await userRepository.UpsertEmploye(id, acces);
    }

    /// <summary>
    /// Indique si l'utilisateur est le dernier administrateur actif du système.
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur.</param>
    /// <returns>True si c'est un administrateur actif et le seul restant.</returns>
    public async Task<bool> IsLastActiveAdmin(int id)
    {
        if (!await userRepository.IsActiveAdmin(id))
            return false;
        return await userRepository.CountActiveAdmins() <= 1;
    }

    /// <summary>
    /// Ajuste le solde de points (RG-03). Le signe du montant détermine le type de transaction.
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur.</param>
    /// <param name="montant">Montant signé de l'ajustement.</param>
    /// <param name="motif">Motif de l'ajustement.</param>
    /// <returns>True si appliqué, false si le solde deviendrait négatif (RG-02).</returns>
    public Task<bool> AdjustPoints(int id, decimal montant, string motif)
    {
        // type_transaction contraint à { GAIN, DEPENSE, EXPIRATION, AJUSTEMENT } :
        // crédit = GAIN, débit = DEPENSE (cohérent avec la formule du solde).
        var type = montant >= 0 ? "GAIN" : "DEPENSE";
        return userRepository.AdjustPoints(id, montant, type, motif);
    }

    /// <summary>
    /// Bloque un compte : désactivation (actif = FALSE), l'utilisateur ne peut plus se connecter.
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur.</param>
    /// <returns>True si bloqué, false si introuvable ou déjà bloqué.</returns>
    public async Task<bool> Block(int id)
    {
        var rows = await userRepository.Deactivate(id);
        return rows > 0;
    }

    /// <summary>
    /// Débloque un compte : réactivation (actif = TRUE).
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur.</param>
    /// <returns>True si débloqué.</returns>
    public async Task<bool> Unblock(int id)
    {
        var rows = await userRepository.Reactivate(id);
        return rows > 0;
    }

    /// <summary>
    /// Retourne les réservations d'un utilisateur, vue administrateur.
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur.</param>
    /// <returns>Les réservations de l'utilisateur.</returns>
    public async Task<IEnumerable<ReservationAdmin>> GetReservations(int id)
    {
        var dbs = await userRepository.GetReservationsByUtilisateur(id);
        return dbs.Select(db => new ReservationAdmin
        {
            Id         = db.Id,
            Date       = db.Date,
            HeureDebut = db.HeureDebut,
            HeureFin   = db.HeureFin,
            PrixPaye   = db.PrixPaye,
            Terrain    = db.Terrain
        });
    }

    /// <summary>
    /// Retourne les horaires de travail d'un utilisateur employé, vue administrateur.
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur.</param>
    /// <returns>Les horaires de l'utilisateur.</returns>
    public async Task<IEnumerable<HoraireAdmin>> GetHoraires(int id)
    {
        var dbs = await userRepository.GetHorairesByUtilisateur(id);
        return dbs.Select(db => new HoraireAdmin
        {
            Id         = db.Id,
            Date       = db.Date,
            HeureDebut = db.HeureDebut,
            HeureFin   = db.HeureFin,
            HeurePayee = db.HeurePayee,
            Statut     = db.Statut,
            Commerce   = db.Commerce
        });
    }
}
