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
    /// Indique si l'utilisateur existe et est actif (non soft-deleted).
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur.</param>
    /// <returns>True si l'utilisateur est actif, false sinon.</returns>
    public async Task<bool> IsActive(int id)
    {
        const string sql = @"
            SELECT COUNT(1)
            FROM UTILISATEUR WHERE id_utilisateur = @Id AND actif = TRUE";
        return await connection.ExecuteScalarAsync<bool>(sql, new { Id = id });
    }

    /// <summary>
    /// Retourne le hash BCrypt du mot de passe d'un utilisateur actif. Null si introuvable ou désactivé.
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur.</param>
    /// <returns>Le hash BCrypt stocké, ou null.</returns>
    public async Task<string?> GetPasswordHash(int id)
    {
        const string sql = @"
            SELECT mot_de_passe
            FROM UTILISATEUR WHERE id_utilisateur = @Id AND actif = TRUE LIMIT 1";
        return await connection.QueryFirstOrDefaultAsync<string?>(sql, new { Id = id });
    }

    /// <summary>
    /// Recherche un utilisateur actif par son identifiant. Retourne null si introuvable ou désactivé.
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur.</param>
    /// <returns>L'utilisateur correspondant (sans le hash du mot de passe), ou null.</returns>
    public async Task<UserDb?> FindById(int id)
    {
        const string sql = @"
            SELECT id_utilisateur AS Id, nom AS Nom, prenom AS Prenom, email AS Email,
                   telephone AS Telephone, points_solde AS PointsSolde, actif AS Actif
            FROM UTILISATEUR WHERE id_utilisateur = @Id AND actif = TRUE LIMIT 1";
        return await connection.QueryFirstOrDefaultAsync<UserDb>(sql, new { Id = id });
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

    // ── Administration ────────────────────────────────────────────

    /// <summary>
    /// Retourne tous les utilisateurs (actifs ET bloqués) avec leur rôle résolu,
    /// pour la console d'administration. Le rôle vaut l'accès de l'EMPLOYE actif, sinon 'Client'.
    /// </summary>
    /// <returns>La liste complète des utilisateurs vue administrateur.</returns>
    public async Task<IEnumerable<UserAdminDb>> GetAllForAdmin()
    {
        const string sql = @"
            SELECT u.id_utilisateur AS Id, u.nom AS Nom, u.prenom AS Prenom, u.email AS Email,
                   u.points_solde AS PointsSolde, u.actif AS Actif,
                   CASE WHEN e.id_employe IS NOT NULL AND e.actif = TRUE
                        THEN e.acces ELSE 'Client' END AS Role
            FROM UTILISATEUR u
            LEFT JOIN EMPLOYE e ON e.id_utilisateur = u.id_utilisateur
            ORDER BY u.nom, u.prenom";
        return await connection.QueryAsync<UserAdminDb>(sql);
    }

    /// <summary>
    /// Crée ou réactive la ligne EMPLOYE d'un utilisateur avec le niveau d'accès donné.
    /// </summary>
    /// <param name="idUtilisateur">Identifiant de l'utilisateur.</param>
    /// <param name="acces">Niveau d'accès staff ('Employe', 'Cuisine', 'Administrateur').</param>
    public async Task UpsertEmploye(int idUtilisateur, string acces)
    {
        const string sql = @"
            INSERT INTO EMPLOYE (id_utilisateur, acces, actif)
            VALUES (@IdUtilisateur, @Acces, TRUE)
            ON DUPLICATE KEY UPDATE acces = @Acces, actif = TRUE";
        await connection.ExecuteAsync(sql, new { IdUtilisateur = idUtilisateur, Acces = acces });
    }

    /// <summary>
    /// Supprime la ligne EMPLOYE d'un utilisateur (rétrogradation au rôle Client).
    /// </summary>
    /// <param name="idUtilisateur">Identifiant de l'utilisateur.</param>
    public async Task DeleteEmploye(int idUtilisateur)
    {
        const string sql = "DELETE FROM EMPLOYE WHERE id_utilisateur = @IdUtilisateur";
        await connection.ExecuteAsync(sql, new { IdUtilisateur = idUtilisateur });
    }

    /// <summary>
    /// Compte les administrateurs actifs (EMPLOYE actif + compte utilisateur non bloqué).
    /// </summary>
    /// <returns>Le nombre d'administrateurs actifs.</returns>
    public async Task<int> CountActiveAdmins()
    {
        const string sql = @"
            SELECT COUNT(*)
            FROM EMPLOYE e
            JOIN UTILISATEUR u ON u.id_utilisateur = e.id_utilisateur
            WHERE e.acces = 'Administrateur' AND e.actif = TRUE AND u.actif = TRUE";
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    /// <summary>
    /// Indique si l'utilisateur est un administrateur actif (rôle Administrateur + compte non bloqué).
    /// </summary>
    /// <param name="idUtilisateur">Identifiant de l'utilisateur.</param>
    /// <returns>True si administrateur actif.</returns>
    public async Task<bool> IsActiveAdmin(int idUtilisateur)
    {
        const string sql = @"
            SELECT COUNT(1)
            FROM EMPLOYE e
            JOIN UTILISATEUR u ON u.id_utilisateur = e.id_utilisateur
            WHERE e.id_utilisateur = @Id AND e.acces = 'Administrateur'
              AND e.actif = TRUE AND u.actif = TRUE";
        return await connection.ExecuteScalarAsync<bool>(sql, new { Id = idUtilisateur });
    }

    /// <summary>
    /// Ajuste le solde de points d'un utilisateur de manière atomique (RG-03) :
    /// met à jour points_solde et insère une ligne TRANSACTION_FIDELITE dans la même
    /// transaction SQL. Refuse l'opération si le solde résultant serait négatif (RG-02).
    /// </summary>
    /// <param name="idUtilisateur">Identifiant de l'utilisateur.</param>
    /// <param name="montant">Montant signé de l'ajustement (positif = crédit, négatif = débit).</param>
    /// <param name="type">Type de transaction stocké ('Ajustement_credit' ou 'Ajustement_debit').</param>
    /// <param name="motif">Motif de l'ajustement (description).</param>
    /// <returns>True si appliqué, false si le solde deviendrait négatif ou l'utilisateur est introuvable.</returns>
    public async Task<bool> AdjustPoints(int idUtilisateur, decimal montant, string type, string motif)
    {
        if (connection.State != ConnectionState.Open)
            connection.Open();

        using var transaction = connection.BeginTransaction();
        try
        {
            const string updateSql = @"
                UPDATE UTILISATEUR
                SET points_solde = points_solde + @Montant
                WHERE id_utilisateur = @Id AND points_solde + @Montant >= 0";
            var rows = await connection.ExecuteAsync(updateSql,
                new { Montant = montant, Id = idUtilisateur }, transaction);

            if (rows == 0)
            {
                transaction.Rollback();
                return false; // solde insuffisant ou utilisateur introuvable
            }

            const string insertSql = @"
                INSERT INTO TRANSACTION_FIDELITE (id_utilisateur, id_commerce, points, type_transaction, description)
                VALUES (@Id, NULL, @Points, @Type, @Motif)";
            await connection.ExecuteAsync(insertSql,
                new { Id = idUtilisateur, Points = Math.Abs(montant), Type = type, Motif = motif }, transaction);

            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
