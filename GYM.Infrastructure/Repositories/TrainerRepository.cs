using GYM.Domain.Entities;
using GYM.Domain.Repositories;
using GYM.Domain.ValueObjects;
using GYM.Infrastructure.Abstractions;
using GYM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GYM.Infrastructure.Repositories;

internal sealed class TrainerRepository : Repository<Trainer>, ITrainerRepository
{
    private readonly GymDbContext _dbContext;

    public TrainerRepository(GymDbContext dbContext)
        : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Trainer?> GetByPersonIdAsync(Id personId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<Trainer>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.PersonId == personId, cancellationToken);
    }

    public Task<bool> ExistsByPersonIdAsync(Id personId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<Trainer>()
            .AnyAsync(t => t.PersonId == personId, cancellationToken);
    }
}
