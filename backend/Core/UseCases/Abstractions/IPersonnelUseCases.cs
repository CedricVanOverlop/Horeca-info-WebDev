using Core.Models;

namespace Core.UseCases.Abstractions;

public interface IPersonnelUseCases
{
    Task<IEnumerable<Employe>> GetAll();
    Task<Employe?> GetById(string id);
    Task<Employe> Create(Employe employe);
    Task Update(Employe employe);
    Task Delete(string id);
}
