using Infrastructure.Models;

namespace Infrastructure.Repositories.Abstractions;

public interface IFideliteRepository
{
    Task<CarteFideliteDb?> GetByUserId(int userId);
    Task<string> CreateCarte(int userId);
    Task AddPoints(string carteId, int points, string? description);
    Task<IEnumerable<TransactionDb>> GetTransactions(string carteId);
}
