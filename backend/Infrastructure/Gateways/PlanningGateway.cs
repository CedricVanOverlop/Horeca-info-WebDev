using Core.IGateways;
using Core.Models;
using Infrastructure.Models;
using Infrastructure.Repositories.Abstractions;

namespace Infrastructure.Gateways;

public class PlanningGateway(IPlanningRepository planningRepository) : IPlanningGateway
{
    /// <summary>
    /// Retourne les horaires de travail d'un utilisateur employé, vue administrateur
    /// (avec le nom du commerce).
    /// </summary>
    /// <param name="idUtilisateur">Identifiant de l'utilisateur.</param>
    /// <returns>Les horaires de l'utilisateur.</returns>
    public async Task<IEnumerable<HoraireAdmin>> GetHorairesByUtilisateur(int idUtilisateur)
    {
        var dbs = await planningRepository.GetHorairesByUtilisateur(idUtilisateur);
        return dbs.Select(db => new HoraireAdmin
        {
            Id         = db.Id,
            Date       = db.Date,
            HeureDebut = db.HeureDebut,
            HeureFin   = db.HeureFin,
            HeurePayee = db.HeurePayee,
            Statut     = db.Statut,
            Commerce   = db.Commerce
        });
    }
}
