using Core.IGateways;
using Core.Models;
using Infrastructure.Models;
using Infrastructure.Repositories.Abstractions;

namespace Infrastructure.Gateways;

public class PlanningGateway(IPlanningRepository planningRepository) : IPlanningGateway
{
    public async Task<IEnumerable<Planning>> GetAll()
    {
        var dbs = await planningRepository.GetAll();
        return dbs.Select(Map);
    }

    public async Task<IEnumerable<Planning>> GetByEmployeId(string employeId)
    {
        var dbs = await planningRepository.GetByEmployeId(employeId);
        return dbs.Select(Map);
    }

    public async Task<Planning> Create(Planning planning)
    {
        var db = new PlanningDb { EmployeId = planning.EmployeId, DateDebut = planning.DateDebut, DateFin = planning.DateFin };
        planning.Id = await planningRepository.Create(db);
        return planning;
    }

    public Task Delete(string id) => planningRepository.Delete(id);

    private static Planning Map(PlanningDb db) => new()
    { Id = db.Id, EmployeId = db.EmployeId, DateDebut = db.DateDebut, DateFin = db.DateFin };
}
