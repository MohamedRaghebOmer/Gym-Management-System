using GYM.Application.Abstractions;
using GYM.Application.DTOs.Role;
using GYM.Domain.Entities;

namespace GYM.Application.ServicesInterfaces;

public interface IRoleService :
    IService<Role, RoleResponseDto, CreateRoleDto, UpdateRoleDto>
{
}
