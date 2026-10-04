namespace GYM.API.Extensions;

internal static class RedisExtensions
{
    public static IServiceCollection AddApiRedis(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration =
                configuration.GetConnectionString("Redis");

            options.InstanceName = "GYM:";
        });

        return services;
    }
}