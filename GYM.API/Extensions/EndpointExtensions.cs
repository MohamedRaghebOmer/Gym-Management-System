using GYM.API.Endpoints;

namespace GYM.API.Extensions;

internal static class EndpointExtensions
{
    public static WebApplication MapApiEndpoints(
        this WebApplication app)
    {
        app.MapPeopleEndpoints();

        return app;
    }
}