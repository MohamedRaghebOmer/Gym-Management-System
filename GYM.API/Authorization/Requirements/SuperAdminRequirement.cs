using Microsoft.AspNetCore.Authorization;

namespace GYM.API.Authorization.Requirements;

public sealed class SuperAdminRequirement : IAuthorizationRequirement
{
}