using GYM.Application.Abstractions.ServicesInterfaces;
using GYM.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GYM.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IPersonService, PersonService>();
        services.AddScoped<ITrainerService, TrainerService>();
        services.AddScoped<IMemberService, MemberService>();
        services.AddScoped<IMembershipPlanService, MembershipPlanService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IGymService, GymService>();

        return services;
    }
}