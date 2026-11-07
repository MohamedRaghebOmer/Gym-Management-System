using GYM.Application.Abstractions;

namespace GYM.Application.DTOs.WorkoutPlan;

public sealed record WorkoutResponseDto(
    int Id,
    int MemberId,
    int TrainerId,
    string Name,
    string? Description,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt) : IResponseDto<Domain.Entities.WorkoutPlan, WorkoutResponseDto>
{
    public static WorkoutResponseDto FromEntity(Domain.Entities.WorkoutPlan entity)
    {
        return new WorkoutResponseDto(
            Id: entity.Id.Value,
            MemberId: entity.MemberId.Value,
            TrainerId: entity.TrainerId.Value,
            Name: entity.Name,
            Description: entity.Description,
            StartDate: entity.StartDate,
            EndDate: entity.EndDate,
            IsActive: entity.IsActive,
            CreatedAt: entity.CreatedAt,
            UpdatedAt: entity.UpdatedAt
        );
    }
}