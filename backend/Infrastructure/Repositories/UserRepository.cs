using Core.Models;
using Dapper;
using Infrastructure.Models;
using Infrastructure.Repositories.Abstractions;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Repositories;

public class UserRepository(IDbConnection connection) : IUserRepository
{
    public async Task<UserDb?> FindByEmailAndPassword(string email, string hashedPassword)
    {
        const string sql = "SELECT * FROM Users WHERE Email = @Email AND MotDePasse = @MotDePasse LIMIT 1";
        return await connection.QueryFirstOrDefaultAsync<UserDb>(sql, new { Email = email, MotDePasse = hashedPassword });
    }

    public async Task<int> Create(RegisterRequest request, string hashedPassword)
    {
        const string sql = @"
            INSERT INTO Users (Nom, Prenom, Email, MotDePasse, Telephone, Role)
            VALUES (@Nom, @Prenom, @Email, @MotDePasse, @Telephone, 'Client');
            SELECT LAST_INSERT_ID();";
        return await connection.ExecuteScalarAsync<int>(sql, new
        {
            request.Nom,
            request.Prenom,
            request.Email,
            MotDePasse = hashedPassword,
            request.Telephone
        });
    }

    public async Task<IEnumerable<UserDb>> GetAll()
    {
        const string sql = "SELECT Id, Nom, Prenom, Email, Telephone, Role FROM Users";
        return await connection.QueryAsync<UserDb>(sql);
    }

    public static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLower();
    }
}
