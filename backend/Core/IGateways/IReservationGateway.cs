using Core.Models;

namespace Core.IGateways;

public interface IReservationGateway
{
    Task<IEnumerable<Reservation>> GetAll();
    Task<IEnumerable<Reservation>> GetByUserId(int userId);
    Task<Reservation> Create(Reservation reservation);
    Task Delete(string id);
}
