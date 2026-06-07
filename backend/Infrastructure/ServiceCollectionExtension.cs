using Core.IGateways;
using Infrastructure.Gateways;
using Infrastructure.Repositories;
using Infrastructure.Repositories.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using System.Data;

namespace Infrastructure;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddScoped<IDbConnection>(_ => new MySqlConnection(connectionString));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEmployeRepository, EmployeRepository>();
        services.AddScoped<IFideliteRepository, FideliteRepository>();
        services.AddScoped<IPlanningRepository, PlanningRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<ITerrainRepository, TerrainRepository>();

        services.AddScoped<IUserGateway, UserGateway>();
        services.AddScoped<IEmployeGateway, EmployeGateway>();
        services.AddScoped<IFideliteGateway, FideliteGateway>();
        services.AddScoped<IOdooGateway, OdooGateway>();
        services.AddScoped<IPlanningGateway, PlanningGateway>();
        services.AddScoped<IReservationGateway, ReservationGateway>();
        services.AddScoped<ITerrainGateway, TerrainGateway>();

        return services;
    }
}
