using Dapper;
using Infrastructure.Models;
using Infrastructure.Repositories.Abstractions;
using System.Data;

namespace Infrastructure.Repositories;

public class EmployeRepository(IDbConnection connection) : IEmployeRepository
{
    /// <summary>
    /// Recherche l'enregistrement EMPLOYE lié à un utilisateur. Retourne null si l'utilisateur est un client.
    /// </summary>
    /// <param name="idUtilisateur">Identifiant de l'utilisateur.</param>
    /// <returns>L'enregistrement EMPLOYE, ou null.</returns>
    public async Task<EmployeDb?> FindByIdUtilisateur(int idUtilisateur)
    {
        const string sql = @"
            SELECT id_employe AS IdEmploye, id_utilisateur AS IdUtilisateur, acces AS Acces,
                   actif AS Actif, id_commerce_preference AS IdCommercePreference
            FROM EMPLOYE WHERE id_utilisateur = @IdUtilisateur LIMIT 1";
        return await connection.QueryFirstOrDefaultAsync<EmployeDb>(sql, new { IdUtilisateur = idUtilisateur });
    }

    /// <summary>
    /// Retourne tous les employés.
    /// </summary>
    /// <returns>Liste des employés.</returns>
    public async Task<IEnumerable<EmployeDb>> GetAll()
    {
        const string sql = @"
            SELECT id_employe AS IdEmploye, id_utilisateur AS IdUtilisateur, acces AS Acces,
                   actif AS Actif, id_commerce_preference AS IdCommercePreference
            FROM EMPLOYE";
        return await connection.QueryAsync<EmployeDb>(sql);
    }

    /// <summary>
    /// Recherche un employé par son identifiant.
    /// </summary>
    /// <param name="id">Identifiant de l'employé.</param>
    /// <returns>L'employé correspondant, ou null.</returns>
    public async Task<EmployeDb?> GetById(int id)
    {
        const string sql = @"
            SELECT id_employe AS IdEmploye, id_utilisateur AS IdUtilisateur, acces AS Acces,
                   actif AS Actif, id_commerce_preference AS IdCommercePreference
            FROM EMPLOYE WHERE id_employe = @Id LIMIT 1";
        return await connection.QueryFirstOrDefaultAsync<EmployeDb>(sql, new { Id = id });
    }

    /// <summary>
    /// Insère un nouvel employé.
    /// </summary>
    /// <param name="employe">Données de l'employé à insérer.</param>
    /// <returns>L'identifiant généré.</returns>
    public async Task<int> Create(EmployeDb employe)
    {
        const string sql = @"
            INSERT INTO EMPLOYE (id_utilisateur, acces, actif, id_commerce_preference)
            VALUES (@IdUtilisateur, @Acces, @Actif, @IdCommercePreference);
            SELECT LAST_INSERT_ID();";
        return await connection.ExecuteScalarAsync<int>(sql, employe);
    }

    /// <summary>
    /// Met à jour les données d'un employé existant.
    /// </summary>
    /// <param name="employe">Données mises à jour.</param>
    public async Task Update(EmployeDb employe)
    {
        const string sql = @"
            UPDATE EMPLOYE SET acces = @Acces, actif = @Actif, id_commerce_preference = @IdCommercePreference
            WHERE id_employe = @IdEmploye";
        await connection.ExecuteAsync(sql, employe);
    }

    /// <summary>
    /// Supprime un employé par son identifiant.
    /// </summary>
    /// <param name="id">Identifiant de l'employé à supprimer.</param>
    public async Task Delete(int id)
    {
        const string sql = "DELETE FROM EMPLOYE WHERE id_employe = @Id";
        await connection.ExecuteAsync(sql, new { Id = id });
    }
}
