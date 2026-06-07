using Dapper;
using Infrastructure.Models;
using Infrastructure.Repositories.Abstractions;
using System.Data;

namespace Infrastructure.Repositories;

public class TerrainRepository(IDbConnection connection) : ITerrainRepository
{
    public async Task<IEnumerable<TerrainDb>> GetAll()
    {
        const string sql = "SELECT * FROM Terrains";
        return await connection.QueryAsync<TerrainDb>(sql);
    }

    public async Task<TerrainDb?> GetById(string id)
    {
        const string sql = "SELECT * FROM Terrains WHERE Id = @Id LIMIT 1";
        return await connection.QueryFirstOrDefaultAsync<TerrainDb>(sql, new { Id = id });
    }

    public async Task<string> Create(TerrainDb terrain)
    {
        const string sql = @"
            INSERT INTO Terrains (Nom, Type, Disponible)
            VALUES (@Nom, @Type, @Disponible);
            SELECT LAST_INSERT_ID();";
        return (await connection.ExecuteScalarAsync<object>(sql, terrain))?.ToString() ?? string.Empty;
    }

    public async Task Update(TerrainDb terrain)
    {
        const string sql = "UPDATE Terrains SET Nom=@Nom, Type=@Type, Disponible=@Disponible WHERE Id=@Id";
        await connection.ExecuteAsync(sql, terrain);
    }
}
