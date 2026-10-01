using GYM.Domain.Abstractions;
using GYM.Infrastructure.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace GYM.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        return services;
    }
}