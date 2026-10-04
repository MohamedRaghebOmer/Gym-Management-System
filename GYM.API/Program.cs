using GYM.API.Extensions;

namespace GYM.API;

public partial class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.ConfigureSerilog();

        builder.Services.AddApiServices(builder.Configuration);

        var app = builder.Build();

        await app.ApplyDatabaseMigrationsAsync();

        app.UseApiPipeline();

        app.MapApiEndpoints();

        app.MapHealthChecks("/health");

        await app.RunAsync();
    }
}