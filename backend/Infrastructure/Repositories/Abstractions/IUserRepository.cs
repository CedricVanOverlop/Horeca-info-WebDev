using Core.Models;
using Infrastructure.Models;

namespace Infrastructure.Repositories.Abstractions;

public interface IUserRepository
{
    Task<UserDb?> FindByEmailAndPassword(string email, string hashedPassword);
    Task<int> Create(RegisterRequest request, string hashedPassword);
    Task<IEnumerable<UserDb>> GetAll();
}
