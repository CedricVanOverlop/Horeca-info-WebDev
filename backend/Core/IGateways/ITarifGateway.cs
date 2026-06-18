using Core.Models;

namespace Core.IGateways;

public interface ITarifGateway
{
    /// <summary>
    /// Retourne tous les tarifs d'un terrain (sert au calcul du prix d'une réservation,
    /// au contrôle de chevauchement et à l'affichage de la grille).
    /// </summary>
    /// <param name="terrainId">Identifiant du terrain.</param>
    Task<IEnumerable<Tarif>> GetByTerrain(string terrainId);

    /// <summary>Retourne un tarif par son identifiant, ou null si absent.</summary>
    /// <param name="id">Identifiant du tarif.</param>
    Task<Tarif?> GetById(string id);

    /// <summary>Insère un tarif (plage horaire + jour + prix) pour un terrain.</summary>
    /// <param name="tarif">Le tarif à créer.</param>
    Task<Tarif> Create(Tarif tarif);

    /// <summary>Met à jour un tarif existant.</summary>
    /// <param name="tarif">Le tarif modifié.</param>
    Task Update(Tarif tarif);

    /// <summary>Supprime un tarif.</summary>
    /// <param name="id">Identifiant du tarif.</param>
    Task Delete(string id);
}
