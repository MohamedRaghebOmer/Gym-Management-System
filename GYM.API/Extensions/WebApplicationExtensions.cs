using Serilog;

namespace GYM.API.Extensions;

internal static class WebApplicationExtensions
{
    public static WebApplication UseApiPipeline(
        this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        app.UseHttpsRedirection();

        app.UseResponseCompression();

        app.UseCors("GymClient");

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseRateLimiter();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        return app;
    }
}