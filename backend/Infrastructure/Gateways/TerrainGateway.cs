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
        var db = new TerrainDb { Nom = terrain.Nom, Type = terrain.Type, Disponible = terrain.Disponible };
        terrain.Id = await terrainRepository.Create(db);
        return terrain;
    }

    public async Task Update(Terrain terrain)
    {
        var db = new TerrainDb { Id = terrain.Id, Nom = terrain.Nom, Type = terrain.Type, Disponible = terrain.Disponible };
        await terrainRepository.Update(db);
    }

    private static Terrain Map(TerrainDb db) => new()
    { Id = db.Id, Nom = db.Nom, Type = db.Type, Disponible = db.Disponible };
}
