using Core.Models;
using Infrastructure.Models;

namespace Infrastructure.Repositories.Abstractions;

public interface IUserRepository
{
    Task<UserDb?> FindByEmail(string email);
    Task<int> Create(RegisterRequest request, string hashedPassword);
    Task<IEnumerable<UserDb>> GetAll();
}
