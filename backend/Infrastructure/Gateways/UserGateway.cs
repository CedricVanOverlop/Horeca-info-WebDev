using Core.Exceptions;
using Core.IGateways;
using Core.Models;
using Infrastructure.Repositories.Abstractions;
using MySql.Data.MySqlClient;

namespace Infrastructure.Gateways;

public class UserGateway(IUserRepository userRepository, IEmployeRepository employeRepository) : IUserGateway
{
    /// <summary>
    /// Authentifie un utilisateur et retourne son profil métier si les credentials sont valides.
    /// </summary>
    /// <param name="email">Adresse email de l'utilisateur.</param>
    /// <param name="motDePasse">Mot de passe en clair (vérifié contre le hash BCrypt).</param>
    /// <returns>L'utilisateur authentifié, ou null si credentials invalides ou employé inactif.</returns>
    public async Task<User?> Authenticate(string email, string motDePasse)
    {
        var userDb = await userRepository.FindByEmail(email);
        if (userDb is null) return null;
        if (!BCrypt.Net.BCrypt.Verify(motDePasse, userDb.MotDePasse)) return null;

        var employeDb = await employeRepository.FindByIdUtilisateur(userDb.Id);
        if (employeDb is not null && !employeDb.Actif) return null;

        return new User
        {
            Id          = userDb.Id,
            Nom         = userDb.Nom,
            Prenom      = userDb.Prenom,
            Email       = userDb.Email,
            Telephone   = userDb.Telephone,
            PointsSolde = userDb.PointsSolde,
            Role        = employeDb?.Acces ?? "Client"
        };
    }

    /// <summary>
    /// Inscrit un nouvel utilisateur avec le mot de passe hashé en BCrypt.
    /// </summary>
    /// <param name="request">Données d'inscription.</param>
    /// <returns>Le profil de l'utilisateur créé avec le rôle Client.</returns>
    /// <exception cref="ConflictException">Si l'email est déjà utilisé.</exception>
    public async Task<User> Register(RegisterRequest request)
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.MotDePasse);
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
}
