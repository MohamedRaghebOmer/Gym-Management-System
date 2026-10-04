using GYM.Application.Abstractions.ServicesInterfaces;
using GYM.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GYM.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IPersonService, PersonService>();

        return services;
    }
}