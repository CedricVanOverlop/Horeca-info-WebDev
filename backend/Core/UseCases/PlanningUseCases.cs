using Core.IGateways;
using Core.Models;
using Core.UseCases.Abstractions;

namespace Core.UseCases;

public class PlanningUseCases(IPlanningGateway planningGateway) : IPlanningUseCases
{
    public Task<IEnumerable<Planning>> GetAll() => planningGateway.GetAll();
    public Task<IEnumerable<Planning>> GetByEmploye(string employeId) => planningGateway.GetByEmployeId(employeId);
    public Task<Planning> Create(Planning planning) => planningGateway.Create(planning);
    public Task Delete(string id) => planningGateway.Delete(id);

    /// <summary>
    /// Retourne les horaires de travail d'un utilisateur employé, vue administrateur.
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur.</param>
    /// <returns>Les horaires de l'utilisateur.</returns>
    public Task<IEnumerable<HoraireAdmin>> GetHorairesAdmin(int userId) =>
        planningGateway.GetHorairesByUtilisateur(userId);
}
