using GYM.Domain.Entities;
using GYM.Domain.Repositories;
using GYM.Domain.ValueObjects;
using GYM.Infrastructure.Abstractions;
using GYM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GYM.Infrastructure.Repositories;

internal sealed class WorkoutPlanExerciseRepository
    : Repository<WorkoutPlanExercise>, IWorkoutPlanExerciseRepository
{
    private readonly GymDbContext _dbContext;

    public WorkoutPlanExerciseRepository(GymDbContext dbContext)
        : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsAsync(
        Id workoutPlanId,
        Id exerciseId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<WorkoutPlanExercise>()
            .AnyAsync(
                wpe => wpe.WorkoutPlanId == workoutPlanId && wpe.ExerciseId == exerciseId,
                cancellationToken);
    }
}
