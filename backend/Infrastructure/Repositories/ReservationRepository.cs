using Dapper;
using Infrastructure.Models;
using Infrastructure.Repositories.Abstractions;
using System.Data;

namespace Infrastructure.Repositories;

public class ReservationRepository(IDbConnection connection) : IReservationRepository
{
    // Colonnes RESERVATION aliasées vers les propriétés de ReservationDb.
    // Les identifiants INT sont castés en CHAR pour mapper sur les propriétés string.
    private const string SelectColumns = @"
        CAST(id_reservation AS CHAR) AS Id, id_utilisateur AS UserId,
        CAST(id_terrain AS CHAR) AS TerrainId, CAST(id_tarif AS CHAR) AS TarifId,
        date AS Date, heure_debut AS HeureDebut, heure_fin AS HeureFin,
        prix_paye AS PrixPaye, moyen_paiement AS MoyenPaiement, remarques AS Remarques,
        date_reservation AS DateReservation";

    public async Task<IEnumerable<ReservationDb>> GetByUserId(int userId)
    {
        var sql = $@"SELECT {SelectColumns}
                     FROM RESERVATION
                     WHERE id_utilisateur = @UserId
                     ORDER BY date DESC, heure_debut DESC";
        return await connection.QueryAsync<ReservationDb>(sql, new { UserId = userId });
    }

    public async Task<ReservationDb?> GetById(string id)
    {
        var sql = $@"SELECT {SelectColumns}
                     FROM RESERVATION
                     WHERE id_reservation = @Id
                     LIMIT 1";
        return await connection.QueryFirstOrDefaultAsync<ReservationDb>(sql, new { Id = id });
    }

    public async Task<string> Create(ReservationDb reservation)
    {
        const string sql = @"
            INSERT INTO RESERVATION
                (date, heure_debut, heure_fin, prix_paye, moyen_paiement, remarques,
                 id_terrain, id_utilisateur, id_tarif)
            VALUES
                (@Date, @HeureDebut, @HeureFin, @PrixPaye, @MoyenPaiement, @Remarques,
                 @TerrainId, @UserId, @TarifId);
            SELECT LAST_INSERT_ID();";
        return (await connection.ExecuteScalarAsync<object>(sql, reservation))?.ToString() ?? string.Empty;
    }

    public async Task Delete(string id)
    {
        const string sql = "DELETE FROM RESERVATION WHERE id_reservation = @Id";
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<int> DeleteFutureByUserId(int userId)
    {
        // Réservations à venir uniquement : on conserve l'historique passé.
        const string sql = @"
            DELETE FROM RESERVATION
            WHERE id_utilisateur = @UserId AND TIMESTAMP(date, heure_debut) > NOW()";
        return await connection.ExecuteAsync(sql, new { UserId = userId });
    }

    public async Task<bool> HasTerrainOverlap(string terrainId, DateTime date, TimeSpan heureDebut, TimeSpan heureFin)
    {
        // Chevauchement = même terrain, même date, et plages qui se croisent.
        const string sql = @"
            SELECT COUNT(1) FROM RESERVATION
            WHERE id_terrain = @TerrainId AND date = @Date
              AND heure_debut < @HeureFin AND @HeureDebut < heure_fin";
        return await connection.ExecuteScalarAsync<bool>(sql,
            new { TerrainId = terrainId, Date = date.Date, HeureDebut = heureDebut, HeureFin = heureFin });
    }

    public async Task<bool> HasUserOverlap(int userId, DateTime date, TimeSpan heureDebut, TimeSpan heureFin)
    {
        // Chevauchement tous terrains confondus pour un même utilisateur.
        const string sql = @"
            SELECT COUNT(1) FROM RESERVATION
            WHERE id_utilisateur = @UserId AND date = @Date
              AND heure_debut < @HeureFin AND @HeureDebut < heure_fin";
        return await connection.ExecuteScalarAsync<bool>(sql,
            new { UserId = userId, Date = date.Date, HeureDebut = heureDebut, HeureFin = heureFin });
    }

    public async Task<bool> HasFutureReservations(string terrainId)
    {
        const string sql = @"
            SELECT COUNT(1) FROM RESERVATION
            WHERE id_terrain = @TerrainId AND TIMESTAMP(date, heure_debut) > NOW()";
        return await connection.ExecuteScalarAsync<bool>(sql, new { TerrainId = terrainId });
    }

    public async Task<IEnumerable<ReservationDb>> GetByTerrainAndDateRange(string terrainId, DateTime from, DateTime to)
    {
        var sql = $@"SELECT {SelectColumns}
                     FROM RESERVATION
                     WHERE id_terrain = @TerrainId AND date BETWEEN @From AND @To
                     ORDER BY date, heure_debut";
        return await connection.QueryAsync<ReservationDb>(sql,
            new { TerrainId = terrainId, From = from.Date, To = to.Date });
    }

    /// <summary>
    /// Retourne les réservations d'un utilisateur (avec le nom du terrain), les plus récentes d'abord.
    /// Vue administrateur.
    /// </summary>
    /// <param name="idUtilisateur">Identifiant de l'utilisateur.</param>
    /// <returns>Les réservations de l'utilisateur.</returns>
    public async Task<IEnumerable<ReservationAdminDb>> GetAdminByUtilisateur(int idUtilisateur)
    {
        const string sql = @"
            SELECT r.id_reservation AS Id, r.date AS Date, r.heure_debut AS HeureDebut,
                   r.heure_fin AS HeureFin, r.prix_paye AS PrixPaye, t.nom AS Terrain,
                   CONCAT(u.prenom, ' ', u.nom) AS Client, u.email AS ClientEmail,
                   r.moyen_paiement AS MoyenPaiement, r.remarques AS Remarques
            FROM RESERVATION r
            JOIN TERRAIN t ON t.id_terrain = r.id_terrain
            JOIN UTILISATEUR u ON u.id_utilisateur = r.id_utilisateur
            WHERE r.id_utilisateur = @Id
            ORDER BY r.date DESC, r.heure_debut DESC";
        return await connection.QueryAsync<ReservationAdminDb>(sql, new { Id = idUtilisateur });
    }

    /// <summary>
    /// Indique s'il existe au moins une réservation référençant ce tarif
    /// (bloque la suppression du tarif par contrainte de clé étrangère).
    /// </summary>
    /// <param name="tarifId">Identifiant du tarif.</param>
    /// <returns>True si le tarif est utilisé par une réservation.</returns>
    public async Task<bool> HasReservationsForTarif(string tarifId)
    {
        const string sql = "SELECT COUNT(1) FROM RESERVATION WHERE id_tarif = @TarifId";
        return await connection.ExecuteScalarAsync<bool>(sql, new { TarifId = tarifId });
    }

    /// <summary>
    /// Retourne toutes les réservations (terrain + client), créneaux à venir d'abord
    /// puis l'historique. Vue staff (administrateur/cuisine).
    /// </summary>
    /// <returns>Toutes les réservations enrichies.</returns>
    public async Task<IEnumerable<ReservationAdminDb>> GetAllAdmin()
    {
        const string sql = @"
            SELECT r.id_reservation AS Id, r.date AS Date, r.heure_debut AS HeureDebut,
                   r.heure_fin AS HeureFin, r.prix_paye AS PrixPaye, t.nom AS Terrain,
                   CONCAT(u.prenom, ' ', u.nom) AS Client, u.email AS ClientEmail,
                   r.moyen_paiement AS MoyenPaiement, r.remarques AS Remarques
            FROM RESERVATION r
            JOIN TERRAIN t ON t.id_terrain = r.id_terrain
            JOIN UTILISATEUR u ON u.id_utilisateur = r.id_utilisateur
            ORDER BY r.date DESC, r.heure_debut DESC";
        return await connection.QueryAsync<ReservationAdminDb>(sql);
    }
}
