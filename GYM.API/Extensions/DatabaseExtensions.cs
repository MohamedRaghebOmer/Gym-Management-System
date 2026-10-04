using GYM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GYM.API.Extensions;

internal static class DatabaseExtensions
{
    public static IServiceCollection AddApiDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<GymDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"));

            options.AddInterceptors(new LoggingInterceptor());
        });

        return services;
    }

    public static async Task ApplyDatabaseMigrationsAsync(
        this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<GymDbContext>();

        await dbContext.Database.MigrateAsync();
    }
}