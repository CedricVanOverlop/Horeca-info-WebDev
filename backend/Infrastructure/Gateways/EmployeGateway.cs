using Core.IGateways;
using Core.Models;
using Infrastructure.Repositories.Abstractions;

namespace Infrastructure.Gateways;

public class EmployeGateway(IEmployeRepository employeRepository) : IEmployeGateway
{
    public async Task<IEnumerable<Employe>> GetAll()
    {
        var dbs = await employeRepository.GetAll();
        return dbs.Select(db => new Employe { Id = db.Id, Nom = db.Nom, Prenom = db.Prenom, Email = db.Email, Poste = db.Poste });
    }

    public async Task<Employe?> GetById(string id)
    {
        var db = await employeRepository.GetById(id);
        if (db is null) return null;
        return new Employe { Id = db.Id, Nom = db.Nom, Prenom = db.Prenom, Email = db.Email, Poste = db.Poste };
    }

    public async Task<Employe> Create(Employe employe)
    {
        employe.Id = await employeRepository.Create(employe);
        return employe;
    }

    public Task Update(Employe employe) => employeRepository.Update(employe);

    public Task Delete(string id) => employeRepository.Delete(id);
}
