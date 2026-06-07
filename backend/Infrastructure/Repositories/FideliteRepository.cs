using Dapper;
using Infrastructure.Models;
using Infrastructure.Repositories.Abstractions;
using System.Data;

namespace Infrastructure.Repositories;

public class FideliteRepository(IDbConnection connection) : IFideliteRepository
{
    public async Task<CarteFideliteDb?> GetByUserId(int userId)
    {
        const string sql = "SELECT * FROM CartesFidelite WHERE UserId = @UserId LIMIT 1";
        return await connection.QueryFirstOrDefaultAsync<CarteFideliteDb>(sql, new { UserId = userId });
    }

    public async Task<string> CreateCarte(int userId)
    {
        const string sql = "INSERT INTO CartesFidelite (UserId, Points) VALUES (@UserId, 0); SELECT LAST_INSERT_ID();";
        return (await connection.ExecuteScalarAsync<object>(sql, new { UserId = userId }))?.ToString() ?? string.Empty;
    }

    public async Task AddPoints(string carteId, int points, string? description)
    {
        const string updateSql = "UPDATE CartesFidelite SET Points = Points + @Points WHERE Id = @Id";
        await connection.ExecuteAsync(updateSql, new { Points = points, Id = carteId });

        const string insertSql = "INSERT INTO Transactions (CarteFideliteId, Points, Description) VALUES (@CarteId, @Points, @Description)";
        await connection.ExecuteAsync(insertSql, new { CarteId = carteId, Points = points, Description = description });
    }

    public async Task<IEnumerable<TransactionDb>> GetTransactions(string carteId)
    {
        const string sql = "SELECT * FROM Transactions WHERE CarteFideliteId = @CarteId ORDER BY CreatedAt DESC";
        return await connection.QueryAsync<TransactionDb>(sql, new { CarteId = carteId });
    }
}
