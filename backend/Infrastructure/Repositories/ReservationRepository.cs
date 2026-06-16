using Dapper;
using Infrastructure.Models;
using Infrastructure.Repositories.Abstractions;
using System.Data;

namespace Infrastructure.Repositories;

public class ReservationRepository(IDbConnection connection) : IReservationRepository
{
    public async Task<IEnumerable<ReservationDb>> GetAll()
    {
        const string sql = "SELECT * FROM Reservations ORDER BY DateDebut";
        return await connection.QueryAsync<ReservationDb>(sql);
    }

    public async Task<IEnumerable<ReservationDb>> GetByUserId(int userId)
    {
        const string sql = "SELECT * FROM Reservations WHERE UserId = @UserId ORDER BY DateDebut";
        return await connection.QueryAsync<ReservationDb>(sql, new { UserId = userId });
    }

    public async Task<string> Create(ReservationDb reservation)
    {
        const string sql = @"
            INSERT INTO Reservations (UserId, TerrainId, DateDebut, DateFin, Prix)
            VALUES (@UserId, @TerrainId, @DateDebut, @DateFin, @Prix);
            SELECT LAST_INSERT_ID();";
        return (await connection.ExecuteScalarAsync<object>(sql, reservation))?.ToString() ?? string.Empty;
    }

    public async Task Delete(string id)
    {
        const string sql = "DELETE FROM Reservations WHERE Id = @Id";
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<int> DeleteFutureByUserId(int userId)
    {
        const string sql = "DELETE FROM Reservations WHERE UserId = @UserId AND DateDebut > NOW()";
        return await connection.ExecuteAsync(sql, new { UserId = userId });
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
                   r.heure_fin AS HeureFin, r.prix_paye AS PrixPaye, t.nom AS Terrain
            FROM RESERVATION r
            JOIN TERRAIN t ON t.id_terrain = r.id_terrain
            WHERE r.id_utilisateur = @Id
            ORDER BY r.date DESC, r.heure_debut DESC";
        return await connection.QueryAsync<ReservationAdminDb>(sql, new { Id = idUtilisateur });
    }
}
