using Core.IGateways;
using Core.Models;
using Core.UseCases.Abstractions;

namespace Core.UseCases;

public class UserUseCases(IUserGateway userGateway) : IUserUseCases
{
    public Task<User?> Authenticate(AuthenticationRequest request)
        => userGateway.Authenticate(request.Email, request.MotDePasse);

    public Task<User> Register(RegisterRequest request)
        => userGateway.Register(request);

    public Task<IEnumerable<User>> GetAll()
        => userGateway.GetAll();
}
