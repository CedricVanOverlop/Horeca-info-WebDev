using Core.Models;
using Dapper;
using Infrastructure.Models;
using Infrastructure.Repositories.Abstractions;
using System.Data;

namespace Infrastructure.Repositories;

public class EmployeRepository(IDbConnection connection) : IEmployeRepository
{
    public async Task<IEnumerable<EmployeDb>> GetAll()
    {
        const string sql = "SELECT * FROM Employes";
        return await connection.QueryAsync<EmployeDb>(sql);
    }

    public async Task<EmployeDb?> GetById(string id)
    {
        const string sql = "SELECT * FROM Employes WHERE Id = @Id LIMIT 1";
        return await connection.QueryFirstOrDefaultAsync<EmployeDb>(sql, new { Id = id });
    }

    public async Task<string> Create(Employe employe)
    {
        const string sql = @"
            INSERT INTO Employes (Nom, Prenom, Email, Poste)
            VALUES (@Nom, @Prenom, @Email, @Poste);
            SELECT LAST_INSERT_ID();";
        return (await connection.ExecuteScalarAsync<object>(sql, employe))?.ToString() ?? string.Empty;
    }

    public async Task Update(Employe employe)
    {
        const string sql = "UPDATE Employes SET Nom=@Nom, Prenom=@Prenom, Email=@Email, Poste=@Poste WHERE Id=@Id";
        await connection.ExecuteAsync(sql, employe);
    }

    public async Task Delete(string id)
    {
        const string sql = "DELETE FROM Employes WHERE Id = @Id";
        await connection.ExecuteAsync(sql, new { Id = id });
    }
}
