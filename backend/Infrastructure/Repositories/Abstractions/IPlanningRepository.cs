using Infrastructure.Models;

namespace Infrastructure.Repositories.Abstractions;

public interface IPlanningRepository
{
    Task<IEnumerable<HoraireAdminDb>> GetHorairesByUtilisateur(int idUtilisateur);
}
