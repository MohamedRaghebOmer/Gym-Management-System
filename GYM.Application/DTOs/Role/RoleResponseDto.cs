using GYM.Application.Abstractions;

namespace GYM.Application.DTOs.Role;

public sealed record RoleResponseDto(
    int Id,
    string Name,
    string? Description,
    DateTime CreatedAt)
    : IResponseDto<Domain.Entities.Role, RoleResponseDto>
{
    public static RoleResponseDto FromEntity(Domain.Entities.Role entity)
    {
        return new RoleResponseDto(
            Id: entity.Id.Value,
            Name: entity.Name,
            Description: entity.Description,
            CreatedAt: entity.CreatedAt);
    }
}
