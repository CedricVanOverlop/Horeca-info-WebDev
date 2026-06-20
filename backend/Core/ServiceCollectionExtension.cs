using Core.UseCases;
using Core.UseCases.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Core;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddScoped<IUserUseCases, UserUseCases>();
        services.AddScoped<IPadelUseCases, PadelUseCases>();
        services.AddScoped<IPlanningUseCases, PlanningUseCases>();
        return services;
    }
}
