using GYM.API.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace GYM.API.Authorization.Handlers;

public sealed class SuperAdminAuthorizationHandler : AuthorizationHandler<SuperAdminRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SuperAdminRequirement requirement)
    {
        if (!context.User.IsInRole("SuperAdmin"))
        {
            return Task.CompletedTask;
        }

        if (context.Resource is not HttpContext httpContext)
        {
            return Task.CompletedTask;
        }

        context.Succeed(requirement);

        return Task.CompletedTask;

    }
}