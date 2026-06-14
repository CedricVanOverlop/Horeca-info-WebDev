using Infrastructure.Models;

namespace Infrastructure.Repositories.Abstractions;

public interface IEmployeRepository
{
    Task<EmployeDb?> FindByIdUtilisateur(int idUtilisateur);
    Task<IEnumerable<EmployeDb>> GetAll();
    Task<EmployeDb?> GetById(int id);
    Task<int> Create(EmployeDb employe);
    Task Update(EmployeDb employe);
    Task Delete(int id);
    Task Deactivate(int id);
}
