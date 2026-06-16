using Core.IGateways;
using Core.Models;
using Infrastructure.Models;
using Infrastructure.Repositories.Abstractions;

namespace Infrastructure.Gateways;

public class PlanningGateway(IPlanningRepository planningRepository) : IPlanningGateway
{
    public async Task<IEnumerable<Planning>> GetAll()
    {
        var dbs = await planningRepository.GetAll();
        return dbs.Select(Map);
    }

    public async Task<IEnumerable<Planning>> GetByEmployeId(string employeId)
    {
        var dbs = await planningRepository.GetByEmployeId(employeId);
        return dbs.Select(Map);
    }

    public async Task<Planning> Create(Planning planning)
    {
        var db = new PlanningDb { EmployeId = planning.EmployeId, DateDebut = planning.DateDebut, DateFin = planning.DateFin };
        planning.Id = await planningRepository.Create(db);
        return planning;
    }

    public Task Delete(string id) => planningRepository.Delete(id);

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

    private static Planning Map(PlanningDb db) => new()
    { Id = db.Id, EmployeId = db.EmployeId, DateDebut = db.DateDebut, DateFin = db.DateFin };
}
