namespace GYM.API.Extensions;

internal static class CorsExtensions
{
    public static IServiceCollection AddApiCors(
        this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("GymClient", policy =>
            {
                policy
                    .WithOrigins(
                        "http://localhost:5246",
                        "https://localhost:7141",
                        "https://localhost:8080",
                        "https://localhost:8081",
                        "https://gym.mohamedragheb.dev")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }
}