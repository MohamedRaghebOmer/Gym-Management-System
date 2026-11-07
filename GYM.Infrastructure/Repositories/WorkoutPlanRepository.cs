using GYM.Domain.Entities;
using GYM.Domain.Repositories;
using GYM.Infrastructure.Abstractions;
using GYM.Infrastructure.Persistence;

namespace GYM.Infrastructure.Repositories;

public sealed class WorkoutPlanRepository : Repository<WorkoutPlan>, IWorkoutPlanRepository
{
    private readonly GymDbContext _dbContext;

    public WorkoutPlanRepository(GymDbContext dbContext)
        : base(dbContext)
    {
        _dbContext = dbContext;
    }
}