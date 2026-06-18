using Core.IGateways;
using Core.Models;
using Infrastructure.Models;
using Infrastructure.Repositories.Abstractions;

namespace Infrastructure.Gateways;

public class ReservationGateway(IReservationRepository reservationRepository) : IReservationGateway
{
    public async Task<IEnumerable<Reservation>> GetByUserId(int userId)
    {
        var dbs = await reservationRepository.GetByUserId(userId);
        return dbs.Select(Map);
    }

    public async Task<Reservation?> GetById(string id)
    {
        var db = await reservationRepository.GetById(id);
        return db is null ? null : Map(db);
    }

    public async Task<Reservation> Create(Reservation reservation)
    {
        var db = new ReservationDb
        {
            UserId         = reservation.UserId,
            TerrainId      = reservation.TerrainId,
            TarifId        = reservation.TarifId,
            Date           = reservation.Date,
            HeureDebut     = reservation.HeureDebut,
            HeureFin       = reservation.HeureFin,
            PrixPaye       = reservation.PrixPaye,
            MoyenPaiement  = reservation.MoyenPaiement,
            Remarques      = reservation.Remarques
        };
        reservation.Id = await reservationRepository.Create(db);
        return reservation;
    }

    public Task Delete(string id) => reservationRepository.Delete(id);

    public Task<bool> HasTerrainOverlap(string terrainId, DateTime date, TimeSpan heureDebut, TimeSpan heureFin) =>
        reservationRepository.HasTerrainOverlap(terrainId, date, heureDebut, heureFin);

    public Task<bool> HasUserOverlap(int userId, DateTime date, TimeSpan heureDebut, TimeSpan heureFin) =>
        reservationRepository.HasUserOverlap(userId, date, heureDebut, heureFin);

    public Task<bool> HasFutureReservations(string terrainId) =>
        reservationRepository.HasFutureReservations(terrainId);

    public async Task<IEnumerable<CreneauOccupe>> GetCreneauxOccupes(string terrainId, DateTime from, DateTime to)
    {
        var dbs = await reservationRepository.GetByTerrainAndDateRange(terrainId, from, to);
        return dbs.Select(db => new CreneauOccupe
        {
            Date       = db.Date,
            HeureDebut = db.HeureDebut,
            HeureFin   = db.HeureFin
        });
    }

    /// <summary>
    /// Retourne les réservations d'un utilisateur, vue administrateur (avec le nom du terrain).
    /// </summary>
    /// <param name="idUtilisateur">Identifiant de l'utilisateur.</param>
    /// <returns>Les réservations de l'utilisateur.</returns>
    public async Task<IEnumerable<ReservationAdmin>> GetAdminByUtilisateur(int idUtilisateur)
    {
        var dbs = await reservationRepository.GetAdminByUtilisateur(idUtilisateur);
        return dbs.Select(MapAdmin);
    }

    /// <summary>
    /// Retourne toutes les réservations enrichies (terrain + client), vue staff.
    /// </summary>
    /// <returns>Toutes les réservations.</returns>
    public async Task<IEnumerable<ReservationAdmin>> GetAllAdmin()
    {
        var dbs = await reservationRepository.GetAllAdmin();
        return dbs.Select(MapAdmin);
    }

    public Task<bool> HasReservationsForTarif(string tarifId) =>
        reservationRepository.HasReservationsForTarif(tarifId);

    private static ReservationAdmin MapAdmin(ReservationAdminDb db) => new()
    {
        Id            = db.Id,
        Date          = db.Date,
        HeureDebut    = db.HeureDebut,
        HeureFin      = db.HeureFin,
        PrixPaye      = db.PrixPaye,
        Terrain       = db.Terrain,
        Client        = db.Client,
        ClientEmail   = db.ClientEmail,
        MoyenPaiement = db.MoyenPaiement,
        Remarques     = db.Remarques
    };

    private static Reservation Map(ReservationDb db) => new()
    {
        Id              = db.Id,
        UserId          = db.UserId,
        TerrainId       = db.TerrainId,
        TarifId         = db.TarifId,
        Date            = db.Date,
        HeureDebut      = db.HeureDebut,
        HeureFin        = db.HeureFin,
        PrixPaye        = db.PrixPaye,
        MoyenPaiement   = db.MoyenPaiement,
        Remarques       = db.Remarques,
        DateReservation = db.DateReservation
    };
}
