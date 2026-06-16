using Core.Models;

namespace Core.IGateways;

public interface IPlanningGateway
{
    Task<IEnumerable<Planning>> GetAll();
    Task<IEnumerable<Planning>> GetByEmployeId(string employeId);
    Task<Planning> Create(Planning planning);
    Task Delete(string id);
    Task<IEnumerable<HoraireAdmin>> GetHorairesByUtilisateur(int idUtilisateur);
}
