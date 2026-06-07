using Core.IGateways;
using Core.Models;
using Core.UseCases.Abstractions;

namespace Core.UseCases;

public class FideliteUseCases(IFideliteGateway fideliteGateway) : IFideliteUseCases
{
    public Task<CarteFidelite?> GetCarteFidelite(int userId) => fideliteGateway.GetByUserId(userId);
    public Task<CarteFidelite> CreateCarte(int userId) => fideliteGateway.Create(userId);
    public Task AddPoints(string carteId, int points, string? description) => fideliteGateway.AddPoints(carteId, points, description);
    public Task<IEnumerable<Transaction>> GetTransactions(string carteId) => fideliteGateway.GetTransactions(carteId);
}
