namespace GYM.API.Extensions;

internal static class OpenApiExtensions
{
    public static IServiceCollection AddApiOpenApi(
        this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }
}