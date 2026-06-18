using Dapper;
using Infrastructure.Models;
using Infrastructure.Repositories.Abstractions;
using System.Data;

namespace Infrastructure.Repositories;

public class TarifRepository(IDbConnection connection) : ITarifRepository
{
    private const string SelectColumns = @"
        CAST(id_tarif AS CHAR) AS Id, type AS Type, prix_heure AS PrixHeure,
        heure_debut AS HeureDebut, heure_fin AS HeureFin, jour_semaine AS JourSemaine,
        CAST(id_terrain AS CHAR) AS TerrainId";

    public async Task<IEnumerable<TarifDb>> GetByTerrain(string terrainId)
    {
        var sql = $@"SELECT {SelectColumns}
                     FROM TARIF
                     WHERE id_terrain = @TerrainId
                     ORDER BY jour_semaine, heure_debut";
        return await connection.QueryAsync<TarifDb>(sql, new { TerrainId = terrainId });
    }

    public async Task<TarifDb?> GetById(string id)
    {
        var sql = $"SELECT {SelectColumns} FROM TARIF WHERE id_tarif = @Id LIMIT 1";
        return await connection.QueryFirstOrDefaultAsync<TarifDb>(sql, new { Id = id });
    }

    public async Task<string> Create(TarifDb tarif)
    {
        const string sql = @"
            INSERT INTO TARIF (type, prix_heure, heure_debut, heure_fin, jour_semaine, id_terrain)
            VALUES (@Type, @PrixHeure, @HeureDebut, @HeureFin, @JourSemaine, @TerrainId);
            SELECT LAST_INSERT_ID();";
        return (await connection.ExecuteScalarAsync<object>(sql, tarif))?.ToString() ?? string.Empty;
    }

    public async Task Update(TarifDb tarif)
    {
        const string sql = @"
            UPDATE TARIF
            SET type = @Type, prix_heure = @PrixHeure, heure_debut = @HeureDebut,
                heure_fin = @HeureFin, jour_semaine = @JourSemaine
            WHERE id_tarif = @Id";
        await connection.ExecuteAsync(sql, tarif);
    }

    public async Task Delete(string id)
    {
        const string sql = "DELETE FROM TARIF WHERE id_tarif = @Id";
        await connection.ExecuteAsync(sql, new { Id = id });
    }
}
