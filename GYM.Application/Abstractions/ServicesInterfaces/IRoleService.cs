using GYM.Application.DTOs.Role;
using GYM.Domain.Entities;

namespace GYM.Application.Abstractions.ServicesInterfaces;

public interface IRoleService :
    IService<Role, RoleResponseDto, CreateRoleDto, UpdateRoleDto>
{
}
