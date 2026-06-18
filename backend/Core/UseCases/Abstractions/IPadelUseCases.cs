using Core.Models;

namespace Core.UseCases.Abstractions;

public interface IPadelUseCases
{
    // ── Consultation ──────────────────────────────────────────────
    Task<IEnumerable<Terrain>> GetTerrains();
    Task<IEnumerable<Reservation>> GetReservations(int userId);
    Task<IEnumerable<ReservationAdmin>> GetReservationsAdmin(int userId);

    /// <summary>
    /// Retourne toutes les réservations enrichies (terrain + client) pour la vue staff
    /// (administrateur/cuisine), qui peut consulter et annuler n'importe quelle réservation.
    /// </summary>
    Task<IEnumerable<ReservationAdmin>> GetAllReservationsAdmin();

    /// <summary>
    /// Retourne les créneaux occupés d'un terrain entre deux dates (sans donnée nominative),
    /// pour la grille de disponibilité.
    /// </summary>
    /// <param name="terrainId">Identifiant du terrain.</param>
    /// <param name="from">Date de début (incluse).</param>
    /// <param name="to">Date de fin (incluse).</param>
    Task<IEnumerable<CreneauOccupe>> GetCreneauxOccupes(string terrainId, DateTime from, DateTime to);

    // ── Réservation ───────────────────────────────────────────────
    /// <summary>
    /// Crée une réservation pour un client (identité issue du JWT). Applique RG-04
    /// (terrain actif), RG-06 (heure_fin > heure_debut), RG-11 (pas de chevauchement
    /// sur le même terrain) ET la règle client : pas de chevauchement tous terrains
    /// confondus. Le prix est calculé et figé serveur.
    /// </summary>
    /// <param name="userId">Identifiant du client connecté.</param>
    /// <param name="request">Données du créneau souhaité.</param>
    Task<Reservation> CreateReservationClient(int userId, CreerReservationRequest request);

    /// <summary>
    /// Crée une réservation manuelle par le personnel (admin/cuisine), toujours liée à
    /// un utilisateur existant. N'applique PAS la règle client tous terrains (le staff
    /// peut réserver plusieurs terrains au même créneau pour des groupes différents),
    /// mais conserve RG-04, RG-06 et RG-11.
    /// </summary>
    /// <param name="idUtilisateur">Identifiant de l'utilisateur bénéficiaire (recherché par le staff).</param>
    /// <param name="request">Données du créneau (Remarques utilisable pour un blocage).</param>
    Task<Reservation> CreateReservationManuelle(int idUtilisateur, CreerReservationRequest request);

    /// <summary>
    /// Supprime une réservation (DELETE physique). Pour un client : uniquement la sienne
    /// et à plus de 12h du créneau. Pour le staff (admin/cuisine) : sans restriction.
    /// </summary>
    /// <param name="reservationId">Identifiant de la réservation.</param>
    /// <param name="userId">Identifiant de l'utilisateur qui demande l'annulation.</param>
    /// <param name="estStaff">Vrai si admin/cuisine (annulation libre), faux si client.</param>
    Task CancelReservation(string reservationId, int userId, bool estStaff);

    /// <summary>
    /// Recherche des utilisateurs pour la réservation manuelle (nom, prénom ou email).
    /// </summary>
    /// <param name="query">Terme de recherche.</param>
    Task<IEnumerable<User>> SearchUtilisateurs(string query);

    // ── Gestion des terrains (admin/cuisine) ──────────────────────
    /// <summary>Crée un terrain (admin uniquement, contrôlé à la route).</summary>
    /// <param name="terrain">Le terrain à créer.</param>
    Task<Terrain> CreateTerrain(Terrain terrain);

    /// <summary>
    /// Met à jour le nom et la plage horaire d'un terrain (admin uniquement). Le statut
    /// actif et le commerce ne sont pas modifiés ici.
    /// </summary>
    /// <param name="id">Identifiant du terrain.</param>
    /// <param name="nom">Nouveau nom.</param>
    /// <param name="heureOuverture">Nouvelle heure d'ouverture.</param>
    /// <param name="heureFermeture">Nouvelle heure de fermeture.</param>
    Task<Terrain> UpdateTerrain(string id, string nom, TimeSpan heureOuverture, TimeSpan heureFermeture);

    /// <summary>
    /// Active ou désactive un terrain. La désactivation est refusée (409) s'il reste
    /// des réservations à venir : elles doivent être annulées manuellement d'abord.
    /// </summary>
    /// <param name="terrainId">Identifiant du terrain.</param>
    /// <param name="actif">Nouvel état souhaité.</param>
    Task ToggleTerrainActif(string terrainId, bool actif);

    // ── Gestion des tarifs (admin uniquement) ─────────────────────
    /// <summary>Retourne la grille tarifaire d'un terrain.</summary>
    /// <param name="terrainId">Identifiant du terrain.</param>
    Task<IEnumerable<Tarif>> GetTarifs(string terrainId);

    /// <summary>
    /// Crée un tarif pour un terrain. Rejette (409) toute plage qui chevauche un tarif
    /// existant du même terrain sur le même jour.
    /// </summary>
    /// <param name="tarif">Le tarif à créer.</param>
    Task<Tarif> CreateTarif(Tarif tarif);

    /// <summary>
    /// Met à jour un tarif. Même contrôle de chevauchement que la création (en excluant
    /// le tarif lui-même).
    /// </summary>
    /// <param name="tarif">Le tarif modifié.</param>
    Task UpdateTarif(Tarif tarif);

    /// <summary>Supprime un tarif.</summary>
    /// <param name="tarifId">Identifiant du tarif.</param>
    Task DeleteTarif(string tarifId);
}
