using Core.Models;
using Dapper;
using Infrastructure.Models;
using Infrastructure.Repositories.Abstractions;
using System.Data;

namespace Infrastructure.Repositories;

public class UserRepository(IDbConnection connection) : IUserRepository
{
    /// <summary>
    /// Recherche un utilisateur par email. Retourne null si introuvable.
    /// </summary>
    /// <param name="email">Email de l'utilisateur.</param>
    /// <returns>L'utilisateur correspondant, ou null.</returns>
    public async Task<UserDb?> FindByEmail(string email)
    {
        const string sql = @"
            SELECT id_utilisateur AS Id, nom AS Nom, prenom AS Prenom, email AS Email,
                   mot_de_passe AS MotDePasse, telephone AS Telephone, points_solde AS PointsSolde
            FROM UTILISATEUR WHERE email = @Email LIMIT 1";
        return await connection.QueryFirstOrDefaultAsync<UserDb>(sql, new { Email = email });
    }

    /// <summary>
    /// Insère un nouvel utilisateur avec le mot de passe déjà hashé en BCrypt.
    /// </summary>
    /// <param name="request">Données d'inscription.</param>
    /// <param name="hashedPassword">Hash BCrypt du mot de passe.</param>
    /// <returns>L'identifiant généré.</returns>
    public async Task<int> Create(RegisterRequest request, string hashedPassword)
    {
        const string sql = @"
            INSERT INTO UTILISATEUR (nom, prenom, email, mot_de_passe, telephone, points_solde)
            VALUES (@Nom, @Prenom, @Email, @MotDePasse, @Telephone, 0);
            SELECT LAST_INSERT_ID();";
        return await connection.ExecuteScalarAsync<int>(sql, new
        {
            request.Nom,
            request.Prenom,
            request.Email,
            MotDePasse = hashedPassword,
            request.Telephone
        });
    }

    /// <summary>
    /// Retourne tous les utilisateurs sans leur mot de passe.
    /// </summary>
    /// <returns>Liste des utilisateurs.</returns>
    public async Task<IEnumerable<UserDb>> GetAll()
    {
        const string sql = @"
            SELECT id_utilisateur AS Id, nom AS Nom, prenom AS Prenom, email AS Email,
                   telephone AS Telephone, points_solde AS PointsSolde
            FROM UTILISATEUR";
        return await connection.QueryAsync<UserDb>(sql);
    }
}
