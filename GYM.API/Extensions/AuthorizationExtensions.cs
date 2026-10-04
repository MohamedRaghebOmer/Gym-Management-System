using GYM.API.Authorization.Handlers;
using GYM.API.Authorization.Policies;
using GYM.API.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace GYM.API.Extensions;

internal static class AuthorizationExtensions
{
    public static IServiceCollection AddApiAuthorization(
        this IServiceCollection services)
    {
        // =================== Gym Staff Policy ===================
        services
            .AddAuthorizationBuilder()
            .AddPolicy(Policies.GymStaff, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new GymStaffRequirement());
            });

        services.AddScoped<
            IAuthorizationHandler,
            GymStaffAuthorizationHandler>();


        // =================== Gym Admin Policy ===================
        services
            .AddAuthorizationBuilder()
            .AddPolicy(Policies.GymAdmin, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new GymAdminRequirement());
            });

        services.AddScoped<
            IAuthorizationHandler,
            GymAdminAuthorizationHandler>();


        // =================== Super Admin Policy ===================
        services
            .AddAuthorizationBuilder()
            .AddPolicy(Policies.SuperAdmin, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new SuperAdminRequirement());
            });

        services.AddScoped<
            IAuthorizationHandler,
            SuperAdminAuthorizationHandler>();

        return services;
    }
}