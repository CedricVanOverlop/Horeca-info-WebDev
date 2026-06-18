using Infrastructure.Models;

namespace Infrastructure.Repositories.Abstractions;

public interface ITarifRepository
{
    Task<IEnumerable<TarifDb>> GetByTerrain(string terrainId);
    Task<TarifDb?> GetById(string id);
    Task<string> Create(TarifDb tarif);
    Task Update(TarifDb tarif);
    Task Delete(string id);
}
