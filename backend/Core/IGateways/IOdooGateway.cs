namespace Core.IGateways;

public interface IOdooGateway
{
    Task<object?> GetData(string endpoint);
}
