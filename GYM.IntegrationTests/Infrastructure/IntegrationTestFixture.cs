using GYM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace GYM.IntegrationTests.Infrastructure;

public sealed class IntegrationTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres;
    private Respawner _respawner = null!;

    public TestWebApplicationFactory Factory { get; private set; } = null!;
    public HttpClient Client { get; private set; } = null!;
    public string ConnectionString =>
        _postgres.GetConnectionString();

    public IntegrationTestFixture()
    {
        _postgres = new PostgreSqlBuilder("postgres:18.6")
            .WithDatabase("gym_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }

    public async Task InitializeAsync()
    {
        // 1. Start PostgreSQL container.
        await _postgres.StartAsync();

        // 2. Create the ASP.NET Core test application.
        Factory = new TestWebApplicationFactory(ConnectionString);

        // 3. Create HttpClient.
        Client = Factory.CreateClient();

        // 4. Run EF Core migrations.
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var dbContext =
                scope.ServiceProvider.GetRequiredService<GymDbContext>();

            await dbContext.Database.MigrateAsync();
        }

        // 5. Initialize Respawn.
        await using var connection =
            new NpgsqlConnection(ConnectionString);

        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(
            connection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["public"]
            });
    }

    public async Task ResetDatabaseAsync()
    {
        await using var connection =
            new NpgsqlConnection(ConnectionString);

        await connection.OpenAsync();

        await _respawner.ResetAsync(connection);
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await Factory.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    public async Task<T> ExecuteDbAsync<T>(
        Func<NpgsqlConnection, Task<T>> action)
    {
        await using var connection =
            new NpgsqlConnection(ConnectionString);

        await connection.OpenAsync();

        return await action(connection);
    }

    public async Task SeedAsync(
        Func<GymDbContext, Task> action)
    {
        await using var scope =
            Factory.Services.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<GymDbContext>();

        await action(dbContext);

        await dbContext.SaveChangesAsync();
    }
}