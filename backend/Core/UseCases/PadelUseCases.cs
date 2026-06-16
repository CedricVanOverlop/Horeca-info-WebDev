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

    /// <summary>
    /// Retourne les réservations d'un utilisateur, vue administrateur.
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur.</param>
    /// <returns>Les réservations de l'utilisateur.</returns>
    public Task<IEnumerable<ReservationAdmin>> GetReservationsAdmin(int userId) =>
        reservationGateway.GetAdminByUtilisateur(userId);
}
