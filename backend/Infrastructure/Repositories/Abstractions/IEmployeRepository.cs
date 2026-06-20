using Infrastructure.Models;

namespace Infrastructure.Repositories.Abstractions;

public interface IEmployeRepository
{
    Task<EmployeDb?> FindByIdUtilisateur(int idUtilisateur);
    Task Deactivate(int id);
}
