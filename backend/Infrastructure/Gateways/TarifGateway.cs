using Core.IGateways;
using Core.Models;
using Infrastructure.Models;
using Infrastructure.Repositories.Abstractions;

namespace Infrastructure.Gateways;

public class TarifGateway(ITarifRepository tarifRepository) : ITarifGateway
{
}
