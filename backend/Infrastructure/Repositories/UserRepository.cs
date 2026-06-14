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
                   mot_de_passe AS MotDePasse, telephone AS Telephone, points_solde AS PointsSolde,
                   actif AS Actif
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
                   telephone AS Telephone, points_solde AS PointsSolde, actif AS Actif
            FROM UTILISATEUR WHERE actif = TRUE";
        return await connection.QueryAsync<UserDb>(sql);
    }


    /// <summary>
    /// Met à jour les informations personnelles d'un utilisateur.
    /// </summary>
    /// <param name="request">Nouvelles données de l'utilisateur.</param>
    /// <returns>Le nombre de lignes affectées (1 si succès, 0 si utilisateur introuvable).</returns>
    public async Task<int> UpdateInfos(UpdateUserRequest request)
    {
        const string sql = @"
            UPDATE UTILISATEUR 
            SET nom = @Nom, prenom = @Prenom, email = @Email, telephone = @Telephone
            WHERE id_utilisateur = @Id";
        return await connection.ExecuteAsync(sql, new
        {
            request.Nom,
            request.Prenom,
            request.Email,
            request.Telephone,
            request.Id
        });
    }

    /// <summary>
    /// Met à jour le mot de passe d'un utilisateur.
    /// </summary>
    /// <param name="id">Id de l'utilisateur.</param>
    /// <param name="hashedPassword"> Nouveau Mot De Passe.</param>
    /// <returns>Le nombre de lignes affectées (1 si succès, 0 si utilisateur introuvable).</returns>
    public async Task<int> UpdatePassword(int id, string hashedPassword)
    {
        const string sql = @"
            UPDATE UTILISATEUR 
            SET mot_de_passe = @MotDePasse
            WHERE id_utilisateur = @Id";
        return await connection.ExecuteAsync(sql, new
        {
            MotDePasse = hashedPassword,
            Id = id
        });
    }

    /// <summary>
    /// Désactive (soft-delete) un utilisateur : passe actif à FALSE sans supprimer la ligne.
    /// Préserve l'historique (réservations passées, transactions fidélité) et les contraintes FK.
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur à désactiver.</param>
    /// <returns>Le nombre de lignes affectées (1 si succès, 0 si utilisateur introuvable ou déjà inactif).</returns>
    public async Task<int> Deactivate(int id)
    {
        const string sql = @"
            UPDATE UTILISATEUR
            SET actif = FALSE
            WHERE id_utilisateur = @Id AND actif = TRUE";
        return await connection.ExecuteAsync(sql, new { Id = id });
    }

    /// <summary>
    /// Réactive (annule le soft-delete) un utilisateur : repasse actif à TRUE.
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur à réactiver.</param>
    /// <returns>Le nombre de lignes affectées.</returns>
    public async Task<int> Reactivate(int id)
    {
        const string sql = @"
            UPDATE UTILISATEUR
            SET actif = TRUE
            WHERE id_utilisateur = @Id";
        return await connection.ExecuteAsync(sql, new { Id = id });
    }
}
