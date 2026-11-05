using GYM.Application.Abstractions;

namespace GYM.Application.DTOs.Exercise;

public sealed record ExerciseResponseDto(
    int Id,
    int GymId,
    string Name,
    string MuscleGroup,
    string? Description,
    string? Instructions,
    string? VideoUrl,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
    : IResponseDto<Domain.Entities.Exercise, ExerciseResponseDto>
{
    public static ExerciseResponseDto FromEntity(Domain.Entities.Exercise entity)
    {
        return new ExerciseResponseDto(
            Id: entity.Id.Value,
            GymId: entity.GymId.Value,
            Name: entity.Name,
            MuscleGroup: entity.MuscleGroup,
            Description: entity.Description,
            Instructions: entity.Instructions,
            VideoUrl: entity.VideoUrl,
            IsActive: entity.IsActive,
            CreatedAt: entity.CreatedAt,
            UpdatedAt: entity.UpdatedAt);
    }
}
