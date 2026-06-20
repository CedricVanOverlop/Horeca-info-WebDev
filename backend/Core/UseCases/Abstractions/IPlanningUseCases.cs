using Core.Models;

namespace Core.UseCases.Abstractions;

public interface IPlanningUseCases
{
    Task<IEnumerable<HoraireAdmin>> GetHorairesAdmin(int userId);
}
