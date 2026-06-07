using Core.IGateways;

namespace Infrastructure.Gateways;

public class OdooGateway : IOdooGateway
{
    public Task<object?> GetData(string endpoint)
    {
        throw new NotImplementedException("Odoo integration not yet implemented.");
    }
}
