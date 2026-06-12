using Core.Models;

namespace Core.IGateways;

public interface IEmployeGateway
{
    Task<IEnumerable<Employe>> GetAll();
    Task<Employe?> GetById(int id);
    Task<Employe> Create(Employe employe);
    Task Update(Employe employe);
    Task Delete(int id);
}
