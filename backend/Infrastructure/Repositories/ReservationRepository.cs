using Dapper;
using Infrastructure.Models;
using Infrastructure.Repositories.Abstractions;
using System.Data;

namespace Infrastructure.Repositories;

public class ReservationRepository(IDbConnection connection) : IReservationRepository
{
    public async Task<IEnumerable<ReservationDb>> GetAll()
    {
        const string sql = "SELECT * FROM Reservations ORDER BY DateDebut";
        return await connection.QueryAsync<ReservationDb>(sql);
    }

    public async Task<IEnumerable<ReservationDb>> GetByUserId(int userId)
    {
        const string sql = "SELECT * FROM Reservations WHERE UserId = @UserId ORDER BY DateDebut";
        return await connection.QueryAsync<ReservationDb>(sql, new { UserId = userId });
    }

    public async Task<string> Create(ReservationDb reservation)
    {
        const string sql = @"
            INSERT INTO Reservations (UserId, TerrainId, DateDebut, DateFin, Prix)
            VALUES (@UserId, @TerrainId, @DateDebut, @DateFin, @Prix);
            SELECT LAST_INSERT_ID();";
        return (await connection.ExecuteScalarAsync<object>(sql, reservation))?.ToString() ?? string.Empty;
    }

    public async Task Delete(string id)
    {
        const string sql = "DELETE FROM Reservations WHERE Id = @Id";
        await connection.ExecuteAsync(sql, new { Id = id });
    }
}
