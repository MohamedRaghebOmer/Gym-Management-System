using GYM.Domain.Entities;
using GYM.Domain.Repositories;
using GYM.Domain.ValueObjects;
using GYM.Infrastructure.Abstractions;
using GYM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GYM.Infrastructure.Repositories;

internal sealed class MemberRepository : Repository<Member>, IMemberRepository
{
    private readonly GymDbContext _dbContext;

    public MemberRepository(GymDbContext dbContext)
        : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Member?> GetByPersonIdAsync(
        Id personId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<Member>()
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.PersonId == personId, cancellationToken);
    }

    public Task<Member?> GetByTrainerIdAsync(
        Id trainerId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<Member>()
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.TrainerId == trainerId, cancellationToken);
    }

    public Task<bool> ExistsByPersonIdAsync(
        Id personId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<Member>()
            .AnyAsync(m => m.PersonId == personId, cancellationToken);
    }

    public Task<bool> ExistsByTrainerIdAsync(
        Id trainerId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<Member>()
            .AnyAsync(m => m.TrainerId == trainerId, cancellationToken);
    }
}
