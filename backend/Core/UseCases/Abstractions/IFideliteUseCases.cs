using Core.Models;

namespace Core.UseCases.Abstractions;

public interface IFideliteUseCases
{
    Task<CarteFidelite?> GetCarteFidelite(int userId);
    Task<CarteFidelite> CreateCarte(int userId);
    Task AddPoints(string carteId, int points, string? description);
    Task<IEnumerable<Transaction>> GetTransactions(string carteId);
}
