using Serilog;

namespace GYM.API.Extensions;

internal static class SerilogExtensions
{
    public static WebApplicationBuilder ConfigureSerilog(
        this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.File(
                    Path.Combine("Logs", "log-.txt"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate:
                    "[{Timestamp:yyyy-MM-dd HH:mm:ss} " +
                    "{Level:u3}] {Message:lj}{NewLine}{Exception}");
        });

        return builder;
    }
}