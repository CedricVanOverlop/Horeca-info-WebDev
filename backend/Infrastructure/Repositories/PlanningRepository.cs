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
}
