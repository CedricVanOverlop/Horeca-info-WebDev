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
}
