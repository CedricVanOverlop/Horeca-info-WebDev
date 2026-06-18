using Core.Models;

namespace Core.IGateways;

public interface IReservationGateway
{
    /// <summary>Retourne les réservations d'un utilisateur donné.</summary>
    /// <param name="userId">Identifiant de l'utilisateur.</param>
    Task<IEnumerable<Reservation>> GetByUserId(int userId);

    /// <summary>Retourne une réservation par son identifiant, ou null si absente.</summary>
    /// <param name="id">Identifiant de la réservation.</param>
    Task<Reservation?> GetById(string id);

    /// <summary>Insère une réservation (prix déjà calculé et figé par le UseCase).</summary>
    /// <param name="reservation">La réservation à créer.</param>
    Task<Reservation> Create(Reservation reservation);

    /// <summary>Supprime physiquement une réservation (DELETE, pas de soft-delete).</summary>
    /// <param name="id">Identifiant de la réservation.</param>
    Task Delete(string id);

    /// <summary>
    /// Indique s'il existe déjà une réservation sur LE MÊME terrain qui chevauche
    /// le créneau donné (RG-11).
    /// </summary>
    /// <param name="terrainId">Identifiant du terrain.</param>
    /// <param name="date">Date du créneau.</param>
    /// <param name="heureDebut">Heure de début du créneau.</param>
    /// <param name="heureFin">Heure de fin du créneau.</param>
    Task<bool> HasTerrainOverlap(string terrainId, DateTime date, TimeSpan heureDebut, TimeSpan heureFin);

    /// <summary>
    /// Indique si l'utilisateur a déjà une réservation qui chevauche le créneau donné,
    /// TOUS terrains confondus (règle : un client ne peut pas être à deux endroits à la fois).
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur.</param>
    /// <param name="date">Date du créneau.</param>
    /// <param name="heureDebut">Heure de début du créneau.</param>
    /// <param name="heureFin">Heure de fin du créneau.</param>
    Task<bool> HasUserOverlap(int userId, DateTime date, TimeSpan heureDebut, TimeSpan heureFin);

    /// <summary>
    /// Indique s'il reste des réservations à venir sur un terrain (bloque la désactivation
    /// du terrain tant qu'elles n'ont pas été annulées manuellement).
    /// </summary>
    /// <param name="terrainId">Identifiant du terrain.</param>
    Task<bool> HasFutureReservations(string terrainId);

    /// <summary>
    /// Retourne les créneaux occupés d'un terrain entre deux dates (sans donnée nominative),
    /// pour afficher la disponibilité dans la grille de réservation.
    /// </summary>
    /// <param name="terrainId">Identifiant du terrain.</param>
    /// <param name="from">Date de début (incluse).</param>
    /// <param name="to">Date de fin (incluse).</param>
    Task<IEnumerable<CreneauOccupe>> GetCreneauxOccupes(string terrainId, DateTime from, DateTime to);

    /// <summary>Retourne les réservations d'un utilisateur, vue administrateur (avec nom du terrain).</summary>
    /// <param name="idUtilisateur">Identifiant de l'utilisateur.</param>
    Task<IEnumerable<ReservationAdmin>> GetAdminByUtilisateur(int idUtilisateur);

    /// <summary>Retourne toutes les réservations enrichies (terrain + client), vue staff.</summary>
    Task<IEnumerable<ReservationAdmin>> GetAllAdmin();

    /// <summary>Indique si au moins une réservation utilise ce tarif.</summary>
    /// <param name="tarifId">Identifiant du tarif.</param>
    Task<bool> HasReservationsForTarif(string tarifId);
}
