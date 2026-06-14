using Core.Models;

namespace Core.UseCases.Abstractions;

public interface IUserUseCases
{
    Task<User?> Authenticate(AuthenticationRequest request);
    Task<User> Register(RegisterRequest request);
    Task<IEnumerable<User>> GetAll();
    Task<User?> GetProfile(int id);
    Task<bool> IsActive(int id);
    Task<bool> UpdateInfos(int id, UpdateUserRequest request);
    Task<bool> UpdatePassword(int id, ChangePasswordRequest request);
    Task<bool> DeleteAccount(int id);

    // ── Administration ────────────────────────────────────────────
    Task<IEnumerable<UserAdmin>> GetAllForAdmin();
    Task ChangeRole(int id, ChangeRoleRequest request);
    Task<bool> AdjustPoints(int id, AjustementPointsRequest request);
    Task<bool> Block(int id);
    Task<bool> Unblock(int id);
    Task<bool> DeleteByAdmin(int id);
    Task<IEnumerable<ReservationAdmin>> GetReservations(int id);
    Task<IEnumerable<HoraireAdmin>> GetHoraires(int id);
}
