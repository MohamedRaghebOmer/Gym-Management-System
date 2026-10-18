using GYM.Application.Abstractions;

namespace GYM.Application.DTOs.Gym;

public sealed record GymResponseDto(
    int Id,
    string Name,
    DateTime CreatedAt)
    : IResponseDto<Domain.Entities.Gym, GymResponseDto>
{
    public static GymResponseDto FromEntity(Domain.Entities.Gym entity)
    {
        return new GymResponseDto(
            Id: entity.Id.Value,
            Name: entity.Name,
            CreatedAt: entity.CreatedAt);
    }
}
