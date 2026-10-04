using GYM.API.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace GYM.API.Authorization.Handlers;

internal sealed class GymStaffAuthorizationHandler : AuthorizationHandler<GymStaffRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        GymStaffRequirement requirement)
    {
        if (!context.User.IsInRole("Admin") &&
            !context.User.IsInRole("Trainer"))
        {
            return Task.CompletedTask;
        }

        if (context.Resource is not HttpContext httpContext)
        {
            return Task.CompletedTask;
        }

        if (!int.TryParse(
                httpContext.Request.RouteValues["gymId"]?.ToString(),
                out var targetGymId))
        {
            return Task.CompletedTask;
        }

        var userGymId = context.User.FindFirst("gym_id")?.Value;

        if (!int.TryParse(userGymId, out var currentGymId))
        {
            return Task.CompletedTask;
        }

        if (currentGymId == targetGymId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}