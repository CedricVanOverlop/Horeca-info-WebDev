using Core.Models;
using Infrastructure.Models;

namespace Infrastructure.Repositories.Abstractions;

public interface IEmployeRepository
{
    Task<IEnumerable<EmployeDb>> GetAll();
    Task<EmployeDb?> GetById(string id);
    Task<string> Create(Employe employe);
    Task Update(Employe employe);
    Task Delete(string id);
}
