using GYM.API.Configurations;
using GYM.Application;
using GYM.Infrastructure;

namespace GYM.API;

public class Program
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

        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild",
            "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        app.MapGet("/weatherforecast", () => Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = summaries[Random.Shared.Next(summaries.Length)]
                })
            .ToArray());

        app.Run();
    }
}

public record WeatherForecast
{
    public DateOnly Date { get; set; }
    public int TemperatureC { get; set; }
    public string Summary { get; set; } = string.Empty;
}