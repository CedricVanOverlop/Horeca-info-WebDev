using Core.Models;
using Infrastructure.Models;

namespace Infrastructure.Repositories.Abstractions;

public interface IUserRepository
{
    Task<UserDb?> FindByEmail(string email);
    Task<UserDb?> FindById(int id);
    Task<string?> GetPasswordHash(int id);
    Task<bool> IsActive(int id);
    Task<int> Create(RegisterRequest request, string hashedPassword);
    Task<IEnumerable<UserDb>> GetAll();
    Task<int> UpdateInfos(UpdateUserRequest request);
    Task<int> UpdatePassword(int id, string hashedPassword);
    Task<int> Deactivate(int id);
    Task<int> Reactivate(int id);
}
