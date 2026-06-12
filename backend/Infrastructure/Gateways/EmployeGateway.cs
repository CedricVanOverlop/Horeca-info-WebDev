using Core.IGateways;
using Core.Models;
using Infrastructure.Models;
using Infrastructure.Repositories.Abstractions;

namespace Infrastructure.Gateways;

public class EmployeGateway(IEmployeRepository employeRepository) : IEmployeGateway
{
    /// <summary>
    /// Retourne tous les employés.
    /// </summary>
    /// <returns>Liste des employés.</returns>
    public async Task<IEnumerable<Employe>> GetAll()
    {
        var dbs = await employeRepository.GetAll();
        return dbs.Select(MapToCore);
    }

    /// <summary>
    /// Recherche un employé par son identifiant.
    /// </summary>
    /// <param name="id">Identifiant de l'employé.</param>
    /// <returns>L'employé correspondant, ou null.</returns>
    public async Task<Employe?> GetById(int id)
    {
        var db = await employeRepository.GetById(id);
        return db is null ? null : MapToCore(db);
    }

    /// <summary>
    /// Crée un nouvel employé.
    /// </summary>
    /// <param name="employe">Données de l'employé.</param>
    /// <returns>L'employé créé avec son identifiant généré.</returns>
    public async Task<Employe> Create(Employe employe)
    {
        employe.IdEmploye = await employeRepository.Create(MapToDb(employe));
        return employe;
    }

    /// <summary>
    /// Met à jour un employé existant.
    /// </summary>
    /// <param name="employe">Données mises à jour.</param>
    public Task Update(Employe employe) => employeRepository.Update(MapToDb(employe));

    /// <summary>
    /// Supprime un employé par son identifiant.
    /// </summary>
    /// <param name="id">Identifiant de l'employé.</param>
    public Task Delete(int id) => employeRepository.Delete(id);

    private static Employe MapToCore(EmployeDb db) => new()
    {
        IdEmploye            = db.IdEmploye,
        IdUtilisateur        = db.IdUtilisateur,
        Acces                = db.Acces,
        Actif                = db.Actif,
        IdCommercePreference = db.IdCommercePreference
    };

    private static EmployeDb MapToDb(Employe employe) => new()
    {
        IdEmploye            = employe.IdEmploye,
        IdUtilisateur        = employe.IdUtilisateur,
        Acces                = employe.Acces,
        Actif                = employe.Actif,
        IdCommercePreference = employe.IdCommercePreference
    };
}
