using GYM.Application.Abstractions;

namespace GYM.Application.DTOs.User;

public sealed record UserResponseDto(
    int Id,
    int PersonId,
    string Username,
    int RoleId,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
    : IResponseDto<Domain.Entities.User, UserResponseDto>
{
    public static UserResponseDto FromEntity(Domain.Entities.User entity)
    {
        return new UserResponseDto(
            Id: entity.Id.Value,
            PersonId: entity.PersonId.Value,
            Username: entity.Username,
            RoleId: entity.RoleId.Value,
            IsActive: entity.IsActive,
            CreatedAt: entity.CreatedAt,
            UpdatedAt: entity.UpdatedAt);
    }
}
