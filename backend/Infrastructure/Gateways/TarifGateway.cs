using Core.IGateways;
using Core.Models;
using Infrastructure.Models;
using Infrastructure.Repositories.Abstractions;

namespace Infrastructure.Gateways;

public class TarifGateway(ITarifRepository tarifRepository) : ITarifGateway
{
    public async Task<IEnumerable<Tarif>> GetByTerrain(string terrainId)
    {
        var dbs = await tarifRepository.GetByTerrain(terrainId);
        return dbs.Select(Map);
    }

    public async Task<Tarif?> GetById(string id)
    {
        var db = await tarifRepository.GetById(id);
        return db is null ? null : Map(db);
    }

    public async Task<Tarif> Create(Tarif tarif)
    {
        tarif.Id = await tarifRepository.Create(ToDb(tarif));
        return tarif;
    }

    public Task Update(Tarif tarif) => tarifRepository.Update(ToDb(tarif));

    public Task Delete(string id) => tarifRepository.Delete(id);

    private static Tarif Map(TarifDb db) => new()
    {
        Id          = db.Id,
        Type        = db.Type,
        PrixHeure   = db.PrixHeure,
        HeureDebut  = db.HeureDebut,
        HeureFin    = db.HeureFin,
        JourSemaine = db.JourSemaine,
        TerrainId   = db.TerrainId
    };

    private static TarifDb ToDb(Tarif tarif) => new()
    {
        Id          = tarif.Id,
        Type        = tarif.Type,
        PrixHeure   = tarif.PrixHeure,
        HeureDebut  = tarif.HeureDebut,
        HeureFin    = tarif.HeureFin,
        JourSemaine = tarif.JourSemaine,
        TerrainId   = tarif.TerrainId
    };
}
