using GYM.API.Configurations;
using GYM.Application;
using GYM.Infrastructure;

namespace GYM.API;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddAuthorization();
        builder.Services.AddProblemDetails();
        builder.Services.AddHealthChecks();
        builder.Services
            .AddInfrastructure()
            .AddApplication()
            .ConfigureDatabase(builder)
            .ConfigureRedis(builder)
            .ConfigureSerilog(builder.Configuration)
            .ConfigureCors(builder)
            .ConfigureAuthentication(builder)
            .ConfigureRateLimitting(builder)
            .ConfigureResponseCompression()
            .ConfigureOpenApi(builder);

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        app.UseCors("GymClient");
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRateLimiter();
        app.MapHealthChecks("/health");

        app.Run();
    }
}