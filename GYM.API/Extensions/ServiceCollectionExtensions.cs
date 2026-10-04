using GYM.API.Authorization;
using GYM.Application;
using GYM.Infrastructure;

namespace GYM.API.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddApplication()
            .AddInfrastructure()
            .AddApiAuthentication(configuration)
            .AddApiAuthorization()
            .AddApiDatabase(configuration)
            .AddApiCors()
            .AddApiRateLimiting()
            .AddApiOpenApi()
            .AddResponseCompression()
            .AddProblemDetails()
            .AddHealthChecks();

        return services;
    }
}