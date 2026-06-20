using Core.Models;

namespace Core.IGateways;

public interface IPlanningGateway
{
    Task<IEnumerable<HoraireAdmin>> GetHorairesByUtilisateur(int idUtilisateur);
}
