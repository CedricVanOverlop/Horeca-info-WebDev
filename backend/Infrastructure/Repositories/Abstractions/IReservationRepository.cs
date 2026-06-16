using Infrastructure.Models;

namespace Infrastructure.Repositories.Abstractions;

public interface IReservationRepository
{
    Task<IEnumerable<ReservationDb>> GetAll();
    Task<IEnumerable<ReservationDb>> GetByUserId(int userId);
    Task<string> Create(ReservationDb reservation);
    Task Delete(string id);
    Task<int> DeleteFutureByUserId(int userId);
    Task<IEnumerable<ReservationAdminDb>> GetAdminByUtilisateur(int idUtilisateur);
}
