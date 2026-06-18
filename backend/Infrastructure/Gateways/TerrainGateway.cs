using Core.IGateways;
using Core.Models;
using Infrastructure.Models;
using Infrastructure.Repositories.Abstractions;

namespace Infrastructure.Gateways;

public class TerrainGateway(ITerrainRepository terrainRepository) : ITerrainGateway
{
    public async Task<IEnumerable<Terrain>> GetAll()
    {
        var dbs = await terrainRepository.GetAll();
        return dbs.Select(Map);
    }

    public async Task<Terrain?> GetById(string id)
    {
        var db = await terrainRepository.GetById(id);
        return db is null ? null : Map(db);
    }

    public async Task<Terrain> Create(Terrain terrain)
    {
        var db = new TerrainDb
        {
            Nom            = terrain.Nom,
            Disponible     = terrain.Disponible,
            HeureOuverture = terrain.HeureOuverture,
            HeureFermeture = terrain.HeureFermeture,
            IdCommerce     = terrain.IdCommerce
        };
        terrain.Id = await terrainRepository.Create(db);
        return terrain;
    }

    public async Task Update(Terrain terrain)
    {
        var db = new TerrainDb
        {
            Id             = terrain.Id,
            Nom            = terrain.Nom,
            Disponible     = terrain.Disponible,
            HeureOuverture = terrain.HeureOuverture,
            HeureFermeture = terrain.HeureFermeture,
            IdCommerce     = terrain.IdCommerce
        };
        await terrainRepository.Update(db);
    }

    // Le type du terrain est inclus dans le nom ("Terrain 1 — Couvert") : pas de colonne SQL dédiée.
    private static Terrain Map(TerrainDb db) => new()
    {
        Id             = db.Id,
        Nom            = db.Nom,
        Type           = string.Empty,
        Disponible     = db.Disponible,
        HeureOuverture = db.HeureOuverture,
        HeureFermeture = db.HeureFermeture,
        IdCommerce     = db.IdCommerce
    };
}
