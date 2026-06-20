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
    /// Désactive un employé (actif = FALSE) sans supprimer la ligne. Le rend non planifiable (RG-07).
    /// </summary>
    /// <param name="id">Identifiant de l'employé à désactiver.</param>
    public async Task Deactivate(int id)
    {
        const string sql = "UPDATE EMPLOYE SET actif = FALSE WHERE id_employe = @Id";
        await connection.ExecuteAsync(sql, new { Id = id });
    }
}
