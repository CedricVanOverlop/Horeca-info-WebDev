using Core.Models;
using Infrastructure.Models;

namespace Infrastructure.Repositories.Abstractions;

public interface IUserRepository
{
    Task<UserDb?> FindByEmail(string email);
    Task<UserDb?> FindById(int id);
    Task<IEnumerable<UserDb>> Search(string query);
    Task<string?> GetPasswordHash(int id);
    Task<bool> IsActive(int id);
    Task<int> Create(RegisterRequest request, string hashedPassword);
    Task<IEnumerable<UserDb>> GetAll();
    Task<int> UpdateInfos(UpdateUserRequest request);
    Task<int> UpdatePassword(int id, string hashedPassword);
    Task<int> Deactivate(int id);
    Task<int> Reactivate(int id);

    // ── Administration ────────────────────────────────────────────
    Task<IEnumerable<UserAdminDb>> GetAllForAdmin();
    Task UpsertEmploye(int idUtilisateur, string acces);
    Task DeleteEmploye(int idUtilisateur);
    Task<int> CountActiveAdmins();
    Task<bool> IsActiveAdmin(int idUtilisateur);
    Task<bool> AdjustPoints(int idUtilisateur, decimal montant, string type, string motif);
}
