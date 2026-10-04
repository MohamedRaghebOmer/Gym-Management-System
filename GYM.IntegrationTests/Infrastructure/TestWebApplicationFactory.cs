using GYM.API;
using GYM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GYM.IntegrationTests.Infrastructure;

public sealed class TestWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public TestWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            // Remove the application's database registration.
            services.RemoveAll<DbContextOptions<GymDbContext>>();
            services.RemoveAll<GymDbContext>();

            // Replace it with the test PostgreSQL database.
            services.AddDbContext<GymDbContext>(options =>
            {
                options.UseNpgsql(_connectionString);
            });
        });
    }
}