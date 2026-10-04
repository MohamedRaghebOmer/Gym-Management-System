using GYM.API.Authorization.Policies;

namespace GYM.API.Endpoints;

internal static class PeopleEndpoints
{
    public static IEndpointRouteBuilder MapPeopleEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("gyms/{gymId:int}/people")
            .RequireAuthorization(Policies.GymStaff);


        return app;
    }
}