using Core.Models;

namespace Core.IGateways;

public interface IFideliteGateway
{
    Task<CarteFidelite?> GetByUserId(int userId);
    Task<CarteFidelite> Create(int userId);
    Task AddPoints(string carteId, int points, string? description);
    Task<IEnumerable<Transaction>> GetTransactions(string carteId);
}
