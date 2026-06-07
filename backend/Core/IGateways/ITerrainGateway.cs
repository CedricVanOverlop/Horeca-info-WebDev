using Core.Models;

namespace Core.IGateways;

public interface ITerrainGateway
{
    Task<IEnumerable<Terrain>> GetAll();
    Task<Terrain?> GetById(string id);
    Task<Terrain> Create(Terrain terrain);
    Task Update(Terrain terrain);
}
