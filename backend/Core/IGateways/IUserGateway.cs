using Core.Models;

namespace Core.IGateways;

public interface IUserGateway
{
    Task<User?> Authenticate(string email, string motDePasse);
    Task<User> Register(RegisterRequest request);
    Task<IEnumerable<User>> GetAll();
    Task<User?> GetProfile(int id);
    Task<bool> IsActive(int id);
    Task<bool> UpdateInfos(UpdateUserRequest request);
    Task<bool> UpdatePassword(int id, string ancienMotDePasse, string nouveauMotDePasse);
    Task<bool> DeleteAccount(int id);
}
