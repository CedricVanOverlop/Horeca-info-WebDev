using Core.Models;

namespace Core.UseCases.Abstractions;

public interface IPadelUseCases
{
    Task<IEnumerable<Terrain>> GetTerrains();
    Task<IEnumerable<Reservation>> GetReservations(int userId);
    Task<Reservation> CreateReservation(Reservation reservation);
    Task DeleteReservation(string id);
}
