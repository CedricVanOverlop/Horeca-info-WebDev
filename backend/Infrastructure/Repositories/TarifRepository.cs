using Dapper;
using Infrastructure.Models;
using Infrastructure.Repositories.Abstractions;
using System.Data;

namespace Infrastructure.Repositories;

public class TarifRepository(IDbConnection connection) : ITarifRepository
{
}
