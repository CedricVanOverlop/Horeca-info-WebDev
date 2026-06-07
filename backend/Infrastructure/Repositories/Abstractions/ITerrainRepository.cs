using Infrastructure.Models;

namespace Infrastructure.Repositories.Abstractions;

public interface ITerrainRepository
{
    Task<IEnumerable<TerrainDb>> GetAll();
    Task<TerrainDb?> GetById(string id);
    Task<string> Create(TerrainDb terrain);
    Task Update(TerrainDb terrain);
}
