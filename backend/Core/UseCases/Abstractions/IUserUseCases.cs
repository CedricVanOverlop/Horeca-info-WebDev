using Core.Models;

namespace Core.UseCases.Abstractions;

public interface IUserUseCases
{
    Task<User?> Authenticate(AuthenticationRequest request);
    Task<User> Register(RegisterRequest request);
    Task<IEnumerable<User>> GetAll();
    Task<bool> UpdateInfos(int id, UpdateUserRequest request);
    Task<bool> UpdatePassword(int id, ChangePasswordRequest request);
    Task<bool> DeleteAccount(int id);
}
