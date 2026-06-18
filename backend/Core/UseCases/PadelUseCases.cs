using Core.Exceptions;
using Core.IGateways;
using Core.Models;
using Core.UseCases.Abstractions;

namespace Core.UseCases;

public class PadelUseCases(
    ITerrainGateway terrainGateway,
    IReservationGateway reservationGateway,
    ITarifGateway tarifGateway,
    IUserGateway userGateway) : IPadelUseCases
{
    // Délai minimum (en heures) avant le créneau pour qu'un client puisse annuler lui-même.
    private const int DelaiAnnulationClientHeures = 12;

    // Horizon maximum (en jours) de réservation pour un client. Le staff n'est pas limité,
    // ce qui lui laisse de la marge pour gérer les modifications/blocages à l'avance.
    private const int MaxJoursReservationClient = 15;

    // ── Consultation ──────────────────────────────────────────────

    public Task<IEnumerable<Terrain>> GetTerrains() => terrainGateway.GetAll();

    public Task<IEnumerable<Reservation>> GetReservations(int userId) =>
        reservationGateway.GetByUserId(userId);

    /// <summary>
    /// Retourne les réservations d'un utilisateur, vue administrateur.
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur.</param>
    /// <returns>Les réservations de l'utilisateur.</returns>
    public Task<IEnumerable<ReservationAdmin>> GetReservationsAdmin(int userId) =>
        reservationGateway.GetAdminByUtilisateur(userId);

    /// <summary>
    /// Retourne toutes les réservations enrichies (terrain + client) pour la vue staff.
    /// </summary>
    /// <returns>Toutes les réservations.</returns>
    public Task<IEnumerable<ReservationAdmin>> GetAllReservationsAdmin() =>
        reservationGateway.GetAllAdmin();

    /// <summary>
    /// Retourne les créneaux occupés d'un terrain entre deux dates (grille de disponibilité).
    /// </summary>
    public Task<IEnumerable<CreneauOccupe>> GetCreneauxOccupes(string terrainId, DateTime from, DateTime to)
    {
        if (string.IsNullOrWhiteSpace(terrainId))
            throw new ValidationException("Le terrain est obligatoire.");
        if (to < from)
            throw new ValidationException("La date de fin doit être postérieure ou égale à la date de début.");

        return reservationGateway.GetCreneauxOccupes(terrainId, from, to);
    }

    // ── Réservation ───────────────────────────────────────────────

    /// <summary>
    /// Crée une réservation pour un client. Voir IPadelUseCases pour les règles appliquées.
    /// </summary>
    public async Task<Reservation> CreateReservationClient(int userId, CreerReservationRequest request)
    {
        // Horizon client : pas de réservation au-delà de 15 jours (le staff n'est pas concerné).
        if (request.Date.Date > DateTime.Today.AddDays(MaxJoursReservationClient))
            throw new ValidationException(
                $"Vous pouvez réserver jusqu'à {MaxJoursReservationClient} jours à l'avance maximum.");

        var (terrain, tarif, prix) = await ValiderEtCalculer(request);

        // RG-11 : pas de chevauchement sur le même terrain.
        if (await reservationGateway.HasTerrainOverlap(terrain.Id, request.Date.Date, request.HeureDebut, request.HeureFin))
            throw new ConflictException("Ce créneau est déjà réservé sur ce terrain.");

        // Règle client : pas de chevauchement tous terrains confondus.
        if (await reservationGateway.HasUserOverlap(userId, request.Date.Date, request.HeureDebut, request.HeureFin))
            throw new ConflictException("Vous avez déjà une réservation sur ce créneau.");

        return await reservationGateway.Create(ConstruireReservation(userId, request, terrain, tarif, prix));
    }

    /// <summary>
    /// Crée une réservation manuelle (admin/cuisine) liée à un utilisateur existant.
    /// </summary>
    public async Task<Reservation> CreateReservationManuelle(int idUtilisateur, CreerReservationRequest request)
    {
        // La réservation manuelle est toujours rattachée à un compte existant.
        var beneficiaire = await userGateway.GetProfile(idUtilisateur)
            ?? throw new NotFoundException("Utilisateur introuvable.");

        var (terrain, tarif, prix) = await ValiderEtCalculer(request);

        // RG-11 : pas de chevauchement sur le même terrain (s'applique aussi au staff).
        if (await reservationGateway.HasTerrainOverlap(terrain.Id, request.Date.Date, request.HeureDebut, request.HeureFin))
            throw new ConflictException("Ce créneau est déjà réservé sur ce terrain.");

        // Pas de contrôle "tous terrains" : le staff peut réserver plusieurs terrains au même créneau.
        return await reservationGateway.Create(
            ConstruireReservation(beneficiaire.Id, request, terrain, tarif, prix));
    }

    /// <summary>
    /// Supprime une réservation. Client : la sienne et à plus de 12h ; staff : sans restriction.
    /// </summary>
    public async Task CancelReservation(string reservationId, int userId, bool estStaff)
    {
        var reservation = await reservationGateway.GetById(reservationId)
            ?? throw new NotFoundException("Réservation introuvable.");

        if (!estStaff)
        {
            if (reservation.UserId != userId)
                throw new ForbiddenException("Vous ne pouvez annuler que vos propres réservations.");

            var debutCreneau = reservation.Date.Date + reservation.HeureDebut;
            if (debutCreneau - DateTime.Now <= TimeSpan.FromHours(DelaiAnnulationClientHeures))
                throw new ValidationException(
                    $"Annulation impossible à moins de {DelaiAnnulationClientHeures}h du créneau.");
        }

        await reservationGateway.Delete(reservationId);
    }

    /// <summary>
    /// Recherche des utilisateurs pour la réservation manuelle (min. 2 caractères).
    /// </summary>
    public Task<IEnumerable<User>> SearchUtilisateurs(string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
            throw new ValidationException("La recherche doit contenir au moins 2 caractères.");

        return userGateway.Search(query.Trim());
    }

    // ── Gestion des terrains ──────────────────────────────────────

    /// <summary>
    /// Crée un terrain après validation des champs (nom, bornes d'ouverture).
    /// </summary>
    public Task<Terrain> CreateTerrain(Terrain terrain)
    {
        if (string.IsNullOrWhiteSpace(terrain.Nom))
            throw new ValidationException("Le nom du terrain est obligatoire.");
        if (terrain.HeureFermeture <= terrain.HeureOuverture)
            throw new ValidationException("L'heure de fermeture doit être postérieure à l'ouverture.");

        return terrainGateway.Create(terrain);
    }

    /// <summary>
    /// Met à jour le nom et la plage horaire d'un terrain (statut actif/commerce inchangés).
    /// </summary>
    public async Task<Terrain> UpdateTerrain(string id, string nom, TimeSpan heureOuverture, TimeSpan heureFermeture)
    {
        if (string.IsNullOrWhiteSpace(nom))
            throw new ValidationException("Le nom du terrain est obligatoire.");
        if (heureFermeture <= heureOuverture)
            throw new ValidationException("L'heure de fermeture doit être postérieure à l'ouverture.");

        var terrain = await terrainGateway.GetById(id)
            ?? throw new NotFoundException("Terrain introuvable.");

        terrain.Nom = nom;
        terrain.HeureOuverture = heureOuverture;
        terrain.HeureFermeture = heureFermeture;
        await terrainGateway.Update(terrain);
        return terrain;
    }

    /// <summary>
    /// Active/désactive un terrain. Désactivation refusée s'il reste des réservations à venir.
    /// </summary>
    public async Task ToggleTerrainActif(string terrainId, bool actif)
    {
        var terrain = await terrainGateway.GetById(terrainId)
            ?? throw new NotFoundException("Terrain introuvable.");

        if (!actif && await reservationGateway.HasFutureReservations(terrainId))
            throw new ConflictException(
                "Impossible de désactiver : des réservations à venir existent. Annulez-les d'abord.");

        terrain.Disponible = actif;
        await terrainGateway.Update(terrain);
    }

    // ── Gestion des tarifs ────────────────────────────────────────

    public Task<IEnumerable<Tarif>> GetTarifs(string terrainId) =>
        tarifGateway.GetByTerrain(terrainId);

    /// <summary>
    /// Crée un tarif après validation et contrôle de chevauchement.
    /// </summary>
    public async Task<Tarif> CreateTarif(Tarif tarif)
    {
        ValiderTarif(tarif);
        await VerifierChevauchementTarif(tarif, tarifIdExclu: null);
        return await tarifGateway.Create(tarif);
    }

    /// <summary>
    /// Met à jour un tarif après validation et contrôle de chevauchement (hors lui-même).
    /// </summary>
    public async Task UpdateTarif(Tarif tarif)
    {
        ValiderTarif(tarif);
        await VerifierChevauchementTarif(tarif, tarifIdExclu: tarif.Id);
        await tarifGateway.Update(tarif);
    }

    /// <summary>
    /// Supprime un tarif. Refusé (409) si une réservation référence encore ce tarif :
    /// la suppression violerait la contrainte de clé étrangère côté base.
    /// </summary>
    /// <param name="tarifId">Identifiant du tarif.</param>
    public async Task DeleteTarif(string tarifId)
    {
        if (await reservationGateway.HasReservationsForTarif(tarifId))
            throw new ConflictException(
                "Impossible de supprimer ce tarif : une réservation l'utilise encore. Annulez d'abord les réservations concernées.");

        await tarifGateway.Delete(tarifId);
    }

    // ── Helpers privés ────────────────────────────────────────────

    /// <summary>
    /// Valide le créneau demandé et calcule le tarif/prix applicable (RG-04, RG-06, RG-08, RG-09).
    /// </summary>
    private async Task<(Terrain terrain, Tarif tarif, decimal prix)> ValiderEtCalculer(CreerReservationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TerrainId))
            throw new ValidationException("Le terrain est obligatoire.");
        if (request.MoyenPaiement is not ("EnLigne" or "SurPlace"))
            throw new ValidationException("Moyen de paiement invalide.");
        if (request.HeureFin <= request.HeureDebut)
            throw new ValidationException("L'heure de fin doit être postérieure à l'heure de début.");

        // Créneaux alignés sur l'heure pile ou la demi-heure (pas de 17h04, 18h32…).
        if (!EstSurDemiHeure(request.HeureDebut) || !EstSurDemiHeure(request.HeureFin))
            throw new ValidationException("Les réservations doivent commencer et finir à l'heure pile ou à la demi-heure.");

        var terrain = await terrainGateway.GetById(request.TerrainId)
            ?? throw new NotFoundException("Terrain introuvable.");

        // RG-04 : réservation uniquement sur un terrain actif.
        if (!terrain.Disponible)
            throw new ValidationException("Ce terrain n'est pas disponible à la réservation.");

        if (request.HeureDebut < terrain.HeureOuverture || request.HeureFin > terrain.HeureFermeture)
            throw new ValidationException("Le créneau est hors des heures d'ouverture du terrain.");

        if (request.Date.Date + request.HeureDebut < DateTime.Now)
            throw new ValidationException("Impossible de réserver un créneau déjà passé.");

        // RG-08 : le prix est déterminé par le tarif couvrant l'HEURE DE DÉBUT du créneau.
        // Le créneau peut déborder sur la plage suivante : on conserve le tarif du début.
        var jourSemaine = JourIso(request.Date);
        var tarifs = await tarifGateway.GetByTerrain(terrain.Id);
        var tarif = tarifs.FirstOrDefault(t =>
                        t.JourSemaine == jourSemaine
                        && t.HeureDebut <= request.HeureDebut
                        && request.HeureDebut < t.HeureFin)
                    ?? throw new ValidationException("Aucun tarif ne couvre l'heure de début de ce créneau.");

        // RG-09 : prix_paye = prix_heure × durée (en heures).
        var dureeHeures = (decimal)(request.HeureFin - request.HeureDebut).TotalHours;
        var prix = tarif.PrixHeure * dureeHeures;

        return (terrain, tarif, prix);
    }

    /// <summary>
    /// Assemble l'entité Reservation à insérer (prix et tarif déjà figés).
    /// </summary>
    private static Reservation ConstruireReservation(
        int userId, CreerReservationRequest request, Terrain terrain, Tarif tarif, decimal prix) =>
        new()
        {
            UserId = userId,
            TerrainId = terrain.Id,
            TarifId = tarif.Id,
            Date = request.Date.Date,
            HeureDebut = request.HeureDebut,
            HeureFin = request.HeureFin,
            PrixPaye = prix,
            MoyenPaiement = request.MoyenPaiement,
            Remarques = request.Remarques,
        };

    /// <summary>
    /// Valide les champs d'un tarif (prix > 0, heures cohérentes, jour ISO valide).
    /// </summary>
    private static void ValiderTarif(Tarif tarif)
    {
        if (string.IsNullOrWhiteSpace(tarif.TerrainId))
            throw new ValidationException("Le terrain du tarif est obligatoire.");
        if (tarif.PrixHeure <= 0)
            throw new ValidationException("Le prix horaire doit être strictement positif.");
        if (tarif.HeureFin <= tarif.HeureDebut)
            throw new ValidationException("L'heure de fin du tarif doit être postérieure à l'heure de début.");
        if (tarif.JourSemaine is < 1 or > 7)
            throw new ValidationException("Le jour de la semaine doit être compris entre 1 et 7.");
    }

    /// <summary>
    /// Rejette (409) un tarif dont la plage chevauche un tarif existant du même terrain
    /// et du même jour. tarifIdExclu permet d'ignorer le tarif lui-même lors d'une mise à jour.
    /// </summary>
    private async Task VerifierChevauchementTarif(Tarif tarif, string? tarifIdExclu)
    {
        var existants = await tarifGateway.GetByTerrain(tarif.TerrainId);
        var chevauche = existants.Any(t =>
            t.Id != tarifIdExclu
            && t.JourSemaine == tarif.JourSemaine
            && t.HeureDebut < tarif.HeureFin
            && tarif.HeureDebut < t.HeureFin);

        if (chevauche)
            throw new ConflictException("Cette plage horaire chevauche un tarif existant pour ce jour.");
    }

    /// <summary>
    /// Convertit une date en jour ISO 8601 (1=lundi … 7=dimanche).
    /// </summary>
    private static int JourIso(DateTime date) =>
        date.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)date.DayOfWeek;

    /// <summary>
    /// Vrai si l'heure tombe sur l'heure pile ou la demi-heure (minutes multiples de 30,
    /// pas de secondes résiduelles).
    /// </summary>
    /// <param name="heure">L'heure à vérifier.</param>
    private static bool EstSurDemiHeure(TimeSpan heure) =>
        heure.Seconds == 0 && heure.Minutes % 30 == 0;
}
