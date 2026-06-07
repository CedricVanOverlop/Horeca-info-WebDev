using Core.Models;

namespace Core.IGateways;

public interface IUserGateway
{
    Task<User?> Authenticate(string email, string motDePasse);
    Task<User> Register(RegisterRequest request);
    Task<IEnumerable<User>> GetAll();
}
