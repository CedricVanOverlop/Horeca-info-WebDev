using Core.IGateways;
using Core.Models;
using Core.UseCases.Abstractions;

namespace Core.UseCases;

public class PlanningUseCases(IPlanningGateway planningGateway) : IPlanningUseCases
{
    /// <summary>
    /// Retourne les horaires de travail d'un utilisateur employé, vue administrateur.
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur.</param>
    /// <returns>Les horaires de l'utilisateur.</returns>
    public Task<IEnumerable<HoraireAdmin>> GetHorairesAdmin(int userId) =>
        planningGateway.GetHorairesByUtilisateur(userId);
}
