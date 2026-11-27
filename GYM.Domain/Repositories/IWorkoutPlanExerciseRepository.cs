using GYM.Domain.Abstractions;
using GYM.Domain.Entities;
using GYM.Domain.ValueObjects;

namespace GYM.Domain.Repositories;

public interface IWorkoutPlanExerciseRepository : IRepository<WorkoutPlanExercise>
{
    Task<bool> ExistsAsync(
        Id workoutPlanId,
        Id exerciseId,
        CancellationToken cancellationToken = default);
}
