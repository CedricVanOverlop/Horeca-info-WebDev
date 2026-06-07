using Core.Models;

namespace Core.UseCases.Abstractions;

public interface IUserUseCases
{
    Task<User?> Authenticate(AuthenticationRequest request);
    Task<User> Register(RegisterRequest request);
    Task<IEnumerable<User>> GetAll();
}
