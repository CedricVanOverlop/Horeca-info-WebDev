using Core.IGateways;
using Core.Models;
using Infrastructure.Repositories;
using Infrastructure.Repositories.Abstractions;

namespace Infrastructure.Gateways;

public class UserGateway(IUserRepository userRepository) : IUserGateway
{
    public async Task<User?> Authenticate(string email, string motDePasse)
    {
        var hashed = UserRepository.HashPassword(motDePasse);
        var db = await userRepository.FindByEmailAndPassword(email, hashed);
        if (db is null) return null;
        return new User
        {
            Id = db.Id,
            Nom = db.Nom,
            Prenom = db.Prenom,
            Email = db.Email,
            Telephone = db.Telephone,
            Role = db.Role
        };
    }

    public async Task<User> Register(RegisterRequest request)
    {
        var hashed = UserRepository.HashPassword(request.MotDePasse);
        var id = await userRepository.Create(request, hashed);
        return new User
        {
            Id = id,
            Nom = request.Nom,
            Prenom = request.Prenom,
            Email = request.Email,
            Telephone = request.Telephone,
            Role = "Client"
        };
    }

    public async Task<IEnumerable<User>> GetAll()
    {
        var dbs = await userRepository.GetAll();
        return dbs.Select(db => new User
        {
            Id = db.Id,
            Nom = db.Nom,
            Prenom = db.Prenom,
            Email = db.Email,
            Telephone = db.Telephone,
            Role = db.Role
        });
    }
}
