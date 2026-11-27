using GYM.Application.Abstractions;

namespace GYM.Application.DTOs.WorkoutPlanExercise;

public sealed record WorkoutPlanExerciseResponseDto(
    int Id,
    int WorkoutPlanId,
    int ExerciseId,
    byte Sets,
    byte? Reps,
    decimal? Weight,
    short? RestSeconds,
    byte DisplayOrder,
    string? Notes)
    : IResponseDto<Domain.Entities.WorkoutPlanExercise, WorkoutPlanExerciseResponseDto>
{
    public static WorkoutPlanExerciseResponseDto FromEntity(Domain.Entities.WorkoutPlanExercise entity)
    {
        return new WorkoutPlanExerciseResponseDto(
            Id: entity.Id.Value,
            WorkoutPlanId: entity.WorkoutPlanId.Value,
            ExerciseId: entity.ExerciseId.Value,
            Sets: entity.Sets,
            Reps: entity.Reps,
            Weight: entity.Weight,
            RestSeconds: entity.RestSeconds,
            DisplayOrder: entity.DisplayOrder,
            Notes: entity.Notes);
    }
}
