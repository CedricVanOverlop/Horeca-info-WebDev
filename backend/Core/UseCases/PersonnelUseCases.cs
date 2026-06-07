using Core.IGateways;
using Core.Models;
using Core.UseCases.Abstractions;

namespace Core.UseCases;

public class PersonnelUseCases(IEmployeGateway employeGateway) : IPersonnelUseCases
{
    public Task<IEnumerable<Employe>> GetAll() => employeGateway.GetAll();
    public Task<Employe?> GetById(string id) => employeGateway.GetById(id);
    public Task<Employe> Create(Employe employe) => employeGateway.Create(employe);
    public Task Update(Employe employe) => employeGateway.Update(employe);
    public Task Delete(string id) => employeGateway.Delete(id);
}
