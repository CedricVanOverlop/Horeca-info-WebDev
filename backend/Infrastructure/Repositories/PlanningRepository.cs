using Dapper;
using Infrastructure.Models;
using Infrastructure.Repositories.Abstractions;
using System.Data;

namespace Infrastructure.Repositories;

public class PlanningRepository(IDbConnection connection) : IPlanningRepository
{
    public async Task<IEnumerable<PlanningDb>> GetAll()
    {
        const string sql = "SELECT * FROM Plannings ORDER BY DateDebut";
        return await connection.QueryAsync<PlanningDb>(sql);
    }

    public async Task<IEnumerable<PlanningDb>> GetByEmployeId(string employeId)
    {
        const string sql = "SELECT * FROM Plannings WHERE EmployeId = @EmployeId ORDER BY DateDebut";
        return await connection.QueryAsync<PlanningDb>(sql, new { EmployeId = employeId });
    }

    public async Task<string> Create(PlanningDb planning)
    {
        const string sql = @"
            INSERT INTO Plannings (EmployeId, DateDebut, DateFin)
            VALUES (@EmployeId, @DateDebut, @DateFin);
            SELECT LAST_INSERT_ID();";
        return (await connection.ExecuteScalarAsync<object>(sql, planning))?.ToString() ?? string.Empty;
    }

    public async Task Delete(string id)
    {
        const string sql = "DELETE FROM Plannings WHERE Id = @Id";
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    /// <summary>
    /// Retourne les horaires de travail d'un utilisateur employé (avec le nom du commerce),
    /// les plus récents d'abord. Vide s'il n'est pas employé.
    /// </summary>
    /// <param name="idUtilisateur">Identifiant de l'utilisateur.</param>
    /// <returns>Les horaires de l'utilisateur.</returns>
    public async Task<IEnumerable<HoraireAdminDb>> GetHorairesByUtilisateur(int idUtilisateur)
    {
        const string sql = @"
            SELECT h.id_horaire AS Id, h.date AS Date, h.heure_debut AS HeureDebut,
                   h.heure_fin AS HeureFin, h.heure_payee AS HeurePayee, h.statut AS Statut,
                   c.nom AS Commerce
            FROM HORAIRE h
            JOIN EMPLOYE e ON e.id_employe = h.id_employe
            JOIN COMMERCE c ON c.id_commerce = h.id_commerce
            WHERE e.id_utilisateur = @Id
            ORDER BY h.date DESC, h.heure_debut DESC";
        return await connection.QueryAsync<HoraireAdminDb>(sql, new { Id = idUtilisateur });
    }
}
