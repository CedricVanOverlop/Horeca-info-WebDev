using Dapper;
using Infrastructure.Models;
using Infrastructure.Repositories.Abstractions;
using System.Data;

namespace Infrastructure.Repositories;

public class TerrainRepository(IDbConnection connection) : ITerrainRepository
{
    private const string SelectColumns = @"
        CAST(id_terrain AS CHAR) AS Id, nom AS Nom, actif AS Disponible,
        heure_ouverture AS HeureOuverture, heure_fermeture AS HeureFermeture,
        id_commerce AS IdCommerce";

    public async Task<IEnumerable<TerrainDb>> GetAll()
    {
        var sql = $"SELECT {SelectColumns} FROM TERRAIN ORDER BY nom";
        return await connection.QueryAsync<TerrainDb>(sql);
    }

    public async Task<TerrainDb?> GetById(string id)
    {
        var sql = $"SELECT {SelectColumns} FROM TERRAIN WHERE id_terrain = @Id LIMIT 1";
        return await connection.QueryFirstOrDefaultAsync<TerrainDb>(sql, new { Id = id });
    }

    public async Task<string> Create(TerrainDb terrain)
    {
        const string sql = @"
            INSERT INTO TERRAIN (nom, actif, heure_ouverture, heure_fermeture, id_commerce)
            VALUES (@Nom, @Disponible, @HeureOuverture, @HeureFermeture, @IdCommerce);
            SELECT LAST_INSERT_ID();";
        return (await connection.ExecuteScalarAsync<object>(sql, terrain))?.ToString() ?? string.Empty;
    }

    public async Task Update(TerrainDb terrain)
    {
        const string sql = @"
            UPDATE TERRAIN
            SET nom = @Nom, actif = @Disponible,
                heure_ouverture = @HeureOuverture, heure_fermeture = @HeureFermeture
            WHERE id_terrain = @Id";
        await connection.ExecuteAsync(sql, terrain);
    }
}
