using Infrastructure.Models;

namespace Infrastructure.Repositories.Abstractions;

public interface IPlanningRepository
{
    Task<IEnumerable<PlanningDb>> GetAll();
    Task<IEnumerable<PlanningDb>> GetByEmployeId(string employeId);
    Task<string> Create(PlanningDb planning);
    Task Delete(string id);
}
