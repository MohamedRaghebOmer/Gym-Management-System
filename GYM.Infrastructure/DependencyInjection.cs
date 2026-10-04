using GYM.Domain.Abstractions;
using GYM.Domain.Repositories;
using GYM.Infrastructure.Abstractions;
using GYM.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace GYM.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPersonRepository, PersonRepository>();

        return services;
    }
}