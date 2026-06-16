using Core.Models;

namespace Core.UseCases.Abstractions;

public interface IPlanningUseCases
{
    Task<IEnumerable<Planning>> GetAll();
    Task<IEnumerable<Planning>> GetByEmploye(string employeId);
    Task<Planning> Create(Planning planning);
    Task Delete(string id);
    Task<IEnumerable<HoraireAdmin>> GetHorairesAdmin(int userId);
}
