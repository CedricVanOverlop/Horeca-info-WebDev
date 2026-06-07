using Core.IGateways;
using Core.Models;
using Infrastructure.Models;
using Infrastructure.Repositories.Abstractions;

namespace Infrastructure.Gateways;

public class ReservationGateway(IReservationRepository reservationRepository) : IReservationGateway
{
    public async Task<IEnumerable<Reservation>> GetAll()
    {
        var dbs = await reservationRepository.GetAll();
        return dbs.Select(Map);
    }

    public async Task<IEnumerable<Reservation>> GetByUserId(int userId)
    {
        var dbs = await reservationRepository.GetByUserId(userId);
        return dbs.Select(Map);
    }

    public async Task<Reservation> Create(Reservation reservation)
    {
        var db = new ReservationDb
        {
            UserId = reservation.UserId,
            TerrainId = reservation.TerrainId,
            DateDebut = reservation.DateDebut,
            DateFin = reservation.DateFin,
            Prix = reservation.Prix
        };
        reservation.Id = await reservationRepository.Create(db);
        return reservation;
    }

    public Task Delete(string id) => reservationRepository.Delete(id);

    private static Reservation Map(ReservationDb db) => new()
    {
        Id = db.Id, UserId = db.UserId, TerrainId = db.TerrainId,
        DateDebut = db.DateDebut, DateFin = db.DateFin, Prix = db.Prix
    };
}
