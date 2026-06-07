using Core.IGateways;
using Core.Models;
using Infrastructure.Repositories.Abstractions;

namespace Infrastructure.Gateways;

public class FideliteGateway(IFideliteRepository fideliteRepository) : IFideliteGateway
{
    public async Task<CarteFidelite?> GetByUserId(int userId)
    {
        var db = await fideliteRepository.GetByUserId(userId);
        if (db is null) return null;
        return new CarteFidelite { Id = db.Id, UserId = db.UserId, Points = db.Points };
    }

    public async Task<CarteFidelite> Create(int userId)
    {
        var id = await fideliteRepository.CreateCarte(userId);
        return new CarteFidelite { Id = id, UserId = userId, Points = 0 };
    }

    public Task AddPoints(string carteId, int points, string? description)
        => fideliteRepository.AddPoints(carteId, points, description);

    public async Task<IEnumerable<Transaction>> GetTransactions(string carteId)
    {
        var dbs = await fideliteRepository.GetTransactions(carteId);
        return dbs.Select(db => new Transaction { Id = db.Id, CarteFideliteId = db.CarteFideliteId, Points = db.Points, Description = db.Description });
    }
}
