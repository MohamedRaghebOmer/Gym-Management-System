using GYM.Domain.Entities;
using GYM.Domain.Repositories;
using GYM.Infrastructure.Abstractions;
using GYM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GYM.Infrastructure.Repositories;

internal sealed class ExerciseRepository : Repository<Exercise>, IExerciseRepository
{
    private readonly GymDbContext _dbContext;

    public ExerciseRepository(GymDbContext dbContext)
        : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Exercise?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<Exercise>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Name == name, cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<Exercise>()
            .AnyAsync(e => e.Name == name, cancellationToken);
    }
}
