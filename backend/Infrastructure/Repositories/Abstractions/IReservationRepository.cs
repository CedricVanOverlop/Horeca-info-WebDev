using Infrastructure.Models;

namespace Infrastructure.Repositories.Abstractions;

public interface IReservationRepository
{
    Task<IEnumerable<ReservationDb>> GetByUserId(int userId);
    Task<ReservationDb?> GetById(string id);
    Task<string> Create(ReservationDb reservation);
    Task Delete(string id);
    Task<int> DeleteFutureByUserId(int userId);
    Task<bool> HasTerrainOverlap(string terrainId, DateTime date, TimeSpan heureDebut, TimeSpan heureFin);
    Task<bool> HasUserOverlap(int userId, DateTime date, TimeSpan heureDebut, TimeSpan heureFin);
    Task<bool> HasFutureReservations(string terrainId);
    Task<IEnumerable<ReservationDb>> GetByTerrainAndDateRange(string terrainId, DateTime from, DateTime to);
    Task<IEnumerable<ReservationAdminDb>> GetAdminByUtilisateur(int idUtilisateur);
    Task<IEnumerable<ReservationAdminDb>> GetAllAdmin();
    Task<bool> HasReservationsForTarif(string tarifId);
}
