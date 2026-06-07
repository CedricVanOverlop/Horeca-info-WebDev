using Core.IGateways;
using Core.Models;
using Core.UseCases.Abstractions;

namespace Core.UseCases;

public class PadelUseCases(ITerrainGateway terrainGateway, IReservationGateway reservationGateway) : IPadelUseCases
{
    public Task<IEnumerable<Terrain>> GetTerrains() => terrainGateway.GetAll();
    public Task<IEnumerable<Reservation>> GetReservations(int userId) => reservationGateway.GetByUserId(userId);
    public Task<Reservation> CreateReservation(Reservation reservation) => reservationGateway.Create(reservation);
    public Task DeleteReservation(string id) => reservationGateway.Delete(id);
}
